namespace Cronometragem.Core.Entidades;

/// <summary>
/// Registro bruto (append-only) de uma leitura de chip em um ponto de controle.
/// Nunca é editado ou removido após gravado — é a fonte da verdade para o cálculo de tempos.
/// </summary>
public class LeituraEvento
{
    public long Id { get; set; }
    public int ProvaId { get; set; }
    public string ChipId { get; set; } = string.Empty;
    public PontoControle PontoControle { get; set; }
    public DateTime DataHoraLeitura { get; set; }
    public string? OrigemLeitor { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
