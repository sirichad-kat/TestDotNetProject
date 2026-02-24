//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Npgsql;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace DotnetProject.Infrastructure.Postgresql.Persistence
//{
//    public partial class PostgresqlApplicationDbContext
//    {
//        private readonly string? _connectionString;
//        private readonly IConfiguration? _configuration;
//        private string? _schemaName;
//        public string? SchemaName => _schemaName;
//        public PostgresqlApplicationDbContext(DbContextOptions<PostgresqlApplicationDbContext> options, IConfiguration configuration) : base(options)
//        {
//            _configuration = configuration;
//            ExtractSchemaFromConnectionString();
//        }

//        internal PostgresqlApplicationDbContext(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        internal PostgresqlApplicationDbContext(IConfiguration configuration, string connectionStringKey = "DefaultConnection")
//        {
//            _configuration = configuration;
//            _connectionString = configuration.GetConnectionString(connectionStringKey);
//            ExtractSchemaFromConnectionString();
//        }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            if (!optionsBuilder.IsConfigured)
//            {
//                if (!string.IsNullOrEmpty(_connectionString))
//                {
//                    optionsBuilder.UseNpgsql(_connectionString);
//                }
//                else if (_configuration != null)
//                {
//                    var defaultConnectionString = _configuration.GetConnectionString("DefaultConnection");
//                    if (!string.IsNullOrEmpty(defaultConnectionString))
//                    {
//                        optionsBuilder.UseNpgsql(defaultConnectionString);
//                    }
//                    else
//                    {
//                        throw new InvalidOperationException("DefaultConnection string is not found in configuration.");
//                    }
//                }
//                else
//                {
//                    throw new InvalidOperationException("DefaultConnection string is not found in configuration.");
//                }
//            }
//        }

//        private void ExtractSchemaFromConnectionString()
//        {
//            try
//            {
//                if (TryGetSchemaFromConfiguration(out _schemaName))
//                    return;

//                if (!string.IsNullOrEmpty(_connectionString))
//                {
//                    if (TryGetSchemaFromNpgsqlBuilder(_connectionString, out _schemaName))
//                        return;

//                    _schemaName = TryGetSchemaFromConnectionStringParts(_connectionString);
//                }
//            }
//            catch
//            {
//                _schemaName = null;
//            }
//        }

//        private bool TryGetSchemaFromConfiguration(out string? schemaName)
//        {
//            schemaName = _configuration?.GetSection("DatabaseSchema")?.Value;
//            return !string.IsNullOrEmpty(schemaName);
//        }

//        private static bool TryGetSchemaFromNpgsqlBuilder(string connectionString, out string? schemaName)
//        {
//            try
//            {
//                var builder = new NpgsqlConnectionStringBuilder(connectionString);
//                schemaName = builder.Username;
//                return true;
//            }
//            catch
//            {
//                schemaName = null;
//                return false;
//            }
//        }

//        private static string? TryGetSchemaFromConnectionStringParts(string connectionString)
//        {
//            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
//            foreach (var part in parts)
//            {
//                var trimmedPart = part.Trim();
//                if (trimmedPart.StartsWith("Username=", StringComparison.OrdinalIgnoreCase) ||
//                    trimmedPart.StartsWith("User Id=", StringComparison.OrdinalIgnoreCase) ||
//                    trimmedPart.StartsWith("UserId=", StringComparison.OrdinalIgnoreCase))
//                {
//                    var keyValue = trimmedPart.Split('=', 2);
//                    if (keyValue.Length == 2)
//                    {
//                        return keyValue[1].Trim();
//                    }
//                }
//            }
//            return null;
//        }
//    }
//}
