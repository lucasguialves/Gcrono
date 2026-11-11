using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cronometragem.App.Servicos;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.ViewModels;

public record LeituraExibicao(DateTime DataHora, string ChipId, PontoControle PontoControle, string OrigemLeitor, string NumeroPeito);

public record PendenteExibicao(string ChipId, string NumeroPeito, DateTime DataHoraLeitura);

/// <summary>
/// Painel ao vivo das leituras recebidas (automáticas ou manuais) e, na mesma tela, o
/// lançamento manual de largada/chegada por número de peito — backup para quando o leitor RFID
/// falhar em captar o chip na hora.
/// </summary>
public partial class PainelAoVivoViewModel : ObservableObject, IDisposable
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;
    private readonly ServicoCronometragem _servico;
    private readonly ServicoLargadaAutomatica _servicoLargadaAutomatica;
    private readonly EstadoAplicacao _estado;

    public ObservableCollection<LeituraExibicao> Leituras { get; } = new();

    [ObservableProperty]
    private ObservableCollection<PendenteExibicao> _pendentesDeLargada = new();

    [ObservableProperty]
    private ObservableCollection<InscricaoExibicao> _inscricoes = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoAtletaEncontrado))]
    private string _numeroPeitoDigitado = string.Empty;

    [ObservableProperty]
    private PontoControle _pontoControleSelecionado = PontoControle.Largada;

    [ObservableProperty]
    private DateTime _horaRegistro = DateTime.Now;

    public Array PontosControleDisponiveis { get; } = Enum.GetValues(typeof(PontoControle));

    public Prova? ProvaAtiva => _estado.ProvaAtiva;

    /// <summary>Confere, em tempo real, se o peito digitado bate com um atleta inscrito.</summary>
    private InscricaoExibicao? AtletaEncontrado =>
        Inscricoes.FirstOrDefault(i => i.Inscricao.NumeroPeito == NumeroPeitoDigitado.Trim());

    public string TextoAtletaEncontrado
    {
        get
        {
            if (string.IsNullOrWhiteSpace(NumeroPeitoDigitado))
                return string.Empty;

            var encontrado = AtletaEncontrado;
            return encontrado is null
                ? "Nenhum atleta com esse número de peito nesta prova."
                : $"{encontrado.NomeAtleta} — categoria {encontrado.NomeCategoria}";
        }
    }

    public PainelAoVivoViewModel(
        IDbContextFactory<CronometragemDbContext> dbFactory,
        ServicoCronometragem servico,
        ServicoLargadaAutomatica servicoLargadaAutomatica,
        EstadoAplicacao estado)
    {
        _dbFactory = dbFactory;
        _servico = servico;
        _servicoLargadaAutomatica = servicoLargadaAutomatica;
        _estado = estado;
        _servico.LeituraProcessada += OnLeituraProcessada;
        _servicoLargadaAutomatica.PendentesAlterados += OnPendentesAlterados;

        _estado.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName != nameof(EstadoAplicacao.ProvaAtiva))
                return;

            OnPropertyChanged(nameof(ProvaAtiva));
            await CarregarInscricoesAsync();
            AtualizarChipsPendentes();
        };

        _ = CarregarInscricoesAsync();
    }

    [RelayCommand]
    private async Task CarregarInscricoesAsync()
    {
        if (_estado.ProvaAtiva is null)
        {
            Inscricoes = new ObservableCollection<InscricaoExibicao>();
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var provaId = _estado.ProvaAtiva.Id;

        var inscricoes = await db.Inscricoes
            .Where(i => i.ProvaId == provaId && i.Status == StatusInscricao.Ativa)
            .ToListAsync();
        var atletasPorId = await db.Atletas.ToDictionaryAsync(a => a.Id);
        var categoriasPorId = await db.Categorias.Where(c => c.ProvaId == provaId).ToDictionaryAsync(c => c.Id);

        Inscricoes = new ObservableCollection<InscricaoExibicao>(inscricoes
            .OrderBy(i => i.NumeroPeito)
            .Select(i => new InscricaoExibicao(
                i,
                atletasPorId.TryGetValue(i.AtletaId, out var atleta) ? atleta.Nome : "?",
                i.CategoriaId.HasValue && categoriasPorId.TryGetValue(i.CategoriaId.Value, out var cat) ? cat.Nome : "-")));
    }

    [RelayCommand]
    private async Task RegistrarManualAsync()
    {
        if (_estado.ProvaAtiva is null)
        {
            MessageBox.Show("Selecione a prova ativa na aba Provas primeiro.");
            return;
        }

        var atleta = AtletaEncontrado;
        if (atleta is null)
        {
            MessageBox.Show("Nenhum atleta encontrado com esse número de peito nesta prova.");
            return;
        }

        await _servico.RegistrarLeituraAsync(
            _estado.ProvaAtiva.Id,
            PontoControleSelecionado,
            atleta.Inscricao.ChipId,
            HoraRegistro,
            origemLeitor: "Manual");

        NumeroPeitoDigitado = string.Empty;
        HoraRegistro = DateTime.Now;
    }

    [RelayCommand]
    private async Task ConfirmarLargadasAsync()
    {
        if (_estado.ProvaAtiva is null)
        {
            MessageBox.Show("Selecione a prova ativa na aba Provas primeiro.");
            return;
        }

        await _servicoLargadaAutomatica.ConfirmarLargadasAsync(_estado.ProvaAtiva.Id);
    }

    private void OnLeituraProcessada(object? sender, LeituraProcessadaEventArgs e)
    {
        if (_estado.ProvaAtiva is null || e.Leitura.ProvaId != _estado.ProvaAtiva.Id)
            return;

        Application.Current.Dispatcher.Invoke(() =>
        {
            Leituras.Insert(0, new LeituraExibicao(
                e.Leitura.DataHoraLeitura,
                e.Leitura.ChipId,
                e.Leitura.PontoControle,
                e.Leitura.OrigemLeitor ?? "-",
                e.Inscricao?.NumeroPeito ?? "NÃO INSCRITO"));
        });
    }

    private void OnPendentesAlterados(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(AtualizarChipsPendentes);
    }

    private void AtualizarChipsPendentes()
    {
        if (_estado.ProvaAtiva is null)
        {
            PendentesDeLargada = new ObservableCollection<PendenteExibicao>();
            return;
        }

        var pendentes = _servicoLargadaAutomatica.PendentesDaProva(_estado.ProvaAtiva.Id);

        PendentesDeLargada = new ObservableCollection<PendenteExibicao>(pendentes.Select(p =>
        {
            var peito = Inscricoes.FirstOrDefault(i => i.Inscricao.ChipId == p.ChipId)?.Inscricao.NumeroPeito
                ?? "NÃO INSCRITO";
            return new PendenteExibicao(p.ChipId, peito, p.DataHoraLeitura);
        }));
    }

    public void Dispose()
    {
        _servico.LeituraProcessada -= OnLeituraProcessada;
        _servicoLargadaAutomatica.PendentesAlterados -= OnPendentesAlterados;
        GC.SuppressFinalize(this);
    }
}
