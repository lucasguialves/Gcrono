using System.Windows.Controls;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class ConfiguracaoLeitoresView : UserControl
{
    public ConfiguracaoLeitoresView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ConfiguracaoLeitoresViewModel>();
    }
}
