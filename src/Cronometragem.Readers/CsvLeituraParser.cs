using System.Globalization;
using Cronometragem.Core.Leitura;

namespace Cronometragem.Readers;

/// <summary>
/// Parser genérico de formato "chipId,timestampIso" por linha — usado como protocolo padrão
/// enquanto o leitor real não é definido. Quando o fabricante do leitor for escolhido, troque
/// este parser por um que entenda o formato de linha dele (mantendo o TcpTextChipReader igual).
/// </summary>
public class CsvLeituraParser : ILeituraParser
{
    private readonly char _separador;

    public CsvLeituraParser(char separador = ',')
    {
        _separador = separador;
    }

    public bool TentarFazerParse(string linha, out string chipId, out DateTime dataHoraLeitura)
    {
        chipId = string.Empty;
        dataHoraLeitura = default;

        if (string.IsNullOrWhiteSpace(linha))
            return false;

        var partes = linha.Split(_separador, 2);
        if (partes.Length != 2)
            return false;

        var chipCandidato = partes[0].Trim();
        var timestampCandidato = partes[1].Trim();

        if (chipCandidato.Length == 0)
            return false;

        if (!DateTime.TryParse(
                timestampCandidato,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var timestamp))
            return false;

        chipId = chipCandidato;
        dataHoraLeitura = timestamp;
        return true;
    }
}
