using System.Windows;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class EditarProvaWindow : Window
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;
    private readonly int _provaId;

    public EditarProvaWindow(Prova prova)
    {
        InitializeComponent();
        _dbFactory = App.Services.GetRequiredService<IDbContextFactory<CronometragemDbContext>>();
        _provaId = prova.Id;

        TxtNome.Text = prova.Nome;
        DpDataProva.SelectedDate = prova.DataProva.ToDateTime(TimeOnly.MinValue);
        TxtHoraLargada.Text = prova.HoraLargadaOficial.ToString("HH:mm:ss");
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNome.Text))
        {
            MessageBox.Show("Informe o nome da prova.");
            return;
        }

        if (DpDataProva.SelectedDate is null)
        {
            MessageBox.Show("Selecione a data da prova.");
            return;
        }

        if (!TimeSpan.TryParse(TxtHoraLargada.Text, out var horaTs))
        {
            MessageBox.Show("Hora de largada inválida. Use o formato HH:mm:ss.");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var prova = await db.Provas.FindAsync(_provaId);
        if (prova is null)
        {
            MessageBox.Show("Esta prova não existe mais (pode ter sido removida).");
            DialogResult = false;
            Close();
            return;
        }

        prova.Nome = TxtNome.Text.Trim();
        prova.DataProva = DateOnly.FromDateTime(DpDataProva.SelectedDate.Value);
        prova.HoraLargadaOficial = TimeOnly.FromTimeSpan(horaTs);

        await db.SaveChangesAsync();

        DialogResult = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
