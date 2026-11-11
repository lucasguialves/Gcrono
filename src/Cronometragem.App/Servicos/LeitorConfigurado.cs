using CommunityToolkit.Mvvm.ComponentModel;
using Cronometragem.Core.Entidades;
using Cronometragem.Core.Leitura;

namespace Cronometragem.App.Servicos;

/// <summary>
/// Associa uma instância de IChipReader ao ponto de controle que ela representa na prova
/// (largada ou chegada). Uma prova pode ter mais de um leitor no mesmo ponto de controle
/// (antenas redundantes), então a associação é por instância, não por tipo.
/// </summary>
public partial class LeitorConfigurado : ObservableObject
{
    public IChipReader Leitor { get; }
    public PontoControle PontoControle { get; }
    public string Nome => Leitor.Nome;

    [ObservableProperty]
    private bool _conectado;

    public LeitorConfigurado(IChipReader leitor, PontoControle pontoControle)
    {
        Leitor = leitor;
        PontoControle = pontoControle;
    }
}
