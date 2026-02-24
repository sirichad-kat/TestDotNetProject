using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DotnetProject.Infrastructure.Oracle.Persistence;

public partial class OracleDbContext : DbContext
{
    public OracleDbContext()
    {
    }

    public OracleDbContext(DbContextOptions<OracleDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FwInit> FwInits { get; set; }
     

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<FwInit>(entity =>
        {
            entity.HasKey(e => new { e.ProgramId, e.KeyName }).HasName("FW_INIT_PK");

            entity.ToTable("FW_INIT");

            entity.Property(e => e.ProgramId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PROGRAM_ID");
            entity.Property(e => e.KeyName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("KEY_NAME");
            entity.Property(e => e.Accessdate)
                .HasColumnType("DATE")
                .HasColumnName("ACCESSDATE");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("DATE")
                .HasColumnName("CREATED_DATE");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.Descr)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DESCR");
            entity.Property(e => e.Function)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("FUNCTION");
            entity.Property(e => e.Owner)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("OWNER");
            entity.Property(e => e.Programcode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PROGRAMCODE");
            entity.Property(e => e.Value)
                .HasMaxLength(2000)
                .IsUnicode(false)
                .HasColumnName("VALUE");
        }); 

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
