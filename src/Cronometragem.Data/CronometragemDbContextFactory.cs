using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cronometragem.Data;

/// <summary>
/// Usado apenas em tempo de design pelo `dotnet ef migrations` — a aplicação real monta as
/// options via DI (Cronometragem.App) com a connection string vinda de configuração.
/// </summary>
public class CronometragemDbContextFactory : IDesignTimeDbContextFactory<CronometragemDbContext>
{
    public CronometragemDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CRONOMETRAGEM_CONNECTION_STRING")
            ?? "Host=localhost;Database=cronometragem;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<CronometragemDbContext>();
        optionsBuilder.UseNpgsql(connectionString, o => o.SetPostgresVersion(9, 6));

        return new CronometragemDbContext(optionsBuilder.Options);
    }
}
