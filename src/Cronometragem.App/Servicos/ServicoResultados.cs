using Cronometragem.Core.Cronometragem;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.Servicos;

public record ResultadoExibicao(ResultadoAtleta Resultado, string NomeAtleta, string NomeCategoria);

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

        var leiturasPorChip = (await db.Leituras.Where(l => l.ProvaId == provaId).ToListAsync())
            .ToLookup(l => l.ChipId);

        var resultados = inscricoes.Select(inscricao =>
        {
            var resultado = _calculadora.Calcular(prova, inscricao, leiturasPorChip[inscricao.ChipId]);
            var nomeAtleta = atletasPorId.TryGetValue(inscricao.AtletaId, out var atleta) ? atleta.Nome : "(desconhecido)";
            var nomeCategoria = inscricao.CategoriaId.HasValue
                && categoriasPorId.TryGetValue(inscricao.CategoriaId.Value, out var categoria)
                    ? categoria.Nome
                    : "-";

            return new ResultadoExibicao(resultado, nomeAtleta, nomeCategoria);
        });

        return resultados
            .OrderBy(r => r.Resultado.Situacao != SituacaoResultado.Finalizado)
            .ThenBy(r => r.Resultado.TempoLiquido ?? r.Resultado.TempoBruto ?? TimeSpan.MaxValue)
            .ToList();
    }
}
