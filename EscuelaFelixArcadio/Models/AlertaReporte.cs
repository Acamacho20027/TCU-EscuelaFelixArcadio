using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class AlertaReporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAlerta { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Tipo de Alerta")]
        public string TipoAlerta { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Severidad")]
        public string Severidad { get; set; } // Baja, Media, Alta, Crítica

        [Display(Name = "Fecha de Generación")]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        [Display(Name = "ID Registro Relacionado")]
        public long? IdRegistroRelacionado { get; set; }

        [MaxLength(50)]
        [Display(Name = "Tipo de Registro Relacionado")]
        public string TipoRegistroRelacionado { get; set; } // Prestamo, Inventario, Sancion, etc.

        [Display(Name = "Leída")]
        public bool Leida { get; set; } = false;

        [Display(Name = "Fecha de Lectura")]
        public DateTime? FechaLectura { get; set; }

        [ForeignKey("ApplicationUser")]
        [Display(Name = "Leída Por")]
        public string LeidaPor { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Display(Name = "Requiere Acción")]
        public bool RequiereAccion { get; set; } = false;

        [Display(Name = "Acción Tomada")]
        public bool AccionTomada { get; set; } = false;
    }
}

