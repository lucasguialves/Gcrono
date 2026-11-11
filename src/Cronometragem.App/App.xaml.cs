using System.Windows;
using Cronometragem.App.Servicos;
using Cronometragem.App.ViewModels;
using Cronometragem.Core.Cronometragem;
using Cronometragem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cronometragem.App;

public partial class App : Application
{
    private IHost? _host;

    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        _host = Host.CreateDefaultBuilder()
            // Host.CreateDefaultBuilder resolve appsettings.json relativo ao diretório de
            // trabalho do processo (Directory.GetCurrentDirectory()), que varia conforme como o
            // app é iniciado (ex.: "dotnet run" de outra pasta). Fixamos o content root no
            // diretório real do executável para sempre achar os appsettings corretos.
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((_, config) =>
            {
                // appsettings.Local.json fica fora do git (.gitignore) — é onde a connection
                // string real com senha deve ficar, sobrescrevendo o placeholder de appsettings.json.
                config.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("Cronometragem")
                    ?? "Host=localhost;Database=cronometragem;Username=postgres;Password=postgres";

                services.AddDbContextFactory<CronometragemDbContext>(options =>
                    options.UseNpgsql(connectionString, o => o.SetPostgresVersion(9, 6)));

                services.AddSingleton<ICalculadoraTempo>(new CalculadoraTempo());
                services.AddSingleton<EstadoAplicacao>();
                services.AddSingleton<ServicoCronometragem>();
                services.AddSingleton<ServicoLargadaAutomatica>();
                services.AddSingleton<ServicoResultados>();
                services.AddSingleton<GerenciadorLeitores>();

                services.AddSingleton<ProvasViewModel>();
                services.AddSingleton<AtletasViewModel>();
                services.AddSingleton<ConfiguracaoLeitoresViewModel>();
                services.AddSingleton<PainelAoVivoViewModel>();
                services.AddSingleton<ResultadosViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();

        Services = _host.Services;

        AplicarMigracoesComTratamentoDeErro();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void AplicarMigracoesComTratamentoDeErro()
    {
        try
        {
            var dbFactory = Services.GetRequiredService<IDbContextFactory<CronometragemDbContext>>();
            using var db = dbFactory.CreateDbContext();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível conectar/migrar o banco de dados PostgreSQL. Verifique a " +
                "connection string em appsettings.json.\n\nDetalhe: " + ex.Message,
                "Erro de banco de dados", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// Rede de segurança: qualquer exceção não tratada que chegue até o thread de UI mostra um
    /// erro em vez de derrubar o app inteiro (padrão do WPF sem este handler é encerrar o
    /// processo). Importante num sistema que roda ao vivo durante uma prova.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "Ocorreu um erro inesperado. O aplicativo continuará rodando.\n\nDetalhe: " + e.Exception,
            "Erro inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show(
                "Ocorreu um erro inesperado fora do thread de interface.\n\nDetalhe: " + ex,
                "Erro inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Evita que uma Task "fire-and-forget" (ex.: CarregarAsync chamado no construtor de uma
        // ViewModel) deixe uma exceção não observada — apenas marca como observada, sem UI aqui
        // (já seria tarde demais para associar a um contexto útil pro usuário).
        e.SetObserved();
    }
}
