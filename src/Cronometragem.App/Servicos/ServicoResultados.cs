using Cronometragem.Core.Cronometragem;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.Servicos;

public record ResultadoExibicao(
    ResultadoAtleta Resultado,
    string NomeAtleta,
    string NomeCategoria,
    string NomeModalidade,
    int? PosicaoGeral,
    int? PosicaoCategoria);

/// <summary>
/// Calcula o ranking da prova sob demanda, lendo as leituras brutas gravadas até o momento.
/// Nenhum tempo fica armazenado — reflete sempre o estado atual de `leituras`.
/// </summary>
public class ServicoResultados
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;
    private readonly ICalculadoraTempo _calculadora;

    public ServicoResultados(IDbContextFactory<CronometragemDbContext> dbFactory, ICalculadoraTempo calculadora)
    {
        _dbFactory = dbFactory;
        _calculadora = calculadora;
    }

    public async Task<List<ResultadoExibicao>> ObterResultadosAsync(int provaId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var prova = await db.Provas.FirstAsync(p => p.Id == provaId);

        var inscricoes = await db.Inscricoes
            .Where(i => i.ProvaId == provaId && i.Status == StatusInscricao.Ativa)
            .ToListAsync();

        var atletasPorId = await db.Atletas.ToDictionaryAsync(a => a.Id);
        var categoriasPorId = await db.Categorias
            .Where(c => c.ProvaId == provaId)
            .ToDictionaryAsync(c => c.Id);
        var modalidadesPorId = await db.Modalidades
            .Where(m => m.ProvaId == provaId)
            .ToDictionaryAsync(m => m.Id);

        var leiturasPorChip = (await db.Leituras.Where(l => l.ProvaId == provaId).ToListAsync())
            .ToLookup(l => l.ChipId);

        var brutos = inscricoes.Select(inscricao =>
        {
            var resultado = _calculadora.Calcular(prova, inscricao, leiturasPorChip[inscricao.ChipId]);
            var nomeAtleta = atletasPorId.TryGetValue(inscricao.AtletaId, out var atleta) ? atleta.Nome : "(desconhecido)";
            var nomeCategoria = inscricao.CategoriaId.HasValue
                && categoriasPorId.TryGetValue(inscricao.CategoriaId.Value, out var categoria)
                    ? categoria.Nome
                    : "-";
            var nomeModalidade = inscricao.ModalidadeId.HasValue
                && modalidadesPorId.TryGetValue(inscricao.ModalidadeId.Value, out var modalidade)
                    ? modalidade.Nome
                    : "-";

            return (resultado, nomeAtleta, nomeCategoria, nomeModalidade);
        }).ToList();

        // Posição só existe pra quem finalizou (tem tempo). Ranking sempre pelo TEMPO BRUTO
        // (desde o tiro oficial da largada) — é o mesmo critério pra todo mundo, disputem ou não
        // a largada individual; comparar tempo líquido de um atleta com bruto de outro não seria
        // justo, já que líquido tende a ser menor.
        var finalizadosOrdenados = brutos
            .Where(b => b.resultado.Situacao == SituacaoResultado.Finalizado)
            .OrderBy(b => b.resultado.TempoBruto)
            .ToList();

        var posicaoGeralPorInscricao = finalizadosOrdenados
            .Select((b, indice) => (b.resultado.InscricaoId, Posicao: indice + 1))
            .ToDictionary(x => x.InscricaoId, x => x.Posicao);

        var posicaoCategoriaPorInscricao = finalizadosOrdenados
            .GroupBy(b => b.nomeCategoria)
            .SelectMany(grupo => grupo
                .OrderBy(b => b.resultado.TempoBruto)
                .Select((b, indice) => (b.resultado.InscricaoId, Posicao: indice + 1)))
            .ToDictionary(x => x.InscricaoId, x => x.Posicao);

        var resultados = brutos.Select(b => new ResultadoExibicao(
            b.resultado,
            b.nomeAtleta,
            b.nomeCategoria,
            b.nomeModalidade,
            posicaoGeralPorInscricao.TryGetValue(b.resultado.InscricaoId, out var pg) ? pg : null,
            posicaoCategoriaPorInscricao.TryGetValue(b.resultado.InscricaoId, out var pc) ? pc : null));

        return resultados
            .OrderBy(r => r.Resultado.Situacao != SituacaoResultado.Finalizado)
            .ThenBy(r => r.Resultado.TempoBruto ?? TimeSpan.MaxValue)
            .ToList();
    }
}
