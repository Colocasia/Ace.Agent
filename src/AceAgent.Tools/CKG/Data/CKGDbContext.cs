using Microsoft.EntityFrameworkCore;
using AceAgent.Tools.CKG.Models;

namespace AceAgent.Tools.CKG.Data;

public class CKGDbContext : DbContext
{
    public DbSet<Function> Functions { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Field> Fields { get; set; }
    public DbSet<Variable> Variables { get; set; }

    public CKGDbContext(DbContextOptions<CKGDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Function entity
        modelBuilder.Entity<Function>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ReturnType).HasMaxLength(200);
            entity.Property(e => e.Parameters).HasMaxLength(2000);
            entity.Property(e => e.Modifiers).HasMaxLength(200);
            entity.Property(e => e.ClassName).HasMaxLength(500);
            entity.Property(e => e.Namespace).HasMaxLength(500);
            entity.Property(e => e.ProjectPath).HasMaxLength(1000);
            entity.Property(e => e.CommitHash).HasMaxLength(100);
            entity.Property(e => e.Documentation).HasMaxLength(4000);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.FilePath);
            entity.HasIndex(e => e.ClassName);
            entity.HasIndex(e => e.Namespace);
        });

        // Configure Class entity
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Namespace).HasMaxLength(500);
            entity.Property(e => e.Modifiers).HasMaxLength(200);
            entity.Property(e => e.BaseClass).HasMaxLength(500);
            entity.Property(e => e.Interfaces).HasMaxLength(2000);
            entity.Property(e => e.ProjectPath).HasMaxLength(1000);
            entity.Property(e => e.CommitHash).HasMaxLength(100);
            entity.Property(e => e.Documentation).HasMaxLength(4000);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.FilePath);
            entity.HasIndex(e => e.Namespace);
        });

        // Configure Property entity
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ClassName).HasMaxLength(500);
            entity.Property(e => e.Namespace).HasMaxLength(500);
            entity.Property(e => e.Modifiers).HasMaxLength(200);
            entity.Property(e => e.ProjectPath).HasMaxLength(1000);
            entity.Property(e => e.CommitHash).HasMaxLength(100);
            entity.Property(e => e.Documentation).HasMaxLength(4000);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.FilePath);
            entity.HasIndex(e => e.ClassName);
            entity.HasIndex(e => e.Namespace);
        });

        // Configure Field entity
        modelBuilder.Entity<Field>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ClassName).HasMaxLength(500);
            entity.Property(e => e.Namespace).HasMaxLength(500);
            entity.Property(e => e.Modifiers).HasMaxLength(200);
            entity.Property(e => e.DefaultValue).HasMaxLength(1000);
            entity.Property(e => e.ProjectPath).HasMaxLength(1000);
            entity.Property(e => e.CommitHash).HasMaxLength(100);
            entity.Property(e => e.Documentation).HasMaxLength(4000);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.FilePath);
            entity.HasIndex(e => e.ClassName);
            entity.HasIndex(e => e.Namespace);
        });

        // Configure Variable entity
        modelBuilder.Entity<Variable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FunctionName).HasMaxLength(500);
            entity.Property(e => e.ClassName).HasMaxLength(500);
            entity.Property(e => e.Namespace).HasMaxLength(500);
            entity.Property(e => e.Scope).HasMaxLength(50);
            entity.Property(e => e.DefaultValue).HasMaxLength(1000);
            entity.Property(e => e.ProjectPath).HasMaxLength(1000);
            entity.Property(e => e.CommitHash).HasMaxLength(100);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.FilePath);
            entity.HasIndex(e => e.FunctionName);
            entity.HasIndex(e => e.ClassName);
            entity.HasIndex(e => e.Namespace);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Default SQLite database
            optionsBuilder.UseSqlite("Data Source=ckg.db");
        }
    }
}