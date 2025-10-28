namespace EscuelaFelixArcadio.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarTablaMensajeSistema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MensajeSistema",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Titulo = c.String(),
                        Contenido = c.String(),
                        Tipo = c.String(),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(),
                        Activo = c.Boolean(nullable: false),
                        RolDestino = c.String(),
                        FechaCreacion = c.DateTime(nullable: false),
                        IdUsuarioCreacion = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MensajeSistema");
        }
    }
}
