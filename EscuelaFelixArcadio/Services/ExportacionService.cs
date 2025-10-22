using EscuelaFelixArcadio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Services
{
    public class ExportacionService
    {
        /// <summary>
        /// Genera un archivo PDF a partir de HTML
        /// Nota: Requiere instalar paquete NuGet para PDF (iTextSharp o DinkToPdf)
        /// Por ahora retorna HTML que puede ser convertido a PDF en el cliente
        /// </summary>
        public string GenerarPDF(string tipoReporte, object datos, string titulo)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>" + titulo + "</title>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                body { font-family: Arial, sans-serif; margin: 20px; }
                h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
                table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                th { background-color: #3498db; color: white; padding: 12px; text-align: left; }
                td { padding: 10px; border-bottom: 1px solid #ddd; }
                tr:hover { background-color: #f5f5f5; }
                .header { text-align: center; margin-bottom: 30px; }
                .fecha { text-align: right; color: #7f8c8d; margin-bottom: 20px; }
                .alert-critical { color: #e74c3c; font-weight: bold; }
                .alert-high { color: #e67e22; }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>Escuela Félix Arcadio Montero Monge</h1>");
            html.AppendLine("<h2>" + titulo + "</h2>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='fecha'>Fecha de generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "</div>");

            // Aquí se puede personalizar según el tipo de reporte
            html.AppendLine("<div id='contenido'>");
            html.AppendLine("<p>Datos del reporte: " + tipoReporte + "</p>");
            html.AppendLine("<!-- Los datos se insertarán dinámicamente -->");
            html.AppendLine("</div>");

            html.AppendLine("</body></html>");

            return html.ToString();
        }

        /// <summary>
        /// Genera un archivo Excel (CSV) a partir de datos
        /// Nota: Para Excel real (.xlsx) instalar EPPlus o ClosedXML
        /// Por ahora genera CSV que Excel puede abrir
        /// </summary>
        public string GenerarExcel(string tipoReporte, dynamic datos)
        {
            var csv = new StringBuilder();

            // Header del archivo
            csv.AppendLine("Escuela Félix Arcadio Montero Monge");
            csv.AppendLine("Reporte: " + tipoReporte);
            csv.AppendLine("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            csv.AppendLine();

            // Los encabezados y datos se agregarán dinámicamente según el tipo de reporte
            // Esto es un placeholder - se debe personalizar por tipo de reporte

            return csv.ToString();
        }

        /// <summary>
        /// Genera CSV específico para Inventario
        /// </summary>
        public byte[] GenerarCSVInventario(IEnumerable<dynamic> datos)
        {
            var csv = new StringBuilder();
            csv.AppendLine("IdInventario,Producto,Codigo,Categoria,Estado,Cantidad,Minimo,Maximo,Variante,FechaActualizacion,AlertaStockBajo");

            foreach (var item in datos)
            {
                csv.AppendLine($"{item.IdInventario},{EscapeCSV(item.Producto)},{EscapeCSV(item.Codigo)},{EscapeCSV(item.Categoria)},{EscapeCSV(item.Estado)},{item.Cantidad},{item.Minimo},{item.Maximo},{EscapeCSV(item.Variante)},{item.FechaActualizacion},{item.AlertaStockBajo}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        /// <summary>
        /// Genera CSV específico para Préstamos
        /// </summary>
        public byte[] GenerarCSVPrestamos(IEnumerable<dynamic> datos)
        {
            var csv = new StringBuilder();
            csv.AppendLine("IdPrestamo,NumeroPrestamo,Usuario,NombreCompleto,Estado,FechaCreacion,FechaDevolucion,FechaVencimiento,Devuelto,Motivo,Urgente,DiasRetraso");

            foreach (var item in datos)
            {
                csv.AppendLine($"{item.IdPrestamo},{EscapeCSV(item.NumeroPrestamo)},{EscapeCSV(item.Usuario)},{EscapeCSV(item.NombreCompleto)},{EscapeCSV(item.Estado)},{item.FechadeCreacion},{item.FechaDevolucion},{item.FechaVencimiento},{item.Devolucion},{EscapeCSV(item.MotivoPrestamo)},{item.EsUrgente},{item.DiasRetraso}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        /// <summary>
        /// Genera CSV específico para Sanciones
        /// </summary>
        public byte[] GenerarCSVSanciones(IEnumerable<dynamic> datos)
        {
            var csv = new StringBuilder();
            csv.AppendLine("IdSancion,Usuario,NombreCompleto,Estado,Tipo,Motivo,Monto,FechaInicio,FechaFin,PrestamoRelacionado,SancionActiva");

            foreach (var item in datos)
            {
                csv.AppendLine($"{item.IdSancion},{EscapeCSV(item.Usuario)},{EscapeCSV(item.NombreCompleto)},{EscapeCSV(item.Estado)},{EscapeCSV(item.Tipo)},{EscapeCSV(item.Motivo)},{item.Monto},{item.FechaInicio},{item.FechaFin},{EscapeCSV(item.PrestamoRelacionado)},{item.SancionActiva}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        /// <summary>
        /// Escapa caracteres especiales para CSV
        /// </summary>
        private string EscapeCSV(object value)
        {
            if (value == null) return "";
            
            string str = value.ToString();
            if (str.Contains(",") || str.Contains("\"") || str.Contains("\n"))
            {
                return "\"" + str.Replace("\"", "\"\"") + "\"";
            }
            return str;
        }

        /// <summary>
        /// Genera HTML para vista previa del reporte antes de exportar
        /// </summary>
        public string GenerarVistaPrevia(string tipoReporte, object datos)
        {
            // Similar a GenerarPDF pero optimizado para visualización web
            return GenerarPDF(tipoReporte, datos, "Vista Previa - " + tipoReporte);
        }
    }
}

