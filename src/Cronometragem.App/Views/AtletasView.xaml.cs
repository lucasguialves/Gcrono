using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cronometragem.App.ViewModels;
using Cronometragem.Core.Entidades;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class AtletasView : UserControl
{
    private AtletasViewModel ViewModel => (AtletasViewModel)DataContext;

    public AtletasView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<AtletasViewModel>();
    }

    private async void GradeAtletas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (GradeAtletas.SelectedItem is not Atleta atleta)
            return;

        var janela = new EditarAtletaWindow(atleta)
        {
            Owner = Window.GetWindow(this)
        };

        if (janela.ShowDialog() == true)
        {
            await ViewModel.RecarregarAposEdicaoAsync();
        }
    }
}
