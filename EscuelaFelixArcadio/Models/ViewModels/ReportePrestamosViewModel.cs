using System;

namespace EscuelaFelixArcadio.Models.ViewModels
{
    public class ReportePrestamosViewModel
    {
        public long IdPrestamo { get; set; }
        public string NumeroPrestamo { get; set; }
        public string Usuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Estado { get; set; }
        public DateTime FechadeCreacion { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool Devolucion { get; set; }
        public string MotivoPrestamo { get; set; }
        public bool EsUrgente { get; set; }
        public long DiasRetraso { get; set; }
    }
}

