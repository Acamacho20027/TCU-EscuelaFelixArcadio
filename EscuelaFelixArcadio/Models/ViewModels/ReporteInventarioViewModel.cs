using System;

namespace EscuelaFelixArcadio.Models.ViewModels
{
    public class ReporteInventarioViewModel
    {
        public long IdInventario { get; set; }
        public string Producto { get; set; }
        public string Codigo { get; set; }
        public string Categoria { get; set; }
        public string Estado { get; set; }
        public int Cantidad { get; set; }
        public int Minimo { get; set; }
        public int Maximo { get; set; }
        public string Variante { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public bool AlertaStockBajo { get; set; }
    }
}

