namespace Cronometragem.Core.Leitura;

/// <summary>
/// Extrai (chipId, dataHoraLeitura) de uma linha bruta recebida do leitor. Cada protocolo de
/// fabricante vira uma implementação desta interface, sem tocar no TcpTextChipReader em si.
/// </summary>
public interface ILeituraParser
{
    /// <summary>Tenta interpretar a linha recebida. Retorna false se a linha não for uma leitura válida.</summary>
    bool TentarFazerParse(string linha, out string chipId, out DateTime dataHoraLeitura);
}
