using System.Net.Sockets;
using System.Text;
using Cronometragem.Core.Leitura;

namespace Cronometragem.Readers;

/// <summary>
/// Leitor genérico que conecta via TCP a um host:porta e interpreta cada linha de texto recebida
/// através de um <see cref="ILeituraParser"/> plugável. A maioria dos leitores RFID de portal
/// expõe os eventos de leitura assim; quando o hardware real for escolhido, basta trocar o
/// parser (ou, se o protocolo não for texto por linha, criar outra implementação de IChipReader) —
/// o resto do sistema não muda.
/// </summary>
public class TcpTextChipReader : IChipReader
{
    public string Nome { get; }

    public event EventHandler<LeituraRecebidaEventArgs>? LeituraRecebida;

    private readonly string _host;
    private readonly int _porta;
    private readonly ILeituraParser _parser;

    private TcpClient? _cliente;
    private CancellationTokenSource? _cts;
    private Task? _loopLeitura;

    public TcpTextChipReader(string nome, string host, int porta, ILeituraParser parser)
    {
        Nome = nome;
        _host = host;
        _porta = porta;
        _parser = parser;
    }

    public async Task ConectarAsync(CancellationToken ct = default)
    {
        _cliente = new TcpClient();
        await _cliente.ConnectAsync(_host, _porta, ct);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loopLeitura = Task.Run(() => LoopDeLeituraAsync(_cts.Token), _cts.Token);
    }

    public async Task DesconectarAsync()
    {
        _cts?.Cancel();

        if (_loopLeitura is not null)
        {
            try { await _loopLeitura; }
            catch (OperationCanceledException) { /* esperado ao cancelar o loop */ }
        }

        _cliente?.Close();
        _cliente = null;
    }

    private async Task LoopDeLeituraAsync(CancellationToken ct)
    {
        if (_cliente is null)
            return;

        using var stream = _cliente.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII);

        while (!ct.IsCancellationRequested)
        {
            var linha = await reader.ReadLineAsync(ct);
            if (linha is null)
                break; // conexão encerrada pelo leitor

            if (_parser.TentarFazerParse(linha, out var chipId, out var dataHoraLeitura))
            {
                LeituraRecebida?.Invoke(this, new LeituraRecebidaEventArgs(chipId, dataHoraLeitura, Nome));
            }
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cliente?.Dispose();
        _cts?.Dispose();
    }
}
