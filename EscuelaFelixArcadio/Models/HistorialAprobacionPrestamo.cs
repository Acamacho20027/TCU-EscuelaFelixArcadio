using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class HistorialAprobacionPrestamo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdHistorial { get; set; }

        [ForeignKey("Prestamo")]
        [Display(Name = "Préstamo")]
        public long IdPrestamo { get; set; }
        public virtual Prestamo Prestamo { get; set; }

        [ForeignKey("UsuarioSolicitante")]
        [Display(Name = "Usuario Solicitante")]
        public string IdUsuarioSolicitante { get; set; }
        public virtual ApplicationUser UsuarioSolicitante { get; set; }

        [ForeignKey("UsuarioRevisor")]
        [Display(Name = "Usuario Revisor")]
        public string IdUsuarioRevisor { get; set; }
        public virtual ApplicationUser UsuarioRevisor { get; set; }

        [MaxLength(50)]
        [Display(Name = "Estado Previo")]
        public string EstadoPrevio { get; set; }

        [MaxLength(50)]
        [Display(Name = "Estado Nuevo")]
        public string EstadoNuevo { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Acción")]
        public string Accion { get; set; } // Aprobado, Rechazado, Pendiente, EnRevision

        [MaxLength(500)]
        [Display(Name = "Motivo de Rechazo")]
        public string MotivoRechazo { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Comentarios del Revisor")]
        public string ComentariosRevisor { get; set; }

        [Display(Name = "Fecha de Revisión")]
        public DateTime FechaRevision { get; set; } = DateTime.Now;

        [Display(Name = "Duración de Revisión (minutos)")]
        public int? DuracionRevision { get; set; }

        [Display(Name = "Prioridad")]
        public int Prioridad { get; set; } = 0; // 0=Normal, 1=Alta, 2=Urgente

        [Display(Name = "Notificado al Solicitante")]
        public bool NotificadoSolicitante { get; set; } = false;
    }
}

