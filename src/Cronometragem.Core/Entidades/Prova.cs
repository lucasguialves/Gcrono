namespace Cronometragem.Core.Entidades;

public class Prova
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateOnly DataProva { get; set; }
    public TimeOnly HoraLargadaOficial { get; set; }

    public List<Categoria> Categorias { get; set; } = new();
    public List<Inscricao> Inscricoes { get; set; } = new();

    /// <summary>
    /// Timestamp absoluto do tiro de largada, combinando a data da prova com a hora oficial.
    /// Usado como referência para o tempo bruto de cada atleta.
    /// </summary>
    public DateTime LargadaOficialEm =>
        DataProva.ToDateTime(HoraLargadaOficial);
}
