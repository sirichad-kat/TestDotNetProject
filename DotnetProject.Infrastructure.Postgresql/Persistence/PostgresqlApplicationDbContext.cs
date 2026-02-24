using DotnetProject.Infrastructure.Persistence;
using DotnetProject.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DotnetProject.Infrastructure.Postgresql.Persistence;

public class PostgresqlApplicationDbContext : ApplicationDbContext
{
    private readonly IConfiguration? _configuration;
    private readonly string? _connectionString;

    public PostgresqlApplicationDbContext(DbContextOptions<PostgresqlApplicationDbContext> options, IConfiguration configuration)
    : base(options)
    {
        _configuration = configuration;
        SchemaName = ExtractSchema(configuration, null);
    }

    internal PostgresqlApplicationDbContext(string connectionString)
    {
        _connectionString = connectionString;
        SchemaName = ExtractSchema(null, connectionString);
    }

    internal PostgresqlApplicationDbContext(IConfiguration configuration, string connectionStringKey = "DefaultConnection")
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString(connectionStringKey);
        SchemaName = ExtractSchema(configuration, _connectionString);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connStr = _connectionString
            ?? _configuration?.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is not found in configuration.");

        optionsBuilder.UseNpgsql(connStr);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations with schema name
        modelBuilder.ApplyConfiguration(new AxonsCollabConfiguration(SchemaName));
        modelBuilder.ApplyConfiguration(new AxonsMemberConfiguration(SchemaName));
        modelBuilder.ApplyConfiguration(new AxonsProjectConfiguration(SchemaName));
        modelBuilder.ApplyConfiguration(new DatabasechangelogConfiguration(SchemaName));
        modelBuilder.ApplyConfiguration(new DatabasechangeloglockConfiguration(SchemaName));
        modelBuilder.ApplyConfiguration(new FwInitConfiguration(SchemaName));

        OnModelCreatingPartial(modelBuilder);
    }

    private static string? ExtractSchema(IConfiguration? configuration, string? connectionString)
    {
        var schemaFromConfig = configuration?.GetSection("DatabaseSchema")?.Value;
        if (!string.IsNullOrEmpty(schemaFromConfig))
            return schemaFromConfig;

        if (string.IsNullOrEmpty(connectionString))
            return null;

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return builder.Username;
        }
        catch
        {
            return TryGetSchemaFromConnectionStringParts(connectionString);
        }
    }

    private static string? TryGetSchemaFromConnectionStringParts(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("Username=", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("User Id=", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("UserId=", StringComparison.OrdinalIgnoreCase))
            {
                var kv = trimmed.Split('=', 2);
                if (kv.Length == 2)
                    return kv[1].Trim();
            }
        }
        return null;
    }
}