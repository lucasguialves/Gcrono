using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Cronometragem.App.Servicos;
using Cronometragem.Core.Entidades;

namespace Cronometragem.App.ViewModels;

public record LeituraExibicao(DateTime DataHora, string ChipId, PontoControle PontoControle, string OrigemLeitor, string NumeroPeito);

public partial class PainelAoVivoViewModel : ObservableObject, IDisposable
{
    private readonly ServicoCronometragem _servico;
    private readonly EstadoAplicacao _estado;

    public ObservableCollection<LeituraExibicao> Leituras { get; } = new();

    public PainelAoVivoViewModel(ServicoCronometragem servico, EstadoAplicacao estado)
    {
        _servico = servico;
        _estado = estado;
        _servico.LeituraProcessada += OnLeituraProcessada;
    }

    private void OnLeituraProcessada(object? sender, LeituraProcessadaEventArgs e)
    {
        if (_estado.ProvaAtiva is null || e.Leitura.ProvaId != _estado.ProvaAtiva.Id)
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            Leituras.Insert(0, new LeituraExibicao(
                e.Leitura.DataHoraLeitura,
                e.Leitura.ChipId,
                e.Leitura.PontoControle,
                e.Leitura.OrigemLeitor ?? "-",
                e.Inscricao?.NumeroPeito ?? "NÃO INSCRITO"));
        });
    }

    public void Dispose()
    {
        _servico.LeituraProcessada -= OnLeituraProcessada;
        GC.SuppressFinalize(this);
    }
}
