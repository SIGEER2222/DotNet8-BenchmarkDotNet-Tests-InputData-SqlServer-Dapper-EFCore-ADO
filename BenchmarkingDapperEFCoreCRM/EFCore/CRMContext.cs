using Microsoft.EntityFrameworkCore;

namespace BenchmarkingDapperEFCoreCRM.EFCore;

public class CRMContext : DbContext
{
    public DbSet<Empresa>? Empresas { get; set; }
    public DbSet<Contato>? Contatos { get; set; }

    public CRMContext(DbContextOptions<CRMContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>().HasMany(r => r.Contatos);
        modelBuilder.Entity<Contato>().HasOne(e => e.EmpresaRepresentada);
    }
}