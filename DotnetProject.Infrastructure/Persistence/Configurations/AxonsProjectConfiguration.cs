using DotnetProject.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetProject.Infrastructure.Persistence.Configurations
{
    public class AxonsProjectConfiguration : IEntityTypeConfiguration<AxonsProject>
    {
        private readonly string? _schemaName;

        public AxonsProjectConfiguration(string? schemaName = null)
        {
            _schemaName = schemaName;
        }

        public void Configure(EntityTypeBuilder<AxonsProject> builder)
        {
            builder.HasKey(e => new { e.Year, e.ProjectCode }).HasName("axons_project_pkey");
            builder.ToTable("axons_project", _schemaName);

            builder.Property(e => e.Year).HasColumnName("year");
            builder.Property(e => e.ProjectCode).HasColumnName("project_code");
            builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            builder.Property(e => e.ProjectName).HasColumnName("project_name");
            builder.Property(e => e.SubTeam).HasColumnName("sub_team");
        }
    }
}
