using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class Espacio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEspacio { get; set; }

        [MaxLength(100)]
        public string Codigo { get; set; }

        [Required, MaxLength(250)]
        public string Nombre { get; set; }

        [MaxLength(1000)]
        public string Descripcion { get; set; }

        public int? Capacidad { get; set; }

        [MaxLength(250)]
        public string Ubicacion { get; set; }

        public int IdEstado { get; set; }
        [ForeignKey("IdEstado")]
        public virtual Estado Estado { get; set; }

        public DateTime FechaCreacion { get; set; }

    }
}