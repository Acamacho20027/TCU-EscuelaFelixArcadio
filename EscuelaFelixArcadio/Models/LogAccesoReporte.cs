using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class LogAccesoReporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdLog { get; set; }

        [ForeignKey("ApplicationUser")]
        [Display(Name = "Usuario")]
        public string IdUsuario { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tipo de Reporte")]
        public string TipoReporte { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Acción")]
        public string Accion { get; set; } // Visualizar, Descargar, Exportar, etc.

        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; } = DateTime.Now;

        [MaxLength(50)]
        [Display(Name = "Dirección IP")]
        public string DireccionIP { get; set; }

        [MaxLength(500)]
        [Display(Name = "Detalles Adicionales")]
        public string DetallesAdicionales { get; set; }
    }
}

