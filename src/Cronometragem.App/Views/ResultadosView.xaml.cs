using System.Windows.Controls;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class ResultadosView : UserControl
{
    public ResultadosView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ResultadosViewModel>();
    }
}
