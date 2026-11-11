using Cronometragem.Core.Entidades;

namespace Cronometragem.Core.Cronometragem;

public class CalculadoraTempo : ICalculadoraTempo
{
    private readonly TimeSpan _janelaDebounce;

    public CalculadoraTempo(TimeSpan? janelaDebounce = null)
    {
        _janelaDebounce = janelaDebounce ?? TimeSpan.FromSeconds(5);
    }

    public ResultadoAtleta Calcular(Prova prova, Inscricao inscricao, IEnumerable<LeituraEvento> leiturasDoChip)
    {
        var porPonto = leiturasDoChip
            .Where(l => l.ChipId == inscricao.ChipId)
            .GroupBy(l => l.PontoControle)
            .ToDictionary(
                g => g.Key,
                g => DebouncePassagens(g.OrderBy(l => l.DataHoraLeitura)).FirstOrDefault());

        porPonto.TryGetValue(PontoControle.Largada, out var leituraLargada);
        porPonto.TryGetValue(PontoControle.Chegada, out var leituraChegada);

        var resultado = new ResultadoAtleta
        {
            InscricaoId = inscricao.Id,
            NumeroPeito = inscricao.NumeroPeito,
            ChipId = inscricao.ChipId,
            TempoLargada = leituraLargada?.DataHoraLeitura,
            TempoChegada = leituraChegada?.DataHoraLeitura,
        };

        if (resultado.TempoChegada is { } chegada)
        {
            resultado.TempoBruto = chegada - prova.LargadaOficialEm;

            if (resultado.TempoLargada is { } largada)
                resultado.TempoLiquido = chegada - largada;
        }

        resultado.Situacao = resultado.TempoChegada is not null
            ? SituacaoResultado.Finalizado
            : resultado.TempoLargada is not null
                ? SituacaoResultado.EmProva
                : SituacaoResultado.NaoLargou;

        return resultado;
    }

    /// <summary>
    /// Colapsa leituras consecutivas (já ordenadas por data/hora) que ocorrem dentro da janela de
    /// debounce em uma única passagem — uma antena costuma capturar o mesmo chip várias vezes na
    /// mesma passagem pela linha de controle. Cada passagem resultante é representada pela
    /// primeira leitura do seu grupo.
    /// </summary>
    public IReadOnlyList<LeituraEvento> DebouncePassagens(IEnumerable<LeituraEvento> leiturasOrdenadas)
    {
        var passagens = new List<LeituraEvento>();
        LeituraEvento? ultimaLeitura = null;

        foreach (var leitura in leiturasOrdenadas)
        {
            var iniciaNovaPassagem = ultimaLeitura is null
                || leitura.DataHoraLeitura - ultimaLeitura.DataHoraLeitura > _janelaDebounce;

            if (iniciaNovaPassagem)
                passagens.Add(leitura);

            ultimaLeitura = leitura;
        }

        return passagens;
    }
}
