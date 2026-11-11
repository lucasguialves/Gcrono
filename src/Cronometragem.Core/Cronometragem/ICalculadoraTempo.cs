using Cronometragem.Core.Entidades;

namespace Cronometragem.Core.Cronometragem;

public interface ICalculadoraTempo
{
    /// <summary>
    /// Calcula o resultado (tempo bruto/líquido) de uma inscrição a partir das leituras brutas
    /// do chip dela. <paramref name="leiturasDoChip"/> deve conter apenas leituras do chip da
    /// inscrição nesta prova (não precisa já estar ordenada).
    /// </summary>
    ResultadoAtleta Calcular(Prova prova, Inscricao inscricao, IEnumerable<LeituraEvento> leiturasDoChip);
}
