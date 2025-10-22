using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class PlantillaReporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPlantilla { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre de Plantilla")]
        public string Nombre { get; set; }

        [MaxLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tipo de Reporte")]
        public string TipoReporte { get; set; }

        [Display(Name = "Configuración (JSON)")]
        public string ConfiguracionJSON { get; set; }

        [Display(Name = "Es Editable")]
        public bool EsEditable { get; set; } = true;

        [Display(Name = "Orden de Visualización")]
        public int OrdenVisualizacion { get; set; } = 0;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}

