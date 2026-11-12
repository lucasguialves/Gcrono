using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Cronometragem.App.Servicos;
using Cronometragem.Core.Cronometragem;
using Cronometragem.Core.Entidades;

namespace Cronometragem.App.ViewModels;

public record ChegadaExibicao(
    int? PosicaoGeral,
    string NomeAtleta,
    string NomeModalidade,
    string NomeCategoria,
    TimeSpan? Tempo);

/// <summary>
/// Alimenta a janela "Telão": a cada leitura de chegada processada (automática ou manual) pra
/// prova ativa, recarrega o ranking COMPLETO e oficial (ServicoResultados) e reconstrói a lista
/// inteira — nunca guarda uma posição "congelada" no momento da leitura, porque duas chegadas
/// processadas em sequência rápida (ex.: vários atletas cruzando a linha quase juntos) podiam
/// fazer um card antigo mostrar uma posição desatualizada em relação à classificação final.
/// A lista fica ordenada pela chegada mais recente primeiro — é o que o locutor precisa ver
/// (quem acabou de passar agora), não o ranking geral (que já está na tela de Resultados).
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

        if (e.Leitura.PontoControle != PontoControle.Chegada)
            return;

        await AtualizarChegadasAsync(_estado.ProvaAtiva.Id);
    }

    private async Task AtualizarChegadasAsync(int provaId)
    {
        var resultados = await _servicoResultados.ObterResultadosAsync(provaId);

        var chegadas = resultados
            .Where(r => r.Resultado.Situacao == SituacaoResultado.Finalizado)
            .OrderByDescending(r => r.Resultado.TempoChegada)
            .Select(r => new ChegadaExibicao(
                r.PosicaoGeral,
                r.NomeAtleta,
                r.NomeModalidade,
                r.NomeCategoria,
                r.Resultado.TempoBruto))
            .ToList();

        Application.Current.Dispatcher.Invoke(() =>
        {
            Chegadas.Clear();
            foreach (var chegada in chegadas)
                Chegadas.Add(chegada);
        });
    }

    public void Dispose()
    {
        _servicoCronometragem.LeituraProcessada -= OnLeituraProcessada;
        GC.SuppressFinalize(this);
    }
}
