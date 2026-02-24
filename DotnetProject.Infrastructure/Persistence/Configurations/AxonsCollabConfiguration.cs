using DotnetProject.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetProject.Infrastructure.Persistence.Configurations
{

    public class AxonsCollabConfiguration : IEntityTypeConfiguration<AxonsCollab>
    {
        private readonly string? _schemaName;

        public AxonsCollabConfiguration(string? schemaName = null)
        {
            _schemaName = schemaName;
        }

        public void Configure(EntityTypeBuilder<AxonsCollab> builder)
        {
            builder.HasKey(e => e.Id).HasName("axons_collab_pkey");
            builder.ToTable("axons_collab", _schemaName);

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.CategoryCode).HasColumnName("category_code");
            builder.Property(e => e.GivenDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("given_date");
            builder.Property(e => e.GivenFullname).HasColumnName("given_fullname");
            builder.Property(e => e.GivenUser).HasColumnName("given_user");
            builder.Property(e => e.Remark).HasColumnName("remark");
            builder.Property(e => e.Sprint).HasColumnName("sprint");
            builder.Property(e => e.StarFullname).HasColumnName("star_fullname");
            builder.Property(e => e.StarUser).HasColumnName("star_user");
            builder.Property(e => e.SubTeam).HasColumnName("sub_team");
            builder.Property(e => e.Year).HasColumnName("year");
        }
    }
}
