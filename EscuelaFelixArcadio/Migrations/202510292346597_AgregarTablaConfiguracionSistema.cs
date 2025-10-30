namespace EscuelaFelixArcadio.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarTablaConfiguracionSistema : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConfiguracionSistema",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Valor = c.String(nullable: false, maxLength: 200),
                        Descripcion = c.String(maxLength: 500),
                        Categoria = c.String(maxLength: 50),
                        FechaActualizacion = c.DateTime(nullable: false),
                        UsuarioActualizacion = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ConfiguracionSistema");
        }
    }
}
