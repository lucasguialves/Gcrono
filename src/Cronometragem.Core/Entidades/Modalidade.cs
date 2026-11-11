namespace Cronometragem.Core.Entidades;

/// <summary>
/// A distância/tipo de prova que o atleta vai disputar (ex.: "5km", "10km", "Caminhada 3km").
/// Uma prova pode ter várias modalidades no mesmo dia/evento.
/// </summary>
public class Modalidade
{
    public int Id { get; set; }
    public int ProvaId { get; set; }
    public string Nome { get; set; } = string.Empty;
}
