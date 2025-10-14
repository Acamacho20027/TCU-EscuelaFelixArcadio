using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class Prestamo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdPrestamo { get; set; }

        [Required, MaxLength(100)]
        public string NumeroPrestamo { get; set; }

        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int IdEstado { get; set; }
        [ForeignKey("IdEstado")]
        public virtual Estado Estado { get; set; }

        public DateTime FechadeCreacion { get; set; }
        public DateTime? FechaDevolucion { get; set; }

        [MaxLength(1000)]
        public string Notas { get; set; }

        /// <summary>
        /// 1 = Préstamo, 0 = Devolución
        /// </summary>
        public bool Devolucion { get; set; }
    }
}