namespace EscuelaFelixArcadio.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AgregarCamposArchivoDocumento : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documento", "TipoDocumento", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Documento", "NombreArchivo", c => c.String(nullable: false, maxLength: 500));
            AddColumn("dbo.Documento", "RutaArchivo", c => c.String(nullable: false, maxLength: 1000));
            AddColumn("dbo.Documento", "TamanoArchivo", c => c.Long(nullable: false));
            AddColumn("dbo.Documento", "Activo", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documento", "Activo");
            DropColumn("dbo.Documento", "TamanoArchivo");
            DropColumn("dbo.Documento", "RutaArchivo");
            DropColumn("dbo.Documento", "NombreArchivo");
            DropColumn("dbo.Documento", "TipoDocumento");
        }
    }
}
