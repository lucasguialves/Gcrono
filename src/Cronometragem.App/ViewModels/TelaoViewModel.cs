using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Cronometragem.App.Servicos;
using Cronometragem.Core.Entidades;

namespace Cronometragem.App.ViewModels;

public record ChegadaExibicao(
    int? PosicaoGeral,
    string NomeAtleta,
    string NomeModalidade,
    string NomeCategoria,
    TimeSpan? Tempo);

/// <summary>
/// Alimenta a janela "Telão": cada vez que uma leitura de chegada é processada (automática ou
/// manual) pra prova ativa, busca o resultado atualizado do atleta (posição geral já recalculada
/// com essa chegada) e insere no topo da lista exibida.
/// </summary>
public partial class TelaoViewModel : ObservableObject, IDisposable
{
    private readonly ServicoCronometragem _servicoCronometragem;
    private readonly ServicoResultados _servicoResultados;
    private readonly EstadoAplicacao _estado;

    public ObservableCollection<ChegadaExibicao> Chegadas { get; } = new();

    public Prova? ProvaAtiva => _estado.ProvaAtiva;

    public TelaoViewModel(
        ServicoCronometragem servicoCronometragem,
        ServicoResultados servicoResultados,
        EstadoAplicacao estado)
    {
        _servicoCronometragem = servicoCronometragem;
        _servicoResultados = servicoResultados;
        _estado = estado;
        _servicoCronometragem.LeituraProcessada += OnLeituraProcessada;
    }

    private async void OnLeituraProcessada(object? sender, LeituraProcessadaEventArgs e)
    {
        if (_estado.ProvaAtiva is null || e.Leitura.ProvaId != _estado.ProvaAtiva.Id)
            return;

        if (e.Leitura.PontoControle != PontoControle.Chegada || e.Inscricao is null)
            return;

        var resultados = await _servicoResultados.ObterResultadosAsync(_estado.ProvaAtiva.Id);
        var resultado = resultados.FirstOrDefault(r => r.Resultado.InscricaoId == e.Inscricao.Id);
        if (resultado is null)
            return;

        var chegada = new ChegadaExibicao(
            resultado.PosicaoGeral,
            resultado.NomeAtleta,
            resultado.NomeModalidade,
            resultado.NomeCategoria,
            resultado.Resultado.TempoLiquido ?? resultado.Resultado.TempoBruto);

        Application.Current.Dispatcher.Invoke(() => Chegadas.Insert(0, chegada));
    }

    public void Dispose()
    {
        _servicoCronometragem.LeituraProcessada -= OnLeituraProcessada;
        GC.SuppressFinalize(this);
    }
}
