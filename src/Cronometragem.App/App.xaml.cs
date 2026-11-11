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

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("Cronometragem")
                    ?? "Host=localhost;Database=cronometragem;Username=postgres;Password=postgres";

                services.AddDbContextFactory<CronometragemDbContext>(options =>
                    options.UseNpgsql(connectionString));

                services.AddSingleton<ICalculadoraTempo>(new CalculadoraTempo());
                services.AddSingleton<EstadoAplicacao>();
                services.AddSingleton<ServicoCronometragem>();
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
}
