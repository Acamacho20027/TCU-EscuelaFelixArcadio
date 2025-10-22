using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class PermisoReporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPermiso { get; set; }

        [ForeignKey("ApplicationUser")]
        [Display(Name = "Usuario")]
        public string IdUsuario { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [ForeignKey("ApplicationRol")]
        [Display(Name = "Rol")]
        public string IdRol { get; set; }
        public virtual ApplicationRol ApplicationRol { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Tipo de Reporte")]
        public string TipoReporte { get; set; }

        [Display(Name = "Puede Visualizar")]
        public bool PuedeVisualizar { get; set; } = false;

        [Display(Name = "Puede Descargar")]
        public bool PuedeDescargar { get; set; } = false;

        [Display(Name = "Puede Editar")]
        public bool PuedeEditar { get; set; } = false;

        [Display(Name = "Puede Compartir")]
        public bool PuedeCompartir { get; set; } = false;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}

