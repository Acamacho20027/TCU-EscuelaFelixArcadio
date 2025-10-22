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

        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(250, ErrorMessage = "El título no puede exceder 250 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [Display(Name = "Descripción")]
        [DataType(DataType.MultilineText)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [MaxLength(100)]
        [Display(Name = "Tipo de Documento")]
        public string TipoDocumento { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Nombre del Archivo")]
        public string NombreArchivo { get; set; }

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Ruta del Archivo")]
        public string RutaArchivo { get; set; }

        [Display(Name = "Tamaño del Archivo (bytes)")]
        public long TamanoArchivo { get; set; }

        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Display(Name = "Fecha de Subida")]
        [DataType(DataType.DateTime)]
        public DateTime FechaSubida { get; set; }

        [Display(Name = "Público")]
        public bool Publico { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        // Propiedad calculada para mostrar el tamaño formateado
        [NotMapped]
        [Display(Name = "Tamaño")]
        public string TamanoFormateado
        {
            get
            {
                if (TamanoArchivo < 1024)
                    return $"{TamanoArchivo} bytes";
                else if (TamanoArchivo < 1024 * 1024)
                    return $"{TamanoArchivo / 1024.0:F2} KB";
                else
                    return $"{TamanoArchivo / (1024.0 * 1024.0):F2} MB";
            }
        }
    }
}