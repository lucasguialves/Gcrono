using System.Windows.Controls;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class PainelAoVivoView : UserControl
{
    public PainelAoVivoView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<PainelAoVivoViewModel>();
    }
}
