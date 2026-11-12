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

    /// <summary>
    /// Registra uma leitura, a menos que esse chip já tenha uma leitura desse mesmo ponto de
    /// controle nessa prova — só a primeira largada e a primeira chegada de cada chip contam;
    /// leituras repetidas (reader pegando o mesmo chip de novo, duplo clique, etc.) são
    /// ignoradas. Retorna false quando a leitura foi rejeitada por já existir.
    /// </summary>
    public async Task<bool> RegistrarLeituraAsync(
        int provaId, PontoControle pontoControle, string chipId, DateTime dataHoraLeitura, string origemLeitor)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var jaRegistrada = await db.Leituras.AnyAsync(l =>
            l.ProvaId == provaId && l.ChipId == chipId && l.PontoControle == pontoControle);

        if (jaRegistrada)
            return false;

        var leitura = new LeituraEvento
        {
            ProvaId = provaId,
            ChipId = chipId,
            PontoControle = pontoControle,
            DataHoraLeitura = dataHoraLeitura,
            OrigemLeitor = origemLeitor
        };

        db.Leituras.Add(leitura);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Duas leituras do mesmo chip/ponto quase simultâneas (reader + manual, por exemplo)
            // podem passar pela checagem acima antes de qualquer uma gravar; a constraint única
            // do banco pega esse caso raro e rejeita a segunda, igual à checagem normal.
            return false;
        }

        var inscricao = await db.Inscricoes.FirstOrDefaultAsync(i =>
            i.ProvaId == provaId && i.ChipId == chipId && i.Status == StatusInscricao.Ativa);

        LeituraProcessada?.Invoke(this, new LeituraProcessadaEventArgs(leitura, inscricao));
        return true;
    }

    /// <summary>
    /// Apaga todas as leituras (largada e chegada) de todos os participantes da prova — não
    /// mexe em atletas, inscrições, categorias ou modalidades, só zera os tempos registrados.
    /// Retorna quantas leituras foram apagadas.
    /// </summary>
    public async Task<int> LimparLeiturasAsync(int provaId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Leituras.Where(l => l.ProvaId == provaId).ExecuteDeleteAsync();
    }
}
