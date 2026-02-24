using DotnetProject.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetProject.Infrastructure.Persistence.Configurations
{

    public class FwInitConfiguration : IEntityTypeConfiguration<FwInit>
    {
        private readonly string? _schemaName;

        public FwInitConfiguration(string? schemaName = null)
        {
            _schemaName = schemaName;
        }

        public void Configure(EntityTypeBuilder<FwInit> builder)
        {
            builder.HasKey(e => new { e.KeyName, e.ProgramId }).HasName("fw_init_pk");
            builder.ToTable("fw_init", _schemaName);

            builder.Property(e => e.KeyName).HasColumnName("key_name");
            builder.Property(e => e.ProgramId).HasColumnName("program_id");
            builder.Property(e => e.Descr).HasColumnName("descr");
            builder.Property(e => e.Value).HasColumnName("value");
        }
    }
}
