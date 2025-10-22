using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class ComentarioReporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdComentario { get; set; }

        [ForeignKey("ReporteGuardado")]
        [Display(Name = "Reporte Guardado")]
        public long IdReporteGuardado { get; set; }
        public virtual ReporteGuardado ReporteGuardado { get; set; }

        [ForeignKey("ApplicationUser")]
        [Display(Name = "Usuario")]
        public string IdUsuario { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        [Display(Name = "Comentario")]
        public string Comentario { get; set; }

        [Display(Name = "Es Anotación")]
        public bool EsAnotacion { get; set; } = false;

        [Display(Name = "Posición X")]
        public int? PosicionX { get; set; }

        [Display(Name = "Posición Y")]
        public int? PosicionY { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Comentario Padre")]
        public long? IdComentarioPadre { get; set; }

        [MaxLength(50)]
        [Display(Name = "Color de Anotación")]
        public string ColorAnotacion { get; set; }
    }
}

