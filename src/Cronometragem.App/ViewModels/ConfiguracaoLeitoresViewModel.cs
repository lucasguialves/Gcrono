using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cronometragem.App.Servicos;
using Cronometragem.Readers;

namespace Cronometragem.App.ViewModels;

public partial class ConfiguracaoLeitoresViewModel : ObservableObject
{
    private readonly GerenciadorLeitores _gerenciador;

    public ObservableCollection<LeitorConfigurado> Leitores => _gerenciador.Leitores;

    public Array ModosDisponiveis { get; } = Enum.GetValues(typeof(ModoLeitor));

    [ObservableProperty] private string _novoNomeLeitor = "Simulador";
    [ObservableProperty] private ModoLeitor _novoModo = ModoLeitor.AutomaticoUnico;

    [ObservableProperty] private LeitorConfigurado? _leitorSelecionado;
    [ObservableProperty] private string _chipParaSimular = string.Empty;

    public ConfiguracaoLeitoresViewModel(GerenciadorLeitores gerenciador)
    {
        _gerenciador = gerenciador;
    }

    [RelayCommand]
    private void AdicionarLeitorSimulado()
    {
        if (string.IsNullOrWhiteSpace(NovoNomeLeitor))
        {
            MessageBox.Show("Informe um nome para o leitor.");
            return;
        }

        _gerenciador.AdicionarLeitor(new MockChipReader(NovoNomeLeitor), NovoModo);
        NovoNomeLeitor = string.Empty;
    }

    [RelayCommand]
    private async Task ConectarAsync(LeitorConfigurado? leitor)
    {
        if (leitor is null) return;
        await _gerenciador.ConectarAsync(leitor);
    }

    [RelayCommand]
    private async Task DesconectarAsync(LeitorConfigurado? leitor)
    {
        if (leitor is null) return;
        await _gerenciador.DesconectarAsync(leitor);
    }

    [RelayCommand]
    private void SimularLeitura()
    {
        if (LeitorSelecionado is null)
        {
            MessageBox.Show("Selecione um leitor na lista.");
            return;
        }

        if (LeitorSelecionado.Leitor is not MockChipReader mock)
        {
            MessageBox.Show("Simular leitura só está disponível para leitores do tipo Simulador.");
            return;
        }

        if (!LeitorSelecionado.Conectado)
        {
            MessageBox.Show("Conecte o leitor antes de simular uma leitura.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ChipParaSimular))
        {
            MessageBox.Show("Informe o identificador do chip.");
            return;
        }

        mock.SimularLeitura(ChipParaSimular);
        ChipParaSimular = string.Empty;
    }
}
