using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscuelaFelixArcadio.Models
{
    public class ConfiguracionSistema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Valor")]
        public string Valor { get; set; }

        [MaxLength(500)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [MaxLength(50)]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Fecha de Actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        [Display(Name = "Usuario Actualización")]
        public string UsuarioActualizacion { get; set; }
    }
}

