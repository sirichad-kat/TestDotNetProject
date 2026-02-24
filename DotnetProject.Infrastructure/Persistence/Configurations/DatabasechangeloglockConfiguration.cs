using DotnetProject.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetProject.Infrastructure.Persistence.Configurations
{


    public class DatabasechangeloglockConfiguration : IEntityTypeConfiguration<Databasechangeloglock>
    {
        private readonly string? _schemaName;

        public DatabasechangeloglockConfiguration(string? schemaName = null)
        {
            _schemaName = schemaName;
        }

        public void Configure(EntityTypeBuilder<Databasechangeloglock> builder)
        {
            builder.HasKey(e => e.Id).HasName("databasechangeloglock_pkey");
            builder.ToTable("databasechangeloglock", _schemaName);

            builder.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            builder.Property(e => e.Locked).HasColumnName("locked");
            builder.Property(e => e.Lockedby).HasMaxLength(255).HasColumnName("lockedby");
            builder.Property(e => e.Lockgranted).HasColumnType("timestamp without time zone").HasColumnName("lockgranted");
        }
    }
}
