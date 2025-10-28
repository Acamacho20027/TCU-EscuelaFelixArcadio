using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Estado> Estado { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<VarianteProducto> VarianteProducto { get; set; }
        public DbSet<NumeroSerieProducto> NumeroSerieProducto { get; set; }
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<TipoMovimientoInventario> TipoMovimientoInventario { get; set; }
        public DbSet<MovimientoInventario> MovimientoInventario { get; set; }
        public DbSet<Prestamo> Prestamo { get; set; }
        public DbSet<Reserva> Reserva { get; set; }
        public DbSet<Sancion> Sancion { get; set; }
        public DbSet<Espacio> Espacio { get; set; }
        public DbSet<ReservaEspacio> ReservaEspacio { get; set; }
        public DbSet<MantenimientoEspacio> MantenimientoEspacio { get; set; }
        public DbSet<Documento> Documento { get; set; }
        public DbSet<RecuperacionContrasena> RecuperacionContrasena { get; set; }

        // DbSets para Sistema de Reportes
        public DbSet<ConfiguracionReporte> ConfiguracionReporte { get; set; }
        public DbSet<PlantillaReporte> PlantillaReporte { get; set; }
        public DbSet<ReporteGuardado> ReporteGuardado { get; set; }
        public DbSet<PermisoReporte> PermisoReporte { get; set; }
        public DbSet<LogAccesoReporte> LogAccesoReporte { get; set; }
        public DbSet<ComentarioReporte> ComentarioReporte { get; set; }
        public DbSet<AlertaReporte> AlertaReporte { get; set; }
        public DbSet<HistorialAprobacionPrestamo> HistorialAprobacionPrestamo { get; set; }
        public DbSet<MensajeSistema> MensajeSistema { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Estado>().ToTable("Estado");
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Producto>().ToTable("Producto");
            modelBuilder.Entity<VarianteProducto>().ToTable("VarianteProducto");
            modelBuilder.Entity<NumeroSerieProducto>().ToTable("NumeroSerieProducto");
            modelBuilder.Entity<Inventario>().ToTable("Inventario");
            modelBuilder.Entity<TipoMovimientoInventario>().ToTable("TipoMovimientoInventario");
            modelBuilder.Entity<MovimientoInventario>().ToTable("MovimientoInventario");
            modelBuilder.Entity<Prestamo>().ToTable("Prestamo");
            modelBuilder.Entity<Reserva>().ToTable("Reserva");
            modelBuilder.Entity<Sancion>().ToTable("Sancion");
            modelBuilder.Entity<Espacio>().ToTable("Espacio");
            modelBuilder.Entity<ReservaEspacio>().ToTable("ReservaEspacio");
            modelBuilder.Entity<MantenimientoEspacio>().ToTable("MantenimientoEspacio");
            modelBuilder.Entity<Documento>().ToTable("Documento");
            modelBuilder.Entity<RecuperacionContrasena>().ToTable("RecuperacionContrasena");

            // Tablas para Sistema de Reportes
            modelBuilder.Entity<ConfiguracionReporte>().ToTable("ConfiguracionReporte");
            modelBuilder.Entity<PlantillaReporte>().ToTable("PlantillaReporte");
            modelBuilder.Entity<ReporteGuardado>().ToTable("ReporteGuardado");
            modelBuilder.Entity<PermisoReporte>().ToTable("PermisoReporte");
            modelBuilder.Entity<LogAccesoReporte>().ToTable("LogAccesoReporte");
            modelBuilder.Entity<ComentarioReporte>().ToTable("ComentarioReporte");
            modelBuilder.Entity<AlertaReporte>().ToTable("AlertaReporte");
            modelBuilder.Entity<HistorialAprobacionPrestamo>().ToTable("HistorialAprobacionPrestamo");
            modelBuilder.Entity<MensajeSistema>().ToTable("MensajeSistema");

            modelBuilder.Entity<Categoria>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<Espacio>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<Inventario>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<MantenimientoEspacio>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<NumeroSerieProducto>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<Prestamo>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<Producto>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<Reserva>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<ReservaEspacio>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);
            modelBuilder.Entity<Sancion>()
           .HasRequired(m => m.Estado)           // la propiedad de navegación
           .WithMany()                           // si no hay colección inversa
           .HasForeignKey(m => m.IdEstado)       // clave foránea
           .WillCascadeOnDelete(false);

            // Configuraciones para evitar cascada en tablas de Reportes
            modelBuilder.Entity<HistorialAprobacionPrestamo>()
                .HasRequired(h => h.UsuarioSolicitante)
                .WithMany()
                .HasForeignKey(h => h.IdUsuarioSolicitante)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<HistorialAprobacionPrestamo>()
                .HasRequired(h => h.UsuarioRevisor)
                .WithMany()
                .HasForeignKey(h => h.IdUsuarioRevisor)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<HistorialAprobacionPrestamo>()
                .HasRequired(h => h.Prestamo)
                .WithMany()
                .HasForeignKey(h => h.IdPrestamo)
                .WillCascadeOnDelete(false);
        }

        public System.Data.Entity.DbSet<EscuelaFelixArcadio.Models.ApplicationRol> IdentityRoles { get; set; }
    }
}