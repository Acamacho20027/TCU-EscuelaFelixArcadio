using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class RecuperacionContrasena
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRecuperacion { get; set; }

        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required, MaxLength(500)]
        public string Token { get; set; }

        [Required]
        public DateTime Expira { get; set; }

        public bool Usado { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}