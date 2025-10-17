using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class Prestamo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdPrestamo { get; set; }

        [Required, MaxLength(100)]
        public string NumeroPrestamo { get; set; }

        [ForeignKey("ApplicationUser")]
        public string Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int IdEstado { get; set; }
        [ForeignKey("IdEstado")]
        public virtual Estado Estado { get; set; }

        public DateTime FechadeCreacion { get; set; }
        public DateTime? FechaDevolucion { get; set; }

        [MaxLength(1000)]
        public string Notas { get; set; }

        /// <summary>
        /// 1 = Préstamo, 0 = Devolución
        /// </summary>
        public bool Devolucion { get; set; }

        // ===== CAMPOS PARA FUNCIONALIDADES AVANZADAS =====

        /// <summary>
        /// Fecha límite para devolver el préstamo
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>
        /// Duración estimada del préstamo en días
        /// </summary>
        public int? DuracionEstimada { get; set; }

        /// <summary>
        /// Número de notificaciones de retraso enviadas
        /// </summary>
        public int NotificacionesEnviadas { get; set; } = 0;

        /// <summary>
        /// Fecha de la última notificación enviada
        /// </summary>
        public DateTime? UltimaNotificacionEnviada { get; set; }

        /// <summary>
        /// Indica si el préstamo está marcado como urgente/prioritario
        /// </summary>
        public bool EsUrgente { get; set; } = false;

        /// <summary>
        /// Motivo del préstamo (clase, entrenamiento, evento, etc.)
        /// </summary>
        [MaxLength(200)]
        public string MotivoPrestamo { get; set; }

        /// <summary>
        /// Fecha programada para el inicio del uso del material
        /// </summary>
        public DateTime? FechaInicioUso { get; set; }

        /// <summary>
        /// Fecha programada para el fin del uso del material
        /// </summary>
        public DateTime? FechaFinUso { get; set; }

        /// <summary>
        /// Indica si el préstamo fue renovado
        /// </summary>
        public bool FueRenovado { get; set; } = false;

        /// <summary>
        /// ID del préstamo original si este es una renovación
        /// </summary>
        public long? IdPrestamoOriginal { get; set; }

        /// <summary>
        /// Calificación del estado del material al devolverlo (1-5)
        /// </summary>
        public int? CalificacionEstadoMaterial { get; set; }

        /// <summary>
        /// Observaciones sobre el estado del material al devolverlo
        /// </summary>
        [MaxLength(500)]
        public string ObservacionesDevolucion { get; set; }
    }
}