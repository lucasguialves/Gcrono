using Cronometragem.Core.Cronometragem;
using Cronometragem.Core.Entidades;
using Xunit;

namespace Cronometragem.Core.Tests;

public class CalculadoraTempoTests
{
    private static Prova CriarProva(TimeOnly horaLargada) => new()
    {
        Id = 1,
        Nome = "Prova Teste",
        DataProva = new DateOnly(2026, 11, 11),
        HoraLargadaOficial = horaLargada
    };

    private static Inscricao CriarInscricao(string chipId = "CHIP-001") => new()
    {
        Id = 10,
        ProvaId = 1,
        AtletaId = 100,
        NumeroPeito = "42",
        ChipId = chipId
    };

    private static LeituraEvento Leitura(string chipId, PontoControle ponto, DateTime quando) => new()
    {
        ProvaId = 1,
        ChipId = chipId,
        PontoControle = ponto,
        DataHoraLeitura = quando
    };

    [Fact]
    public void Calcular_SemNenhumaLeitura_RetornaNaoLargou()
    {
        var prova = CriarProva(new TimeOnly(8, 0));
        var inscricao = CriarInscricao();
        var calculadora = new CalculadoraTempo();

        var resultado = calculadora.Calcular(prova, inscricao, Enumerable.Empty<LeituraEvento>());

        Assert.Equal(SituacaoResultado.NaoLargou, resultado.Situacao);
        Assert.Null(resultado.TempoLargada);
        Assert.Null(resultado.TempoChegada);
        Assert.Null(resultado.TempoBruto);
        Assert.Null(resultado.TempoLiquido);
    }

    [Fact]
    public void Calcular_SoComLargada_RetornaEmProva()
    {
        var prova = CriarProva(new TimeOnly(8, 0));
        var inscricao = CriarInscricao();
        var largada = new DateTime(2026, 11, 11, 8, 0, 5);
        var leituras = new[] { Leitura(inscricao.ChipId, PontoControle.Largada, largada) };
        var calculadora = new CalculadoraTempo();

        var resultado = calculadora.Calcular(prova, inscricao, leituras);

        Assert.Equal(SituacaoResultado.EmProva, resultado.Situacao);
        Assert.Equal(largada, resultado.TempoLargada);
        Assert.Null(resultado.TempoChegada);
        Assert.Null(resultado.TempoBruto);
        Assert.Null(resultado.TempoLiquido);
    }

    [Fact]
    public void Calcular_ComLargadaEChegada_CalculaTempoBrutoELiquido()
    {
        var prova = CriarProva(new TimeOnly(8, 0, 0));
        var inscricao = CriarInscricao();

        // Chip cruzou a largada 5s depois do tiro oficial (fila de largada), e chegou 1h depois.
        var largada = new DateTime(2026, 11, 11, 8, 0, 5);
        var chegada = new DateTime(2026, 11, 11, 9, 0, 5);

        var leituras = new[]
        {
            Leitura(inscricao.ChipId, PontoControle.Largada, largada),
            Leitura(inscricao.ChipId, PontoControle.Chegada, chegada),
        };

        var resultado = new CalculadoraTempo().Calcular(prova, inscricao, leituras);

        Assert.Equal(SituacaoResultado.Finalizado, resultado.Situacao);
        // Tempo bruto: do tiro oficial (8:00:00) até a chegada (9:00:05) = 1h00m05s
        Assert.Equal(TimeSpan.FromSeconds(3605), resultado.TempoBruto);
        // Tempo líquido: da largada do próprio chip (8:00:05) até a chegada (9:00:05) = 1h00m00s
        Assert.Equal(TimeSpan.FromHours(1), resultado.TempoLiquido);
    }

    [Fact]
    public void Calcular_ComChegadaSemLargada_CalculaSoTempoBruto()
    {
        var prova = CriarProva(new TimeOnly(8, 0, 0));
        var inscricao = CriarInscricao();
        var chegada = new DateTime(2026, 11, 11, 9, 0, 0);

        var leituras = new[] { Leitura(inscricao.ChipId, PontoControle.Chegada, chegada) };

        var resultado = new CalculadoraTempo().Calcular(prova, inscricao, leituras);

        Assert.Equal(SituacaoResultado.Finalizado, resultado.Situacao);
        Assert.Equal(TimeSpan.FromHours(1), resultado.TempoBruto);
        Assert.Null(resultado.TempoLiquido);
    }

    [Fact]
    public void Calcular_IgnoraLeiturasDeOutroChip()
    {
        var prova = CriarProva(new TimeOnly(8, 0));
        var inscricao = CriarInscricao("CHIP-001");
        var chegada = new DateTime(2026, 11, 11, 9, 0, 0);

        var leituras = new[]
        {
            Leitura("CHIP-999", PontoControle.Chegada, new DateTime(2026, 11, 11, 8, 30, 0)),
            Leitura(inscricao.ChipId, PontoControle.Chegada, chegada),
        };

        var resultado = new CalculadoraTempo().Calcular(prova, inscricao, leituras);

        Assert.Equal(chegada, resultado.TempoChegada);
    }

    [Fact]
    public void Calcular_LeiturasDuplicadasDentroDaJanela_UsaAPrimeiraComoPassagemUnica()
    {
        var prova = CriarProva(new TimeOnly(8, 0));
        var inscricao = CriarInscricao();
        var primeiraPassagem = new DateTime(2026, 11, 11, 9, 0, 0);

        // Antena captura o mesmo chip 3 vezes na mesma passagem, a 1s e 3s de distância.
        var leituras = new[]
        {
            Leitura(inscricao.ChipId, PontoControle.Chegada, primeiraPassagem),
            Leitura(inscricao.ChipId, PontoControle.Chegada, primeiraPassagem.AddSeconds(1)),
            Leitura(inscricao.ChipId, PontoControle.Chegada, primeiraPassagem.AddSeconds(3)),
        };

        var resultado = new CalculadoraTempo(TimeSpan.FromSeconds(5)).Calcular(prova, inscricao, leituras);

        Assert.Equal(primeiraPassagem, resultado.TempoChegada);
    }

    [Fact]
    public void DebouncePassagens_LeiturasForaDaJanela_SaoPassagensDistintas()
    {
        var baseHora = new DateTime(2026, 11, 11, 9, 0, 0);
        var leituras = new[]
        {
            new LeituraEvento { ChipId = "C1", PontoControle = PontoControle.Chegada, DataHoraLeitura = baseHora },
            new LeituraEvento { ChipId = "C1", PontoControle = PontoControle.Chegada, DataHoraLeitura = baseHora.AddSeconds(2) },
            // Passagem distinta: 20s depois da última leitura, fora da janela de 5s.
            new LeituraEvento { ChipId = "C1", PontoControle = PontoControle.Chegada, DataHoraLeitura = baseHora.AddSeconds(22) },
        };

        var calculadora = new CalculadoraTempo(TimeSpan.FromSeconds(5));
        var passagens = calculadora.DebouncePassagens(leituras);

        Assert.Equal(2, passagens.Count);
        Assert.Equal(baseHora, passagens[0].DataHoraLeitura);
        Assert.Equal(baseHora.AddSeconds(22), passagens[1].DataHoraLeitura);
    }
}
