using System.Windows.Controls;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class AtletasView : UserControl
{
    public AtletasView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<AtletasViewModel>();
    }
}
