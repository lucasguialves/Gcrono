namespace Cronometragem.Core.Entidades;

public class Atleta
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public char? Sexo { get; set; } // 'M' ou 'F'
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}
