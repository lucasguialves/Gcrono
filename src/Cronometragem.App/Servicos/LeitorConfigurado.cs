using CommunityToolkit.Mvvm.ComponentModel;
using Cronometragem.Core.Leitura;

namespace Cronometragem.App.Servicos;

/// <summary>
/// Associa uma instância de IChipReader ao modo como ela classifica as leituras recebidas
/// (ponto de controle fixo, ou automático para quando o mesmo leitor serve largada e chegada).
/// Uma prova pode ter mais de um leitor no mesmo modo (antenas redundantes), então a associação
/// é por instância, não por tipo.
/// </summary>
public partial class LeitorConfigurado : ObservableObject
{
    public IChipReader Leitor { get; }
    public ModoLeitor Modo { get; }
    public string Nome => Leitor.Nome;

    [ObservableProperty]
    private bool _conectado;

    public LeitorConfigurado(IChipReader leitor, ModoLeitor modo)
    {
        Leitor = leitor;
        Modo = modo;
    }
}
