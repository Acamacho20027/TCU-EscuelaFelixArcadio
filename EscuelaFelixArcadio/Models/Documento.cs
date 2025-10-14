using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class Documento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdDocumento { get; set; }

        [Required, MaxLength(250)]
        public string Titulo { get; set; }

        [MaxLength(1000)]
        public string Descripcion { get; set; }

        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime FechaSubida { get; set; }

        public bool Publico { get; set; }
    }
}