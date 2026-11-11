using Cronometragem.Core.Leitura;

namespace Cronometragem.Readers;

/// <summary>
/// Leitor simulado: não fala com hardware nenhum. Serve pra desenvolver e testar o pipeline
/// inteiro (captura → gravação → cálculo → UI) antes de o leitor físico estar definido —
/// dispare leituras chamando SimularLeitura a partir da tela de configuração/painel ao vivo.
/// </summary>
public class MockChipReader : IChipReader
{
    public string Nome { get; }

    public event EventHandler<LeituraRecebidaEventArgs>? LeituraRecebida;

    private bool _conectado;

    public MockChipReader(string nome = "Simulador")
    {
        Nome = nome;
    }

    public Task ConectarAsync(CancellationToken ct = default)
    {
        _conectado = true;
        return Task.CompletedTask;
    }

    public Task DesconectarAsync()
    {
        _conectado = false;
        return Task.CompletedTask;
    }

    /// <summary>Dispara manualmente uma leitura simulada, como se o chip tivesse acabado de cruzar a antena.</summary>
    public void SimularLeitura(string chipId, DateTime? dataHoraLeitura = null)
    {
        if (!_conectado)
            throw new InvalidOperationException($"Leitor '{Nome}' não está conectado.");

        LeituraRecebida?.Invoke(this, new LeituraRecebidaEventArgs(chipId, dataHoraLeitura ?? DateTime.Now, Nome));
    }

    public void Dispose()
    {
        _conectado = false;
    }
}
