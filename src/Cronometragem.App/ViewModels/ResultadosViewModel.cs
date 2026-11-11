using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cronometragem.App.Servicos;
using Microsoft.Win32;

namespace Cronometragem.App.ViewModels;

public partial class ResultadosViewModel : ObservableObject
{
    private readonly ServicoResultados _servico;
    private readonly EstadoAplicacao _estado;

    [ObservableProperty]
    private ObservableCollection<ResultadoExibicao> _resultados = new();

    public ResultadosViewModel(ServicoResultados servico, EstadoAplicacao estado)
    {
        _servico = servico;
        _estado = estado;
    }

    [RelayCommand]
    private async Task AtualizarAsync()
    {
        if (_estado.ProvaAtiva is null)
        {
            MessageBox.Show("Selecione uma prova ativa na aba Provas.");
            return;
        }

        var lista = await _servico.ObterResultadosAsync(_estado.ProvaAtiva.Id);
        Resultados = new ObservableCollection<ResultadoExibicao>(lista);
    }

    [RelayCommand]
    private void ExportarCsv()
    {
        if (Resultados.Count == 0)
        {
            MessageBox.Show("Não há resultados carregados para exportar. Clique em Atualizar primeiro.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = "resultados.csv"
        };

        if (dialog.ShowDialog() != true)
            return;

        using var writer = new StreamWriter(dialog.FileName, append: false, Encoding.UTF8);
        writer.WriteLine("PosicaoGeral,PosicaoCategoria,Peito,Atleta,Categoria,TempoBruto,TempoLiquido,Situacao");

        foreach (var r in Resultados)
        {
            writer.WriteLine(string.Join(',',
                r.PosicaoGeral?.ToString() ?? "",
                r.PosicaoCategoria?.ToString() ?? "",
                r.Resultado.NumeroPeito,
                r.NomeAtleta,
                r.NomeCategoria,
                r.Resultado.TempoBruto?.ToString() ?? "",
                r.Resultado.TempoLiquido?.ToString() ?? "",
                r.Resultado.Situacao));
        }

        MessageBox.Show("Resultados exportados com sucesso.");
    }
}
