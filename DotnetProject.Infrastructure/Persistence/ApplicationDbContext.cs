
using DotnetProject.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetProject.Infrastructure.Persistence;

public partial class ApplicationDbContext : DbContext
{
    protected string? SchemaName { get; init; }

    public ApplicationDbContext()
    {
    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions options)
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        OnModelCreatingPartial(modelBuilder);
    }

    protected virtual void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Optionally leave empty or provide partial customization
    }
}