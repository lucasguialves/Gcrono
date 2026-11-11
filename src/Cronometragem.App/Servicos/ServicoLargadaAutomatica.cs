using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.Servicos;

/// <summary>
/// Suporta o fluxo de "um leitor só" para largada e chegada: toda leitura de um chip que ainda
/// não tem largada confirmada nesta prova fica pendente em memória (não grava em `leituras`
/// ainda). Quando o operador chama ConfirmarLargadasAsync, todas as leituras pendentes da prova
/// viram oficialmente a largada de cada atleta. A partir daí, qualquer nova leitura do mesmo
/// chip é detectada como chegada automaticamente (porque a largada já existe no banco).
/// </summary>
public class ServicoLargadaAutomatica
{
    private readonly ServicoCronometragem _servicoCronometragem;
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;

    private readonly Dictionary<(int ProvaId, string ChipId), (DateTime DataHora, string OrigemLeitor)> _pendentes = new();

    public event EventHandler? PendentesAlterados;

    public ServicoLargadaAutomatica(ServicoCronometragem servicoCronometragem, IDbContextFactory<CronometragemDbContext> dbFactory)
    {
        _servicoCronometragem = servicoCronometragem;
        _dbFactory = dbFactory;
    }

    public IReadOnlyCollection<string> ChipsPendentes(int provaId) =>
        _pendentes.Keys.Where(k => k.ProvaId == provaId).Select(k => k.ChipId).ToList();

    public async Task ProcessarLeituraAsync(int provaId, string chipId, DateTime dataHoraLeitura, string origemLeitor)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var jaTemLargada = await db.Leituras.AnyAsync(l =>
            l.ProvaId == provaId && l.ChipId == chipId && l.PontoControle == PontoControle.Largada);

        if (jaTemLargada)
        {
            // Largada já confirmada antes: esta leitura é a chegada do atleta.
            await _servicoCronometragem.RegistrarLeituraAsync(provaId, PontoControle.Chegada, chipId, dataHoraLeitura, origemLeitor);
            return;
        }

        var chave = (provaId, chipId);
        if (!_pendentes.ContainsKey(chave))
        {
            // Guarda a primeira leitura pendente do chip; confirmações futuras usam esse horário.
            _pendentes[chave] = (dataHoraLeitura, origemLeitor);
            PendentesAlterados?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Confirma como largada todas as leituras pendentes da prova. Retorna quantas foram confirmadas.</summary>
    public async Task<int> ConfirmarLargadasAsync(int provaId)
    {
        var pendentesDaProva = _pendentes.Where(kv => kv.Key.ProvaId == provaId).ToList();

        foreach (var (chave, leitura) in pendentesDaProva)
        {
            await _servicoCronometragem.RegistrarLeituraAsync(
                provaId, PontoControle.Largada, chave.ChipId, leitura.DataHora, leitura.OrigemLeitor);
            _pendentes.Remove(chave);
        }

        PendentesAlterados?.Invoke(this, EventArgs.Empty);
        return pendentesDaProva.Count;
    }
}
