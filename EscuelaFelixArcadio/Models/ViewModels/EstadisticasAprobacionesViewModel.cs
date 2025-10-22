namespace EscuelaFelixArcadio.Models.ViewModels
{
    public class EstadisticasAprobacionesViewModel
    {
        public int Total { get; set; }
        public int Aprobados { get; set; }
        public int Rechazados { get; set; }
        public int Pendientes { get; set; }
        public double TasaAprobacion { get; set; }
        public double TasaRechazo { get; set; }
        public int? TiempoPromedioRevision { get; set; }
    }
}

