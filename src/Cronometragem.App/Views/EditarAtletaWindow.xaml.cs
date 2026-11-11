using System.Windows;
using System.Windows.Controls;
using Cronometragem.Core.Entidades;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cronometragem.App.Views;

public partial class EditarAtletaWindow : Window
{
    private readonly IDbContextFactory<CronometragemDbContext> _dbFactory;
    private readonly int _atletaId;

    public EditarAtletaWindow(Atleta atleta)
    {
        InitializeComponent();
        _dbFactory = App.Services.GetRequiredService<IDbContextFactory<CronometragemDbContext>>();
        _atletaId = atleta.Id;

        TxtNome.Text = atleta.Nome;
        TxtDocumento.Text = atleta.Documento ?? string.Empty;
        TxtTelefone.Text = atleta.Telefone ?? string.Empty;
        TxtEmail.Text = atleta.Email ?? string.Empty;
        DpDataNascimento.SelectedDate = atleta.DataNascimento?.ToDateTime(TimeOnly.MinValue);

        var tagSexo = atleta.Sexo?.ToString() ?? string.Empty;
        foreach (ComboBoxItem item in CmbSexo.Items)
        {
            if ((string)item.Tag == tagSexo)
            {
                CmbSexo.SelectedItem = item;
                break;
            }
        }
        CmbSexo.SelectedItem ??= CmbSexo.Items[0];
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNome.Text))
        {
            MessageBox.Show("Informe o nome do atleta.");
            return;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();
        var atleta = await db.Atletas.FindAsync(_atletaId);
        if (atleta is null)
        {
            MessageBox.Show("Este atleta não existe mais (pode ter sido removido).");
            DialogResult = false;
            Close();
            return;
        }

        atleta.Nome = TxtNome.Text.Trim();
        atleta.Documento = string.IsNullOrWhiteSpace(TxtDocumento.Text) ? null : TxtDocumento.Text.Trim();
        atleta.Telefone = string.IsNullOrWhiteSpace(TxtTelefone.Text) ? null : TxtTelefone.Text.Trim();
        atleta.Email = string.IsNullOrWhiteSpace(TxtEmail.Text) ? null : TxtEmail.Text.Trim();
        atleta.DataNascimento = DpDataNascimento.SelectedDate.HasValue
            ? DateOnly.FromDateTime(DpDataNascimento.SelectedDate.Value)
            : null;

        var tagSexo = ((ComboBoxItem)CmbSexo.SelectedItem).Tag as string;
        atleta.Sexo = string.IsNullOrEmpty(tagSexo) ? null : tagSexo[0];

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
