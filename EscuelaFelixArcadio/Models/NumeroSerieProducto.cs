using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class NumeroSerieProducto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdSerie { get; set; }

        public int? IdVariante { get; set; }
        [ForeignKey("IdVariante")]
        public VarianteProducto Variante { get; set; }

        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto Producto { get; set; }

        [Required, MaxLength(200)]
        public string NumeroSerie { get; set; }

        public int IdEstado { get; set; }
        [ForeignKey("IdEstado")]
        public Estado Estado { get; set; }

        [MaxLength(200)]
        public string Ubicacion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}