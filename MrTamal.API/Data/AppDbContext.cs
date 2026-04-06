using Microsoft.EntityFrameworkCore;
using MrTamal.Shared.Models;

namespace MrTamal.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Catalogo> Catalogos => Set<Catalogo>();
    public DbSet<Ingreso> Ingresos => Set<Ingreso>();
    public DbSet<Egreso> Egresos => Set<Egreso>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<AsignacionSucursal> AsignacionesSucursal => Set<AsignacionSucursal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Catalogo>().HasIndex(c => new { c.Codigo, c.Tipo }).IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Sucursal).WithMany().HasForeignKey(u => u.SucursalId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ingreso>()
            .HasOne(i => i.Catalogo).WithMany().HasForeignKey(i => i.CatalogoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Ingreso>()
            .HasOne(i => i.Usuario).WithMany().HasForeignKey(i => i.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Ingreso>()
            .HasOne(i => i.Sucursal).WithMany().HasForeignKey(i => i.SucursalId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Egreso>()
            .HasOne(e => e.Catalogo).WithMany().HasForeignKey(e => e.CatalogoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Egreso>()
            .HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Egreso>()
            .HasOne(e => e.Sucursal).WithMany().HasForeignKey(e => e.SucursalId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Ingreso>().Property(i => i.Cantidad).HasPrecision(18, 2);
        modelBuilder.Entity<Egreso>().Property(e => e.Cantidad).HasPrecision(18, 2);
        modelBuilder.Entity<MenuItem>().Property(m => m.Precio).HasPrecision(18, 2);
        modelBuilder.Entity<Sucursal>().Property(s => s.Pais).HasDefaultValue("US");
        modelBuilder.Entity<Sucursal>().Property(s => s.Moneda).HasDefaultValue("USD");
        modelBuilder.Entity<Sucursal>().Property(s => s.SimboloMoneda).HasDefaultValue("$");

        modelBuilder.Entity<AsignacionSucursal>()
            .HasOne(a => a.Usuario).WithMany().HasForeignKey(a => a.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AsignacionSucursal>()
            .HasOne(a => a.Sucursal).WithMany().HasForeignKey(a => a.SucursalId).OnDelete(DeleteBehavior.Restrict);
    }
}
