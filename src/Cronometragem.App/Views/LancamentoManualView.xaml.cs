using System.Windows.Controls;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class LancamentoManualView : UserControl
{
    public LancamentoManualView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<LancamentoManualViewModel>();
    }
}
