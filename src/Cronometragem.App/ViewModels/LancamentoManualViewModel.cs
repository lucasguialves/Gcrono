using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cronometragem.App.Servicos;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.ViewModels;

/// <summary>
/// Backup manual para quando o leitor RFID falha em captar o chip na hora: permite lançar a
/// largada ou chegada de um atleta buscando pelo número de peito, em vez de depender da leitura
/// automática. A leitura registrada aqui entra na mesma tabela `leituras` com origem "Manual" —
/// o cálculo de tempo e o painel ao vivo não fazem distinção entre leitura automática e manual.
/// </summary>
public partial class LancamentoManualViewModel : ObservableObject
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;
    private readonly ServicoCronometragem _servicoCronometragem;
    private readonly EstadoAplicacao _estado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoAtletaEncontrado))]
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

    public LancamentoManualViewModel(
        IDbContextFactory<CronometragemDbContext> dbFactory,
        ServicoCronometragem servicoCronometragem,
        EstadoAplicacao estado)
    {
        _dbFactory = dbFactory;
        _servicoCronometragem = servicoCronometragem;
        _estado = estado;

        _estado.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName != nameof(EstadoAplicacao.ProvaAtiva))
                return;

            OnPropertyChanged(nameof(ProvaAtiva));
            await CarregarAsync();
        };

        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task CarregarAsync()
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
    private async Task RegistrarAsync()
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

        await _servicoCronometragem.RegistrarLeituraAsync(
            _estado.ProvaAtiva.Id,
            PontoControleSelecionado,
            atleta.Inscricao.ChipId,
            HoraRegistro,
            origemLeitor: "Manual");

        MessageBox.Show(
            $"Registrado: peito {atleta.Inscricao.NumeroPeito} ({atleta.NomeAtleta}) — {PontoControleSelecionado} às {HoraRegistro:HH:mm:ss}.",
            "Lançamento manual");

        NumeroPeitoDigitado = string.Empty;
        HoraRegistro = DateTime.Now;
    }
}
