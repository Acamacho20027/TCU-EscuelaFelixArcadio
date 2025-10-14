using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProducto { get; set; }

        [Required, MaxLength(100)]
        public string Codigo { get; set; }

        [Required, MaxLength(250)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }
        [MaxLength(150)]
        public string Marca { get; set; }

        public bool EsServicio { get; set; } = false;
        public bool Eliminado { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Llaves foráneas
        public int IdEstado { get; set; }
        [ForeignKey("IdEstado")]
        public Estado Estado { get; set; }

        public int? IdCategoria { get; set; }
        [ForeignKey("IdCategoria")]
        public Categoria Categoria { get; set; }

    }
}