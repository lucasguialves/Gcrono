namespace Cronometragem.Core.Cronometragem;

public enum SituacaoResultado
{
    NaoLargou,
    EmProva,
    Finalizado
}

public class ResultadoAtleta
{
    public int InscricaoId { get; set; }
    public string NumeroPeito { get; set; } = string.Empty;
    public string ChipId { get; set; } = string.Empty;

    public DateTime? TempoLargada { get; set; }
    public DateTime? TempoChegada { get; set; }

    /// <summary>Tempo desde o disparo oficial da largada até a chegada do atleta.</summary>
    public TimeSpan? TempoBruto { get; set; }

    /// <summary>Tempo desde a passagem do próprio chip na largada até a chegada (tempo "chip").</summary>
    public TimeSpan? TempoLiquido { get; set; }

    public SituacaoResultado Situacao { get; set; } = SituacaoResultado.NaoLargou;
}
