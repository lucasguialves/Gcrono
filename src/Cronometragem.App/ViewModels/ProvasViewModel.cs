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

    [RelayCommand]
    private async Task CarregarAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var lista = await db.Provas.OrderByDescending(p => p.DataProva).ToListAsync();
        Provas = new ObservableCollection<Prova>(lista);
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
    private void SelecionarComoAtiva()
    {
        if (ProvaSelecionada is null)
        {
            MessageBox.Show("Selecione uma prova na lista primeiro.");
            return;
        }

        _estado.ProvaAtiva = ProvaSelecionada;
    }
}
