namespace EscuelaFelixArcadio.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarSistemaReportesCompleto : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AlertaReporte",
                c => new
                    {
                        IdAlerta = c.Int(nullable: false, identity: true),
                        TipoAlerta = c.String(nullable: false, maxLength: 100),
                        Descripcion = c.String(nullable: false, maxLength: 500),
                        Severidad = c.String(nullable: false, maxLength: 20),
                        FechaGeneracion = c.DateTime(nullable: false),
                        IdRegistroRelacionado = c.Long(),
                        TipoRegistroRelacionado = c.String(maxLength: 50),
                        Leida = c.Boolean(nullable: false),
                        FechaLectura = c.DateTime(),
                        LeidaPor = c.String(maxLength: 128),
                        RequiereAccion = c.Boolean(nullable: false),
                        AccionTomada = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdAlerta)
                .ForeignKey("dbo.AspNetUsers", t => t.LeidaPor)
                .Index(t => t.LeidaPor);
            
            CreateTable(
                "dbo.ComentarioReporte",
                c => new
                    {
                        IdComentario = c.Long(nullable: false, identity: true),
                        IdReporteGuardado = c.Long(nullable: false),
                        IdUsuario = c.String(maxLength: 128),
                        Comentario = c.String(nullable: false),
                        EsAnotacion = c.Boolean(nullable: false),
                        PosicionX = c.Int(),
                        PosicionY = c.Int(),
                        FechaCreacion = c.DateTime(nullable: false),
                        IdComentarioPadre = c.Long(),
                        ColorAnotacion = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.IdComentario)
                .ForeignKey("dbo.AspNetUsers", t => t.IdUsuario)
                .ForeignKey("dbo.ReporteGuardado", t => t.IdReporteGuardado, cascadeDelete: true)
                .Index(t => t.IdReporteGuardado)
                .Index(t => t.IdUsuario);
            
            CreateTable(
                "dbo.ReporteGuardado",
                c => new
                    {
                        IdReporteGuardado = c.Long(nullable: false, identity: true),
                        TipoReporte = c.String(nullable: false, maxLength: 50),
                        NombreReporte = c.String(nullable: false, maxLength: 200),
                        RutaArchivo = c.String(maxLength: 500),
                        FechaGeneracion = c.DateTime(nullable: false),
                        GeneradoPor = c.String(maxLength: 128),
                        FiltrosUtilizados = c.String(),
                        TamanoArchivo = c.Long(),
                        Formato = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.IdReporteGuardado)
                .ForeignKey("dbo.AspNetUsers", t => t.GeneradoPor)
                .Index(t => t.GeneradoPor);
            
            CreateTable(
                "dbo.ConfiguracionReporte",
                c => new
                    {
                        IdConfiguracion = c.Int(nullable: false, identity: true),
                        Id = c.String(maxLength: 128),
                        NombreConfiguracion = c.String(nullable: false, maxLength: 200),
                        TipoReporte = c.String(nullable: false, maxLength: 50),
                        FiltrosJSON = c.String(),
                        ColumnasSeleccionadas = c.String(),
                        OrdenColumnas = c.String(),
                        FechaCreacion = c.DateTime(nullable: false),
                        EsPublica = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdConfiguracion)
                .ForeignKey("dbo.AspNetUsers", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.HistorialAprobacionPrestamo",
                c => new
                    {
                        IdHistorial = c.Long(nullable: false, identity: true),
                        IdPrestamo = c.Long(nullable: false),
                        IdUsuarioSolicitante = c.String(nullable: false, maxLength: 128),
                        IdUsuarioRevisor = c.String(nullable: false, maxLength: 128),
                        EstadoPrevio = c.String(maxLength: 50),
                        EstadoNuevo = c.String(maxLength: 50),
                        Accion = c.String(nullable: false, maxLength: 20),
                        MotivoRechazo = c.String(maxLength: 500),
                        ComentariosRevisor = c.String(maxLength: 1000),
                        FechaRevision = c.DateTime(nullable: false),
                        DuracionRevision = c.Int(),
                        Prioridad = c.Int(nullable: false),
                        NotificadoSolicitante = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdHistorial)
                .ForeignKey("dbo.Prestamo", t => t.IdPrestamo)
                .ForeignKey("dbo.AspNetUsers", t => t.IdUsuarioRevisor)
                .ForeignKey("dbo.AspNetUsers", t => t.IdUsuarioSolicitante)
                .Index(t => t.IdPrestamo)
                .Index(t => t.IdUsuarioSolicitante)
                .Index(t => t.IdUsuarioRevisor);
            
            CreateTable(
                "dbo.LogAccesoReporte",
                c => new
                    {
                        IdLog = c.Long(nullable: false, identity: true),
                        IdUsuario = c.String(maxLength: 128),
                        TipoReporte = c.String(nullable: false, maxLength: 50),
                        Accion = c.String(nullable: false, maxLength: 50),
                        FechaHora = c.DateTime(nullable: false),
                        DireccionIP = c.String(maxLength: 50),
                        DetallesAdicionales = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.IdLog)
                .ForeignKey("dbo.AspNetUsers", t => t.IdUsuario)
                .Index(t => t.IdUsuario);
            
            CreateTable(
                "dbo.PermisoReporte",
                c => new
                    {
                        IdPermiso = c.Int(nullable: false, identity: true),
                        IdUsuario = c.String(maxLength: 128),
                        IdRol = c.String(maxLength: 128),
                        TipoReporte = c.String(nullable: false, maxLength: 50),
                        PuedeVisualizar = c.Boolean(nullable: false),
                        PuedeDescargar = c.Boolean(nullable: false),
                        PuedeEditar = c.Boolean(nullable: false),
                        PuedeCompartir = c.Boolean(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdPermiso)
                .ForeignKey("dbo.AspNetRoles", t => t.IdRol)
                .ForeignKey("dbo.AspNetUsers", t => t.IdUsuario)
                .Index(t => t.IdUsuario)
                .Index(t => t.IdRol);
            
            CreateTable(
                "dbo.PlantillaReporte",
                c => new
                    {
                        IdPlantilla = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 200),
                        Descripcion = c.String(maxLength: 500),
                        TipoReporte = c.String(nullable: false, maxLength: 50),
                        ConfiguracionJSON = c.String(),
                        EsEditable = c.Boolean(nullable: false),
                        OrdenVisualizacion = c.Int(nullable: false),
                        Activa = c.Boolean(nullable: false),
                        FechaCreacion = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdPlantilla);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PermisoReporte", "IdUsuario", "dbo.AspNetUsers");
            DropForeignKey("dbo.PermisoReporte", "IdRol", "dbo.AspNetRoles");
            DropForeignKey("dbo.LogAccesoReporte", "IdUsuario", "dbo.AspNetUsers");
            DropForeignKey("dbo.HistorialAprobacionPrestamo", "IdUsuarioSolicitante", "dbo.AspNetUsers");
            DropForeignKey("dbo.HistorialAprobacionPrestamo", "IdUsuarioRevisor", "dbo.AspNetUsers");
            DropForeignKey("dbo.HistorialAprobacionPrestamo", "IdPrestamo", "dbo.Prestamo");
            DropForeignKey("dbo.ConfiguracionReporte", "Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ComentarioReporte", "IdReporteGuardado", "dbo.ReporteGuardado");
            DropForeignKey("dbo.ReporteGuardado", "GeneradoPor", "dbo.AspNetUsers");
            DropForeignKey("dbo.ComentarioReporte", "IdUsuario", "dbo.AspNetUsers");
            DropForeignKey("dbo.AlertaReporte", "LeidaPor", "dbo.AspNetUsers");
            DropIndex("dbo.PermisoReporte", new[] { "IdRol" });
            DropIndex("dbo.PermisoReporte", new[] { "IdUsuario" });
            DropIndex("dbo.LogAccesoReporte", new[] { "IdUsuario" });
            DropIndex("dbo.HistorialAprobacionPrestamo", new[] { "IdUsuarioRevisor" });
            DropIndex("dbo.HistorialAprobacionPrestamo", new[] { "IdUsuarioSolicitante" });
            DropIndex("dbo.HistorialAprobacionPrestamo", new[] { "IdPrestamo" });
            DropIndex("dbo.ConfiguracionReporte", new[] { "Id" });
            DropIndex("dbo.ReporteGuardado", new[] { "GeneradoPor" });
            DropIndex("dbo.ComentarioReporte", new[] { "IdUsuario" });
            DropIndex("dbo.ComentarioReporte", new[] { "IdReporteGuardado" });
            DropIndex("dbo.AlertaReporte", new[] { "LeidaPor" });
            DropTable("dbo.PlantillaReporte");
            DropTable("dbo.PermisoReporte");
            DropTable("dbo.LogAccesoReporte");
            DropTable("dbo.HistorialAprobacionPrestamo");
            DropTable("dbo.ConfiguracionReporte");
            DropTable("dbo.ReporteGuardado");
            DropTable("dbo.ComentarioReporte");
            DropTable("dbo.AlertaReporte");
        }
    }
}
