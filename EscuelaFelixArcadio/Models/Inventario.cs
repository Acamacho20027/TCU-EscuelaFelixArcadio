using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class Inventario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdInventario { get; set; }

        public int IdEstado { get; set; }
        [ForeignKey("IdEstado")]
        public Estado Estado { get; set; }

        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto Producto { get; set; }

        public int? IdVariante { get; set; }
        [ForeignKey("IdVariante")]
        public VarianteProducto Variante { get; set; }

        public int Cantidad { get; set; } = 0;
        public int Minimo { get; set; } = 0;
        public int Maximo { get; set; } = 0;

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}