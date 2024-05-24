using Microsoft.EntityFrameworkCore;
using SqlSugar;

namespace BenchmarkingDapperEFCoreCRM.EFCore;

public class CRMContext : DbContext
{
    public DbSet<Empresa>? Empresas { get; set; }
    public DbSet<Contato>? Contatos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(Configurations.BaseEFCore);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>().HasMany(r => r.Contatos);
        modelBuilder.Entity<Contato>().HasOne(e => e.EmpresaRepresentada);
    }
}