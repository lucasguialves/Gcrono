namespace Cronometragem.Core.Leitura;

public class LeituraRecebidaEventArgs : EventArgs
{
    public string ChipId { get; }
    public DateTime DataHoraLeitura { get; }
    public string OrigemLeitor { get; }

    public LeituraRecebidaEventArgs(string chipId, DateTime dataHoraLeitura, string origemLeitor)
    {
        ChipId = chipId;
        DataHoraLeitura = dataHoraLeitura;
        OrigemLeitor = origemLeitor;
    }
}
