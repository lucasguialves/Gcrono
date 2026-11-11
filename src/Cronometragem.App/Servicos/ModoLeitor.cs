namespace Cronometragem.App.Servicos;

/// <summary>
/// Como um leitor configurado classifica as leituras que recebe:
/// - Largada/Chegada: todo evento desse leitor é sempre daquele ponto de controle fixo.
/// - AutomaticoUnico: um único leitor físico usado tanto na largada quanto na chegada — a
///   leitura de um chip sem largada confirmada ainda fica pendente; depois de confirmada
///   (ver ServicoLargadaAutomatica), a próxima leitura do mesmo chip já é chegada automaticamente.
/// </summary>
public enum ModoLeitor
{
    Largada,
    Chegada,
    AutomaticoUnico
}
