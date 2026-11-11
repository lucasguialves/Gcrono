using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.Servicos;

public class LeituraProcessadaEventArgs : EventArgs
{
    public LeituraEvento Leitura { get; }
    public Inscricao? Inscricao { get; }

    public LeituraProcessadaEventArgs(LeituraEvento leitura, Inscricao? inscricao)
    {
        Leitura = leitura;
        Inscricao = inscricao;
    }
}

/// <summary>
/// Ponto único por onde toda leitura de chip (venha do leitor real ou do simulador) é gravada.
/// Não calcula/armazena resultado aqui — isso é feito sob demanda por ServicoResultados a partir
/// da tabela de leituras, para nunca ficar dessincronizado se uma leitura for corrigida.
/// </summary>
public class ServicoCronometragem
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;

    public event EventHandler<LeituraProcessadaEventArgs>? LeituraProcessada;

    public ServicoCronometragem(IDbContextFactory<CronometragemDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task RegistrarLeituraAsync(
        int provaId, PontoControle pontoControle, string chipId, DateTime dataHoraLeitura, string origemLeitor)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var leitura = new LeituraEvento
        {
            ProvaId = provaId,
            ChipId = chipId,
            PontoControle = pontoControle,
            DataHoraLeitura = dataHoraLeitura,
            OrigemLeitor = origemLeitor
        };

        db.Leituras.Add(leitura);
        await db.SaveChangesAsync();

        var inscricao = await db.Inscricoes.FirstOrDefaultAsync(i =>
            i.ProvaId == provaId && i.ChipId == chipId && i.Status == StatusInscricao.Ativa);

        LeituraProcessada?.Invoke(this, new LeituraProcessadaEventArgs(leitura, inscricao));
    }
}
