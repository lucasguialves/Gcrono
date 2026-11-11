using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cronometragem.App.Servicos;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;

namespace Cronometragem.App.ViewModels;

public record InscricaoExibicao(Inscricao Inscricao, string NomeAtleta, string NomeCategoria);

public partial class AtletasViewModel : ObservableObject
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;
    private readonly EstadoAplicacao _estado;

    [ObservableProperty] private ObservableCollection<Atleta> _atletas = new();
    [ObservableProperty] private ObservableCollection<Categoria> _categorias = new();
    [ObservableProperty] private ObservableCollection<InscricaoExibicao> _inscricoes = new();

    [ObservableProperty] private string _novoNomeAtleta = string.Empty;
    [ObservableProperty] private string _novoDocumento = string.Empty;

    [ObservableProperty] private Atleta? _atletaSelecionadoParaInscricao;
    [ObservableProperty] private string _novoNumeroPeito = string.Empty;
    [ObservableProperty] private string _novoChipId = string.Empty;
    [ObservableProperty] private Categoria? _categoriaSelecionadaParaInscricao;

    [ObservableProperty] private string _novoNomeCategoria = string.Empty;
    [ObservableProperty] private string _novaIdadeMin = string.Empty;
    [ObservableProperty] private string _novaIdadeMax = string.Empty;

    public Prova? ProvaAtiva => _estado.ProvaAtiva;

    public AtletasViewModel(IDbContextFactory<CronometragemDbContext> dbFactory, EstadoAplicacao estado)
    {
        _dbFactory = dbFactory;
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
        await using var db = await _dbFactory.CreateDbContextAsync();
        Atletas = new ObservableCollection<Atleta>(await db.Atletas.OrderBy(a => a.Nome).ToListAsync());

        if (_estado.ProvaAtiva is null)
        {
            Categorias = new ObservableCollection<Categoria>();
            Inscricoes = new ObservableCollection<InscricaoExibicao>();
            return;
        }

        var provaId = _estado.ProvaAtiva.Id;
        Categorias = new ObservableCollection<Categoria>(
            await db.Categorias.Where(c => c.ProvaId == provaId).ToListAsync());

        var inscricoes = await db.Inscricoes.Where(i => i.ProvaId == provaId).ToListAsync();
        var atletasPorId = Atletas.ToDictionary(a => a.Id);
        var categoriasPorId = Categorias.ToDictionary(c => c.Id);

        Inscricoes = new ObservableCollection<InscricaoExibicao>(inscricoes.Select(i => new InscricaoExibicao(
            i,
            atletasPorId.TryGetValue(i.AtletaId, out var atleta) ? atleta.Nome : "?",
            i.CategoriaId.HasValue && categoriasPorId.TryGetValue(i.CategoriaId.Value, out var cat) ? cat.Nome : "-")));
    }

    [RelayCommand]
    private async Task CriarAtletaAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoNomeAtleta))
        {
            MessageBox.Show("Informe o nome do atleta.");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Atletas.Add(new Atleta { Nome = NovoNomeAtleta, Documento = NovoDocumento });
        await db.SaveChangesAsync();

        NovoNomeAtleta = string.Empty;
        NovoDocumento = string.Empty;
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task CriarCategoriaAsync()
    {
        if (_estado.ProvaAtiva is null)
        {
            MessageBox.Show("Selecione a prova ativa na aba Provas primeiro.");
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
            ProvaId = _estado.ProvaAtiva.Id,
            Nome = NovoNomeCategoria,
            IdadeMin = int.TryParse(NovaIdadeMin, out var min) ? min : null,
            IdadeMax = int.TryParse(NovaIdadeMax, out var max) ? max : null
        });
        await db.SaveChangesAsync();

        NovoNomeCategoria = string.Empty;
        NovaIdadeMin = string.Empty;
        NovaIdadeMax = string.Empty;
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task CriarInscricaoAsync()
    {
        if (_estado.ProvaAtiva is null)
        {
            MessageBox.Show("Selecione a prova ativa na aba Provas primeiro.");
            return;
        }

        if (AtletaSelecionadoParaInscricao is null
            || string.IsNullOrWhiteSpace(NovoNumeroPeito)
            || string.IsNullOrWhiteSpace(NovoChipId))
        {
            MessageBox.Show("Selecione o atleta e informe o número de peito e o chip.");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Inscricoes.Add(new Inscricao
        {
            ProvaId = _estado.ProvaAtiva.Id,
            AtletaId = AtletaSelecionadoParaInscricao.Id,
            NumeroPeito = NovoNumeroPeito,
            ChipId = NovoChipId,
            CategoriaId = CategoriaSelecionadaParaInscricao?.Id
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show(
                "Não foi possível salvar a inscrição — número de peito ou chip já usado nesta prova?\n\n" + ex.Message,
                "Erro ao inscrever atleta");
            return;
        }

        NovoNumeroPeito = string.Empty;
        NovoChipId = string.Empty;
        await CarregarAsync();
    }
}
