using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cronometragem.App.Servicos;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.ViewModels;

public partial class ProvasViewModel : ObservableObject
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;
    private readonly EstadoAplicacao _estado;

    [ObservableProperty]
    private ObservableCollection<Prova> _provas = new();

    [ObservableProperty]
    private Prova? _provaSelecionada;

    [ObservableProperty]
    private string _novoNome = string.Empty;

    [ObservableProperty]
    private DateTime _novaData = DateTime.Today;

    [ObservableProperty]
    private string _novaHoraLargada = "08:00:00";

    [ObservableProperty]
    private ObservableCollection<Categoria> _categorias = new();

    [ObservableProperty]
    private ObservableCollection<Modalidade> _modalidades = new();

    [ObservableProperty] private string _novoNomeCategoria = string.Empty;
    [ObservableProperty] private string _novaIdadeMin = string.Empty;
    [ObservableProperty] private string _novaIdadeMax = string.Empty;

    public string[] OpcoesSexoCategoria { get; } = { "Qualquer", "Masculino", "Feminino" };
    [ObservableProperty] private string _novoSexoCategoria = "Qualquer";

    [ObservableProperty] private string _novoNomeModalidade = string.Empty;

    public Prova? ProvaAtiva => _estado.ProvaAtiva;

    public ProvasViewModel(IDbContextFactory<CronometragemDbContext> dbFactory, EstadoAplicacao estado)
    {
        _dbFactory = dbFactory;
        _estado = estado;
        _estado.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EstadoAplicacao.ProvaAtiva))
                OnPropertyChanged(nameof(ProvaAtiva));
        };

        _ = CarregarAsync();
    }

    partial void OnProvaSelecionadaChanged(Prova? value) => _ = CarregarCategoriasEModalidadesAsync();

    [RelayCommand]
    private async Task CarregarAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var lista = await db.Provas.OrderByDescending(p => p.DataProva).ToListAsync();
        Provas = new ObservableCollection<Prova>(lista);
    }

    private async Task CarregarCategoriasEModalidadesAsync()
    {
        if (ProvaSelecionada is null)
        {
            Categorias = new ObservableCollection<Categoria>();
            Modalidades = new ObservableCollection<Modalidade>();
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var provaId = ProvaSelecionada.Id;

        Categorias = new ObservableCollection<Categoria>(
            await db.Categorias.Where(c => c.ProvaId == provaId).ToListAsync());
        Modalidades = new ObservableCollection<Modalidade>(
            await db.Modalidades.Where(m => m.ProvaId == provaId).ToListAsync());
    }

    [RelayCommand]
    private async Task CriarAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoNome))
        {
            MessageBox.Show("Informe o nome da prova.");
            return;
        }

        if (!TimeSpan.TryParse(NovaHoraLargada, out var horaTs))
        {
            MessageBox.Show("Hora de largada inválida. Use o formato HH:mm:ss.");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Provas.Add(new Prova
        {
            Nome = NovoNome,
            DataProva = DateOnly.FromDateTime(NovaData),
            HoraLargadaOficial = TimeOnly.FromTimeSpan(horaTs)
        });
        await db.SaveChangesAsync();

        NovoNome = string.Empty;
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task CriarCategoriaAsync()
    {
        if (ProvaSelecionada is null)
        {
            MessageBox.Show("Selecione uma prova na lista primeiro.");
            return;
        }

        if (string.IsNullOrWhiteSpace(NovoNomeCategoria))
        {
            MessageBox.Show("Informe o nome da categoria.");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Categorias.Add(new Categoria
        {
            ProvaId = ProvaSelecionada.Id,
            Nome = NovoNomeCategoria,
            IdadeMin = int.TryParse(NovaIdadeMin, out var min) ? min : null,
            IdadeMax = int.TryParse(NovaIdadeMax, out var max) ? max : null,
            Sexo = NovoSexoCategoria switch { "Masculino" => 'M', "Feminino" => 'F', _ => null }
        });
        await db.SaveChangesAsync();

        NovoNomeCategoria = string.Empty;
        NovaIdadeMin = string.Empty;
        NovaIdadeMax = string.Empty;
        NovoSexoCategoria = "Qualquer";
        await CarregarCategoriasEModalidadesAsync();
    }

    [RelayCommand]
    private async Task CriarModalidadeAsync()
    {
        if (ProvaSelecionada is null)
        {
            MessageBox.Show("Selecione uma prova na lista primeiro.");
            return;
        }

        if (string.IsNullOrWhiteSpace(NovoNomeModalidade))
        {
            MessageBox.Show("Informe o nome da modalidade (ex.: 5km, 10km, Caminhada).");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Modalidades.Add(new Modalidade
        {
            ProvaId = ProvaSelecionada.Id,
            Nome = NovoNomeModalidade
        });
        await db.SaveChangesAsync();

        NovoNomeModalidade = string.Empty;
        await CarregarCategoriasEModalidadesAsync();
    }

    [RelayCommand]
    private void SelecionarComoAtiva()
    {
        if (ProvaSelecionada is null)
        {
            MessageBox.Show("Selecione uma prova na lista primeiro.");
            return;
        }

        _estado.ProvaAtiva = ProvaSelecionada;
    }

    /// <summary>Chamado pela View após editar a prova numa janela — recarrega a lista com os dados atualizados.</summary>
    public async Task RecarregarAposEdicaoAsync()
    {
        await CarregarAsync();

        // Se a prova editada era a ativa, atualiza a referência em EstadoAplicacao também
        // (senão o nome/data antigos ficariam presos nas outras telas até trocar de prova ativa).
        if (_estado.ProvaAtiva is { } ativa)
        {
            var atualizada = Provas.FirstOrDefault(p => p.Id == ativa.Id);
            if (atualizada is not null)
                _estado.ProvaAtiva = atualizada;
        }
    }
}
