using DotnetProject.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetProject.Infrastructure.Persistence.Configurations
{

    public class DatabasechangelogConfiguration : IEntityTypeConfiguration<Databasechangelog>
    {
        private readonly string? _schemaName;

        public DatabasechangelogConfiguration(string? schemaName = null)
        {
            _schemaName = schemaName;
        }

        public void Configure(EntityTypeBuilder<Databasechangelog> builder)
        {
            builder.HasNoKey();
            builder.ToTable("databasechangelog", _schemaName);

            builder.Property(e => e.Author).HasMaxLength(255).HasColumnName("author");
            builder.Property(e => e.Comments).HasMaxLength(255).HasColumnName("comments");
            builder.Property(e => e.Contexts).HasMaxLength(255).HasColumnName("contexts");
            builder.Property(e => e.Dateexecuted).HasColumnType("timestamp without time zone").HasColumnName("dateexecuted");
            builder.Property(e => e.DeploymentId).HasMaxLength(10).HasColumnName("deployment_id");
            builder.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            builder.Property(e => e.Exectype).HasMaxLength(10).HasColumnName("exectype");
            builder.Property(e => e.Filename).HasMaxLength(255).HasColumnName("filename");
            builder.Property(e => e.Id).HasMaxLength(255).HasColumnName("id");
            builder.Property(e => e.Labels).HasMaxLength(255).HasColumnName("labels");
            builder.Property(e => e.Liquibase).HasMaxLength(20).HasColumnName("liquibase");
            builder.Property(e => e.Md5sum).HasMaxLength(35).HasColumnName("md5sum");
            builder.Property(e => e.Orderexecuted).HasColumnName("orderexecuted");
            builder.Property(e => e.Tag).HasMaxLength(255).HasColumnName("tag");
        }
    }
}
