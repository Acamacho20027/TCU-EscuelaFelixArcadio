using System;

namespace EscuelaFelixArcadio.Models.ViewModels
{
    public class ReporteSancionesViewModel
    {
        public long IdSancion { get; set; }
        public string Usuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Estado { get; set; }
        public string Tipo { get; set; }
        public string Motivo { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string PrestamoRelacionado { get; set; }
        public bool SancionActiva { get; set; }
    }
}

