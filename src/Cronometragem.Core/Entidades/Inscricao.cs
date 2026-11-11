namespace Cronometragem.Core.Entidades;

public enum StatusInscricao
{
    Ativa,
    Cancelada,
    Desqualificada
}

/// <summary>
/// Associa um atleta a uma prova específica: número de peito, chip RFID e categoria.
/// O mesmo chip físico pode ser reaproveitado em provas diferentes, por isso o vínculo
/// chip ↔ atleta vive aqui, não na entidade Atleta.
/// </summary>
public class Inscricao
{
    public int Id { get; set; }
    public int ProvaId { get; set; }
    public int AtletaId { get; set; }
    public string NumeroPeito { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string ChipId { get; set; } = string.Empty;
    public StatusInscricao Status { get; set; } = StatusInscricao.Ativa;
}
