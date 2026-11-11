using System.Windows.Controls;
using Cronometragem.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class ProvasView : UserControl
{
    public ProvasView()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ProvasViewModel>();
    }
}
