using System.Windows;
using System.Windows.Controls;
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
}
