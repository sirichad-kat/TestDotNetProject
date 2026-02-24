using DotnetProject.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetProject.Infrastructure.Persistence.Configurations
{


    public class AxonsMemberConfiguration : IEntityTypeConfiguration<AxonsMember>
    {
        private readonly string? _schemaName;

        public AxonsMemberConfiguration(string? schemaName = null)
        {
            _schemaName = schemaName;
        }

        public void Configure(EntityTypeBuilder<AxonsMember> builder)
        {
            builder.HasKey(e => e.MemberId).HasName("axons_member_pkey");
            builder.ToTable("axons_member", _schemaName);

            builder.HasIndex(e => e.Email, "axons_member_email_key").IsUnique();
            builder.HasIndex(e => e.Username, "axons_member_username_key").IsUnique();
            builder.HasIndex(e => e.Department, "idx_member_department");
            builder.HasIndex(e => e.FullName, "idx_member_fullname");
            builder.HasIndex(e => e.SquadName, "idx_member_squad");
            builder.HasIndex(e => e.Status, "idx_member_status");

            builder.Property(e => e.MemberId).HasColumnName("member_id");
            builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");
            builder.Property(e => e.Department).HasColumnName("department");
            builder.Property(e => e.Email).HasColumnName("email");
            builder.Property(e => e.FullName).HasColumnName("full_name");
            builder.Property(e => e.NoQa).HasMaxLength(1).HasDefaultValueSql("'Y'::bpchar").HasColumnName("no_qa");
            builder.Property(e => e.Role).HasColumnName("role");
            builder.Property(e => e.SquadName).HasColumnName("squad_name");
            builder.Property(e => e.Status).HasMaxLength(1).HasDefaultValueSql("'A'::bpchar").HasColumnName("status");
            builder.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").HasColumnName("updated_at");
            builder.Property(e => e.Username).HasColumnName("username");
        }
    }
}
