using CommunityToolkit.Mvvm.ComponentModel;
using Cronometragem.Core.Entidades;

namespace Cronometragem.App.Servicos;

/// <summary>
/// Estado compartilhado entre as telas: qual prova está ativa no momento. Registrado como
/// singleton no container de DI para que todas as ViewModels vejam a mesma prova selecionada.
/// </summary>
public partial class EstadoAplicacao : ObservableObject
{
    [ObservableProperty]
    private Prova? _provaAtiva;
}
