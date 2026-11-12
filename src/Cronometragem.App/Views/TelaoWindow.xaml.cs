using System.Windows;
using Cronometragem.App.Servicos;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class TelaoWindow : Window
{
    private readonly TelaoViewModel _viewModel;

    public TelaoWindow()
    {
        InitializeComponent();

        _viewModel = new TelaoViewModel(
            App.Services.GetRequiredService<ServicoCronometragem>(),
            App.Services.GetRequiredService<ServicoResultados>(),
            App.Services.GetRequiredService<EstadoAplicacao>());
        DataContext = _viewModel;
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.Dispose();
        base.OnClosed(e);
    }
}
