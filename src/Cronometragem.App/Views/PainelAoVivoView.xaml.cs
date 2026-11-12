using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class PainelAoVivoView : UserControl
{
    private PainelAoVivoViewModel ViewModel => (PainelAoVivoViewModel)DataContext;

    public PainelAoVivoView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PainelAoVivoViewModel>();
    }

    private void TxtHoraRegistro_GotFocus(object sender, RoutedEventArgs e) => ViewModel.IniciarEdicaoHorario();

    private void TxtHoraRegistro_LostFocus(object sender, RoutedEventArgs e) => ViewModel.FinalizarEdicaoHorario();

    private void AbrirTelao_Click(object sender, RoutedEventArgs e)
    {
        new TelaoWindow().Show();
    }

    private void TxtNumeroPeito_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (ViewModel.RegistrarManualCommand.CanExecute(null))
            ViewModel.RegistrarManualCommand.Execute(null);
    }
}
