namespace EscuelaFelixArcadio.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarCamposAvanzadosPrestamo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Prestamo", "FechaVencimiento", c => c.DateTime());
            AddColumn("dbo.Prestamo", "DuracionEstimada", c => c.Int());
            AddColumn("dbo.Prestamo", "NotificacionesEnviadas", c => c.Int(nullable: false));
            AddColumn("dbo.Prestamo", "UltimaNotificacionEnviada", c => c.DateTime());
            AddColumn("dbo.Prestamo", "EsUrgente", c => c.Boolean(nullable: false));
            AddColumn("dbo.Prestamo", "MotivoPrestamo", c => c.String(maxLength: 200));
            AddColumn("dbo.Prestamo", "FechaInicioUso", c => c.DateTime());
            AddColumn("dbo.Prestamo", "FechaFinUso", c => c.DateTime());
            AddColumn("dbo.Prestamo", "FueRenovado", c => c.Boolean(nullable: false));
            AddColumn("dbo.Prestamo", "IdPrestamoOriginal", c => c.Long());
            AddColumn("dbo.Prestamo", "CalificacionEstadoMaterial", c => c.Int());
            AddColumn("dbo.Prestamo", "ObservacionesDevolucion", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Prestamo", "ObservacionesDevolucion");
            DropColumn("dbo.Prestamo", "CalificacionEstadoMaterial");
            DropColumn("dbo.Prestamo", "IdPrestamoOriginal");
            DropColumn("dbo.Prestamo", "FueRenovado");
            DropColumn("dbo.Prestamo", "FechaFinUso");
            DropColumn("dbo.Prestamo", "FechaInicioUso");
            DropColumn("dbo.Prestamo", "MotivoPrestamo");
            DropColumn("dbo.Prestamo", "EsUrgente");
            DropColumn("dbo.Prestamo", "UltimaNotificacionEnviada");
            DropColumn("dbo.Prestamo", "NotificacionesEnviadas");
            DropColumn("dbo.Prestamo", "DuracionEstimada");
            DropColumn("dbo.Prestamo", "FechaVencimiento");
        }
    }
}
