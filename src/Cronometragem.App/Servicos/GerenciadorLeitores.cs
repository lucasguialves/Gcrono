using System.Collections.ObjectModel;
using Cronometragem.Core.Leitura;
using Cronometragem.Core.Entidades;

namespace Cronometragem.App.Servicos;

/// <summary>
/// Mantém a lista de leitores configurados para a prova ativa e encaminha toda leitura recebida
/// para o ServicoCronometragem (ou ServicoLargadaAutomatica, no modo de leitor único), usando a
/// prova atualmente selecionada em EstadoAplicacao.
/// </summary>
public class GerenciadorLeitores
{
    private readonly ServicoCronometragem _servicoCronometragem;
    private readonly ServicoLargadaAutomatica _servicoLargadaAutomatica;
    private readonly EstadoAplicacao _estado;

    public ObservableCollection<LeitorConfigurado> Leitores { get; } = new();

    public GerenciadorLeitores(
        ServicoCronometragem servicoCronometragem,
        ServicoLargadaAutomatica servicoLargadaAutomatica,
        EstadoAplicacao estado)
    {
        _servicoCronometragem = servicoCronometragem;
        _servicoLargadaAutomatica = servicoLargadaAutomatica;
        _estado = estado;
    }

    public void AdicionarLeitor(IChipReader leitor, ModoLeitor modo)
    {
        var configurado = new LeitorConfigurado(leitor, modo);
        leitor.LeituraRecebida += async (_, e) => await OnLeituraRecebidaAsync(configurado, e);
        Leitores.Add(configurado);
    }

    public async Task ConectarAsync(LeitorConfigurado configurado)
    {
        await configurado.Leitor.ConectarAsync();
        configurado.Conectado = true;
    }

    public async Task DesconectarAsync(LeitorConfigurado configurado)
    {
        await configurado.Leitor.DesconectarAsync();
        configurado.Conectado = false;
    }

    private async Task OnLeituraRecebidaAsync(LeitorConfigurado configurado, LeituraRecebidaEventArgs e)
    {
        var prova = _estado.ProvaAtiva;
        if (prova is null)
            return; // nenhuma prova selecionada ainda: leitura é descartada (não gravamos sem saber de qual prova é)

        switch (configurado.Modo)
        {
            case ModoLeitor.AutomaticoUnico:
                await _servicoLargadaAutomatica.ProcessarLeituraAsync(prova.Id, e.ChipId, e.DataHoraLeitura, e.OrigemLeitor);
                break;
            case ModoLeitor.Largada:
                await _servicoCronometragem.RegistrarLeituraAsync(prova.Id, PontoControle.Largada, e.ChipId, e.DataHoraLeitura, e.OrigemLeitor);
                break;
            case ModoLeitor.Chegada:
                await _servicoCronometragem.RegistrarLeituraAsync(prova.Id, PontoControle.Chegada, e.ChipId, e.DataHoraLeitura, e.OrigemLeitor);
                break;
        }
    }
}
