namespace Cronometragem.Core.Entidades;

public class Categoria
{
    public int Id { get; set; }
    public int ProvaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int? IdadeMin { get; set; }
    public int? IdadeMax { get; set; }
    public char? Sexo { get; set; } // 'M', 'F' ou null (livre)
}
