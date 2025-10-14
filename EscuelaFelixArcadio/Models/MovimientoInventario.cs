using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class MovimientoInventario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdMovimiento { get; set; }

        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto Producto { get; set; }

        public int? IdVariante { get; set; }
        [ForeignKey("IdVariante")]
        public VarianteProducto Variante { get; set; }

        public long? IdSerie { get; set; }
        [ForeignKey("IdSerie")]
        public NumeroSerieProducto Serie { get; set; }

        public int IdEstadoInventario { get; set; }
        [ForeignKey("IdEstadoInventario")]
        public TipoMovimientoInventario TipoMovimientoInventario { get; set; }

        public int Cantidad { get; set; }

        [Required, MaxLength(50)]
        public string TipoMovimiento { get; set; }

        [MaxLength(200)]
        public string Referencia { get; set; }

        [MaxLength(1000)]
        public string Notas { get; set; }

        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    }
}