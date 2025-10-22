using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class ConfiguracionReporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdConfiguracion { get; set; }

        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre de Configuración")]
        public string NombreConfiguracion { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tipo de Reporte")]
        public string TipoReporte { get; set; }

        [Display(Name = "Filtros (JSON)")]
        public string FiltrosJSON { get; set; }

        [Display(Name = "Columnas Seleccionadas (JSON)")]
        public string ColumnasSeleccionadas { get; set; }

        [Display(Name = "Orden de Columnas (JSON)")]
        public string OrdenColumnas { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Es Pública")]
        public bool EsPublica { get; set; } = false;
    }
}

