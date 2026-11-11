using System.Windows;
using System.Windows.Controls;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class ProvasView : UserControl
{
    private ProvasViewModel ViewModel => (ProvasViewModel)DataContext;

    public ProvasView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ProvasViewModel>();
    }

    private async void EditarProva_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ProvaSelecionada is null)
        {
            MessageBox.Show("Selecione uma prova na lista primeiro.");
            return;
        }

        var janela = new EditarProvaWindow(ViewModel.ProvaSelecionada)
        {
            Owner = Window.GetWindow(this)
        };

        if (janela.ShowDialog() == true)
        {
            await ViewModel.RecarregarAposEdicaoAsync();
        }
    }
}
