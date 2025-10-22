using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class ReporteGuardado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdReporteGuardado { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tipo de Reporte")]
        public string TipoReporte { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Nombre del Reporte")]
        public string NombreReporte { get; set; }

        [MaxLength(500)]
        [Display(Name = "Ruta del Archivo")]
        public string RutaArchivo { get; set; }

        [Display(Name = "Fecha de Generación")]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        [ForeignKey("ApplicationUser")]
        [Display(Name = "Generado Por")]
        public string GeneradoPor { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Display(Name = "Filtros Utilizados (JSON)")]
        public string FiltrosUtilizados { get; set; }

        [Display(Name = "Tamaño del Archivo (bytes)")]
        public long? TamanoArchivo { get; set; }

        [MaxLength(10)]
        [Display(Name = "Formato")]
        public string Formato { get; set; } // PDF, Excel, etc.
    }
}

