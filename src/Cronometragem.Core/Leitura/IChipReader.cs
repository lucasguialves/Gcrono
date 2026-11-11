namespace Cronometragem.Core.Leitura;

/// <summary>
/// Abstração de um leitor/antena RFID. Implementações concretas (TCP, SDK do fabricante,
/// simulador) ficam em Cronometragem.Readers — o resto do sistema só depende desta interface,
/// então trocar de hardware não exige mudar a captura, persistência ou cálculo de tempos.
/// </summary>
public interface IChipReader : IDisposable
{
    /// <summary>Identificação deste leitor, usada para preencher LeituraEvento.OrigemLeitor.</summary>
    string Nome { get; }

    event EventHandler<LeituraRecebidaEventArgs>? LeituraRecebida;

    Task ConectarAsync(CancellationToken ct = default);

    Task DesconectarAsync();
}
