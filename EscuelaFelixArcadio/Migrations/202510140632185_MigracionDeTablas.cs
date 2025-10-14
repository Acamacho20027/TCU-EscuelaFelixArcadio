namespace EscuelaFelixArcadio.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigracionDeTablas : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categoria",
                c => new
                    {
                        IdCategoria = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 150),
                        IdCategoriaPadre = c.Int(),
                        Descripcion = c.String(maxLength: 500),
                        FechaCreacion = c.DateTime(nullable: false),
                        IdEstado = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IdCategoria)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.Estado",
                c => new
                    {
                        IdEstado = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.IdEstado);
            
            CreateTable(
                "dbo.Documento",
                c => new
                    {
                        IdDocumento = c.Long(nullable: false, identity: true),
                        Titulo = c.String(nullable: false, maxLength: 250),
                        Descripcion = c.String(maxLength: 1000),
                        Id = c.String(maxLength: 128),
                        FechaSubida = c.DateTime(nullable: false),
                        Publico = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdDocumento)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IntentosFallidos = c.Int(nullable: false),
                        EstaBloqueado = c.Boolean(nullable: false),
                        FechaBloqueo = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MantenimientoEspacio",
                c => new
                    {
                        IdMantenimiento = c.Long(nullable: false, identity: true),
                        IdEspacio = c.Int(nullable: false),
                        Id = c.String(maxLength: 128),
                        Descripcion = c.String(maxLength: 1000),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(),
                        IdEstado = c.Int(nullable: false),
                        Costo = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.IdMantenimiento)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.Espacio", t => t.IdEspacio, cascadeDelete: true)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .Index(t => t.IdEspacio)
                .Index(t => t.Id)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.Espacio",
                c => new
                    {
                        IdEspacio = c.Int(nullable: false, identity: true),
                        Codigo = c.String(maxLength: 100),
                        Nombre = c.String(nullable: false, maxLength: 250),
                        Descripcion = c.String(maxLength: 1000),
                        Capacidad = c.Int(),
                        Ubicacion = c.String(maxLength: 250),
                        IdEstado = c.Int(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdEspacio)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.MovimientoInventario",
                c => new
                    {
                        IdMovimiento = c.Long(nullable: false, identity: true),
                        IdProducto = c.Int(nullable: false),
                        IdVariante = c.Int(),
                        IdSerie = c.Long(),
                        IdEstadoInventario = c.Int(nullable: false),
                        Cantidad = c.Int(nullable: false),
                        TipoMovimiento = c.String(nullable: false, maxLength: 50),
                        Referencia = c.String(maxLength: 200),
                        Notas = c.String(maxLength: 1000),
                        Id = c.String(maxLength: 128),
                        FechaMovimiento = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdMovimiento)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.Producto", t => t.IdProducto, cascadeDelete: true)
                .ForeignKey("dbo.NumeroSerieProducto", t => t.IdSerie)
                .ForeignKey("dbo.TipoMovimientoInventario", t => t.IdEstadoInventario, cascadeDelete: true)
                .ForeignKey("dbo.VarianteProducto", t => t.IdVariante)
                .Index(t => t.IdProducto)
                .Index(t => t.IdVariante)
                .Index(t => t.IdSerie)
                .Index(t => t.IdEstadoInventario)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Producto",
                c => new
                    {
                        IdProducto = c.Int(nullable: false, identity: true),
                        Codigo = c.String(nullable: false, maxLength: 100),
                        Nombre = c.String(nullable: false, maxLength: 250),
                        Descripcion = c.String(),
                        Marca = c.String(maxLength: 150),
                        EsServicio = c.Boolean(nullable: false),
                        Eliminado = c.Boolean(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                        IdEstado = c.Int(nullable: false),
                        IdCategoria = c.Int(),
                    })
                .PrimaryKey(t => t.IdProducto)
                .ForeignKey("dbo.Categoria", t => t.IdCategoria)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .Index(t => t.IdEstado)
                .Index(t => t.IdCategoria);
            
            CreateTable(
                "dbo.NumeroSerieProducto",
                c => new
                    {
                        IdSerie = c.Long(nullable: false, identity: true),
                        IdVariante = c.Int(),
                        IdProducto = c.Int(nullable: false),
                        NumeroSerie = c.String(nullable: false, maxLength: 200),
                        IdEstado = c.Int(nullable: false),
                        Ubicacion = c.String(maxLength: 200),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdSerie)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .ForeignKey("dbo.Producto", t => t.IdProducto, cascadeDelete: true)
                .ForeignKey("dbo.VarianteProducto", t => t.IdVariante)
                .Index(t => t.IdVariante)
                .Index(t => t.IdProducto)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.VarianteProducto",
                c => new
                    {
                        IdVariante = c.Int(nullable: false, identity: true),
                        IdProducto = c.Int(nullable: false),
                        CodigoVariante = c.String(maxLength: 100),
                        NombreVariante = c.String(maxLength: 200),
                        CostoAdicional = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdVariante)
                .ForeignKey("dbo.Producto", t => t.IdProducto, cascadeDelete: true)
                .Index(t => t.IdProducto);
            
            CreateTable(
                "dbo.TipoMovimientoInventario",
                c => new
                    {
                        IdEstadoInventario = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.IdEstadoInventario);
            
            CreateTable(
                "dbo.Prestamo",
                c => new
                    {
                        IdPrestamo = c.Long(nullable: false, identity: true),
                        NumeroPrestamo = c.String(nullable: false, maxLength: 100),
                        Id = c.String(maxLength: 128),
                        IdEstado = c.Int(nullable: false),
                        FechadeCreacion = c.DateTime(nullable: false),
                        FechaDevolucion = c.DateTime(),
                        Notas = c.String(maxLength: 1000),
                        Devolucion = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdPrestamo)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .Index(t => t.Id)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.RecuperacionContrasena",
                c => new
                    {
                        IdRecuperacion = c.Int(nullable: false, identity: true),
                        Id = c.String(maxLength: 128),
                        Token = c.String(nullable: false, maxLength: 500),
                        Expira = c.DateTime(nullable: false),
                        Usado = c.Boolean(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdRecuperacion)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Reserva",
                c => new
                    {
                        IdReserva = c.Long(nullable: false, identity: true),
                        NumeroReserva = c.String(nullable: false, maxLength: 100),
                        Id = c.String(maxLength: 128),
                        TipoRecurso = c.String(nullable: false, maxLength: 50),
                        IdRecurso = c.Long(nullable: false),
                        IdVariante = c.Int(),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(nullable: false),
                        IdEstado = c.Int(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdReserva)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .Index(t => t.Id)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.ReservaEspacio",
                c => new
                    {
                        IdReservaEspacio = c.Long(nullable: false, identity: true),
                        IdEspacio = c.Int(nullable: false),
                        Id = c.String(maxLength: 128),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(nullable: false),
                        IdEstado = c.Int(nullable: false),
                        Notas = c.String(maxLength: 1000),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdReservaEspacio)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.Espacio", t => t.IdEspacio, cascadeDelete: true)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .Index(t => t.IdEspacio)
                .Index(t => t.Id)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Sancion",
                c => new
                    {
                        IdSancion = c.Long(nullable: false, identity: true),
                        Id = c.String(maxLength: 128),
                        IdPrestamo = c.Long(),
                        IdEstado = c.Int(nullable: false),
                        Motivo = c.String(maxLength: 1000),
                        Tipo = c.String(maxLength: 100),
                        Monto = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdSancion)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .ForeignKey("dbo.Prestamo", t => t.IdPrestamo)
                .Index(t => t.Id)
                .Index(t => t.IdPrestamo)
                .Index(t => t.IdEstado);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Descripcion = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Inventario",
                c => new
                    {
                        IdInventario = c.Int(nullable: false, identity: true),
                        IdEstado = c.Int(nullable: false),
                        IdProducto = c.Int(nullable: false),
                        IdVariante = c.Int(),
                        Cantidad = c.Int(nullable: false),
                        Minimo = c.Int(nullable: false),
                        Maximo = c.Int(nullable: false),
                        FechaActualizacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdInventario)
                .ForeignKey("dbo.Estado", t => t.IdEstado)
                .ForeignKey("dbo.Producto", t => t.IdProducto, cascadeDelete: true)
                .ForeignKey("dbo.VarianteProducto", t => t.IdVariante)
                .Index(t => t.IdEstado)
                .Index(t => t.IdProducto)
                .Index(t => t.IdVariante);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Inventario", "IdVariante", "dbo.VarianteProducto");
            DropForeignKey("dbo.Inventario", "IdProducto", "dbo.Producto");
            DropForeignKey("dbo.Inventario", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.Documento", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Sancion", "IdPrestamo", "dbo.Prestamo");
            DropForeignKey("dbo.Sancion", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.Sancion", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReservaEspacio", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.ReservaEspacio", "IdEspacio", "dbo.Espacio");
            DropForeignKey("dbo.ReservaEspacio", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Reserva", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.Reserva", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.RecuperacionContrasena", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Prestamo", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.Prestamo", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.MovimientoInventario", "IdVariante", "dbo.VarianteProducto");
            DropForeignKey("dbo.MovimientoInventario", "IdEstadoInventario", "dbo.TipoMovimientoInventario");
            DropForeignKey("dbo.MovimientoInventario", "IdSerie", "dbo.NumeroSerieProducto");
            DropForeignKey("dbo.NumeroSerieProducto", "IdVariante", "dbo.VarianteProducto");
            DropForeignKey("dbo.VarianteProducto", "IdProducto", "dbo.Producto");
            DropForeignKey("dbo.NumeroSerieProducto", "IdProducto", "dbo.Producto");
            DropForeignKey("dbo.NumeroSerieProducto", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.MovimientoInventario", "IdProducto", "dbo.Producto");
            DropForeignKey("dbo.Producto", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.Producto", "IdCategoria", "dbo.Categoria");
            DropForeignKey("dbo.MovimientoInventario", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.MantenimientoEspacio", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.MantenimientoEspacio", "IdEspacio", "dbo.Espacio");
            DropForeignKey("dbo.Espacio", "IdEstado", "dbo.Estado");
            DropForeignKey("dbo.MantenimientoEspacio", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Categoria", "IdEstado", "dbo.Estado");
            DropIndex("dbo.Inventario", new[] { "IdVariante" });
            DropIndex("dbo.Inventario", new[] { "IdProducto" });
            DropIndex("dbo.Inventario", new[] { "IdEstado" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Sancion", new[] { "IdEstado" });
            DropIndex("dbo.Sancion", new[] { "IdPrestamo" });
            DropIndex("dbo.Sancion", new[] { "Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.ReservaEspacio", new[] { "IdEstado" });
            DropIndex("dbo.ReservaEspacio", new[] { "Id" });
            DropIndex("dbo.ReservaEspacio", new[] { "IdEspacio" });
            DropIndex("dbo.Reserva", new[] { "IdEstado" });
            DropIndex("dbo.Reserva", new[] { "Id" });
            DropIndex("dbo.RecuperacionContrasena", new[] { "Id" });
            DropIndex("dbo.Prestamo", new[] { "IdEstado" });
            DropIndex("dbo.Prestamo", new[] { "Id" });
            DropIndex("dbo.VarianteProducto", new[] { "IdProducto" });
            DropIndex("dbo.NumeroSerieProducto", new[] { "IdEstado" });
            DropIndex("dbo.NumeroSerieProducto", new[] { "IdProducto" });
            DropIndex("dbo.NumeroSerieProducto", new[] { "IdVariante" });
            DropIndex("dbo.Producto", new[] { "IdCategoria" });
            DropIndex("dbo.Producto", new[] { "IdEstado" });
            DropIndex("dbo.MovimientoInventario", new[] { "Id" });
            DropIndex("dbo.MovimientoInventario", new[] { "IdEstadoInventario" });
            DropIndex("dbo.MovimientoInventario", new[] { "IdSerie" });
            DropIndex("dbo.MovimientoInventario", new[] { "IdVariante" });
            DropIndex("dbo.MovimientoInventario", new[] { "IdProducto" });
            DropIndex("dbo.Espacio", new[] { "IdEstado" });
            DropIndex("dbo.MantenimientoEspacio", new[] { "IdEstado" });
            DropIndex("dbo.MantenimientoEspacio", new[] { "Id" });
            DropIndex("dbo.MantenimientoEspacio", new[] { "IdEspacio" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Documento", new[] { "Id" });
            DropIndex("dbo.Categoria", new[] { "IdEstado" });
            DropTable("dbo.Inventario");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Sancion");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.ReservaEspacio");
            DropTable("dbo.Reserva");
            DropTable("dbo.RecuperacionContrasena");
            DropTable("dbo.Prestamo");
            DropTable("dbo.TipoMovimientoInventario");
            DropTable("dbo.VarianteProducto");
            DropTable("dbo.NumeroSerieProducto");
            DropTable("dbo.Producto");
            DropTable("dbo.MovimientoInventario");
            DropTable("dbo.Espacio");
            DropTable("dbo.MantenimientoEspacio");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Documento");
            DropTable("dbo.Estado");
            DropTable("dbo.Categoria");
        }
    }
}
