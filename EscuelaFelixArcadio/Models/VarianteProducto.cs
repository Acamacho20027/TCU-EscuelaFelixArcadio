using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class VarianteProducto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdVariante { get; set; }

        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto Producto { get; set; }

        [MaxLength(100)]
        public string CodigoVariante { get; set; }

        [MaxLength(200)]
        public string NombreVariante { get; set; }

        public decimal CostoAdicional { get; set; } = 0;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}