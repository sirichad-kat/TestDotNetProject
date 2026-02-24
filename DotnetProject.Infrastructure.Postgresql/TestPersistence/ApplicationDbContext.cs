using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DotnetProject.Infrastructure.Postgresql.TestPersistence;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AxonsCollab> AxonsCollabs { get; set; }

    public virtual DbSet<AxonsMember> AxonsMembers { get; set; }

    public virtual DbSet<AxonsProject> AxonsProjects { get; set; }

    public virtual DbSet<Databasechangelog> Databasechangelogs { get; set; }

    public virtual DbSet<Databasechangeloglock> Databasechangeloglocks { get; set; }

    public virtual DbSet<FwInit> FwInits { get; set; }
     

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AxonsCollab>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("axons_collab_pkey");

            entity.ToTable("axons_collab", "dotnet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('axons_collab_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CategoryCode).HasColumnName("category_code");
            entity.Property(e => e.GivenDate).HasColumnName("given_date");
            entity.Property(e => e.GivenFullname).HasColumnName("given_fullname");
            entity.Property(e => e.GivenUser).HasColumnName("given_user");
            entity.Property(e => e.Remark).HasColumnName("remark");
            entity.Property(e => e.Sprint).HasColumnName("sprint");
            entity.Property(e => e.StarFullname).HasColumnName("star_fullname");
            entity.Property(e => e.StarUser).HasColumnName("star_user");
            entity.Property(e => e.SubTeam).HasColumnName("sub_team");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<AxonsMember>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("axons_member_pkey");

            entity.ToTable("axons_member", "dotnet");

            entity.HasIndex(e => e.Email, "axons_member_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "axons_member_username_key").IsUnique();

            entity.HasIndex(e => e.Department, "idx_member_department");

            entity.HasIndex(e => e.FullName, "idx_member_fullname");

            entity.HasIndex(e => e.SquadName, "idx_member_squad");

            entity.HasIndex(e => e.Status, "idx_member_status");

            entity.Property(e => e.MemberId)
                .HasDefaultValueSql("nextval('axons_member_member_id_seq'::regclass)")
                .HasColumnName("member_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Department).HasColumnName("department");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.NoQa)
                .HasMaxLength(1)
                .HasDefaultValueSql("'Y'::bpchar")
                .HasColumnName("no_qa");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.SquadName).HasColumnName("squad_name");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .HasDefaultValueSql("'A'::bpchar")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<AxonsProject>(entity =>
        {
            entity.HasKey(e => new { e.Year, e.ProjectCode }).HasName("axons_project_pkey");

            entity.ToTable("axons_project", "dotnet");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ProjectCode).HasColumnName("project_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.ProjectName).HasColumnName("project_name");
            entity.Property(e => e.SubTeam).HasColumnName("sub_team");
        });

        modelBuilder.Entity<Databasechangelog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("databasechangelog", "dotnet");

            entity.Property(e => e.Author)
                .HasMaxLength(255)
                .HasColumnName("author");
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .HasColumnName("comments");
            entity.Property(e => e.Contexts)
                .HasMaxLength(255)
                .HasColumnName("contexts");
            entity.Property(e => e.Dateexecuted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateexecuted");
            entity.Property(e => e.DeploymentId)
                .HasMaxLength(10)
                .HasColumnName("deployment_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Exectype)
                .HasMaxLength(10)
                .HasColumnName("exectype");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.Labels)
                .HasMaxLength(255)
                .HasColumnName("labels");
            entity.Property(e => e.Liquibase)
                .HasMaxLength(20)
                .HasColumnName("liquibase");
            entity.Property(e => e.Md5sum)
                .HasMaxLength(35)
                .HasColumnName("md5sum");
            entity.Property(e => e.Orderexecuted).HasColumnName("orderexecuted");
            entity.Property(e => e.Tag)
                .HasMaxLength(255)
                .HasColumnName("tag");
        });

        modelBuilder.Entity<Databasechangeloglock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("databasechangeloglock_pkey");

            entity.ToTable("databasechangeloglock", "dotnet");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Locked).HasColumnName("locked");
            entity.Property(e => e.Lockedby)
                .HasMaxLength(255)
                .HasColumnName("lockedby");
            entity.Property(e => e.Lockgranted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lockgranted");
        });

        modelBuilder.Entity<FwInit>(entity =>
        {
            entity.HasKey(e => new { e.KeyName, e.ProgramId }).HasName("fw_init_pk");

            entity.ToTable("fw_init", "dotnet");

            entity.Property(e => e.KeyName).HasColumnName("key_name");
            entity.Property(e => e.ProgramId).HasColumnName("program_id");
            entity.Property(e => e.Descr).HasColumnName("descr");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
