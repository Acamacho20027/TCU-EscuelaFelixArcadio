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
        /// Genera un archivo PDF a partir de HTML con datos reales
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
                body { 
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                    margin: 0; 
                    padding: 20px; 
                    background: #f8f9fa;
                    color: #333;
                }
                .header { 
                    text-align: center; 
                    margin-bottom: 30px; 
                    background: white;
                    padding: 30px;
                    border-radius: 10px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    position: relative;
                }
                .volver-btn {
                    position: absolute;
                    left: 30px;
                    top: 50%;
                    transform: translateY(-50%);
                    background: #6c757d;
                    color: white;
                    padding: 6px 12px;
                    text-decoration: none;
                    border-radius: 4px;
                    font-weight: 500;
                    font-size: 12px;
                }
                .logo {
                    width: 80px;
                    height: 80px;
                    background: linear-gradient(135deg, #0ea5e9, #3b82f6);
                    border-radius: 50%;
                    margin: 0 auto 20px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    color: white;
                    font-size: 24px;
                    font-weight: bold;
                }
                h1 { 
                    color: #1e293b; 
                    font-size: 28px;
                    margin: 0 0 10px 0;
                    font-weight: 700;
                }
                h2 { 
                    color: #0ea5e9; 
                    font-size: 20px;
                    margin: 0;
                    font-weight: 600;
                }
                .fecha { 
                    text-align: right; 
                    color: #64748b; 
                    margin-bottom: 20px; 
                    font-size: 14px;
                }
                .report-info {
                    background: white;
                    padding: 20px;
                    border-radius: 10px;
                    margin-bottom: 20px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                }
                table { 
                    width: 100%; 
                    border-collapse: collapse; 
                    margin-top: 20px;
                    background: white;
                    border-radius: 10px;
                    overflow: hidden;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                }
                th { 
                    background: linear-gradient(135deg, #0ea5e9, #3b82f6); 
                    color: white; 
                    padding: 15px 12px; 
                    text-align: left; 
                    font-weight: 600;
                    font-size: 12px;
                    text-transform: uppercase;
                    letter-spacing: 0.5px;
                }
                td { 
                    padding: 12px; 
                    border-bottom: 1px solid #f1f5f9; 
                    font-size: 13px;
                }
                tr:nth-child(even) { 
                    background-color: #f8fafc; 
                }
                tr:hover { 
                    background-color: #f1f5f9; 
                }
                .badge {
                    padding: 4px 8px;
                    border-radius: 12px;
                    font-size: 11px;
                    font-weight: 600;
                    text-transform: uppercase;
                }
                .badge-success {
                    background: #d1fae5;
                    color: #065f46;
                }
                .badge-danger {
                    background: #fee2e2;
                    color: #991b1b;
                }
                .badge-warning {
                    background: #fef3c7;
                    color: #92400e;
                }
                .footer {
                    text-align: center;
                    margin-top: 30px;
                    padding: 20px;
                    color: #64748b;
                    font-size: 12px;
                }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            html.AppendLine("<div class='header'>");
            
            // Solo mostrar botón "Volver" para historial de aprobaciones
            if (tipoReporte.ToLower() == "historialaprobaciones")
            {
                // Botón eliminado según solicitud del usuario
            }
            
            html.AppendLine("<div class='logo'><img src='data:image/jpeg;base64," + ConvertirImagenABase64() + "' alt='Logo Escuela' style='width: 80px; height: 80px; border-radius: 50%; object-fit: cover;'></div>");
            html.AppendLine("<h1>Escuela Félix Arcadio Montero Monge</h1>");
            html.AppendLine("<h2>" + titulo + "</h2>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='fecha'>Generado el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " por: Admin</div>");

            // Generar contenido según el tipo de reporte
            html.AppendLine("<div class='report-info'>");
            
            switch (tipoReporte.ToLower())
            {
                case "inventario":
                    GenerarTablaInventario(html, datos);
                    break;
                case "prestamos":
                    GenerarTablaPrestamos(html, datos);
                    break;
                case "sanciones":
                    GenerarTablaSanciones(html, datos);
                    break;
                case "historialaprobaciones":
                    GenerarTablaHistorialAprobaciones(html, datos);
                    break;
                default:
                    html.AppendLine("<p>No hay datos disponibles para este reporte.</p>");
                    break;
            }
            
            html.AppendLine("</div>");

            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Página 1 de 1</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private void GenerarTablaInventario(StringBuilder html, object datos)
        {
            var inventarioData = datos as List<EscuelaFelixArcadio.Models.ViewModels.ReporteInventarioViewModel>;
            if (inventarioData == null || !inventarioData.Any())
            {
                html.AppendLine("<p>No hay datos de inventario disponibles.</p>");
                return;
            }

            html.AppendLine("<table>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>ID</th>");
            html.AppendLine("<th>Producto</th>");
            html.AppendLine("<th>Código</th>");
            html.AppendLine("<th>Categoría</th>");
            html.AppendLine("<th>Estado</th>");
            html.AppendLine("<th>Cantidad</th>");
            html.AppendLine("<th>Mínimo</th>");
            html.AppendLine("<th>Máximo</th>");
            html.AppendLine("<th>Variante</th>");
            html.AppendLine("<th>Última Actualización</th>");
            html.AppendLine("<th>Alerta</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var item in inventarioData)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{item.IdInventario}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Producto)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Codigo)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Categoria)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Estado)}</td>");
                html.AppendLine($"<td>{item.Cantidad}</td>");
                html.AppendLine($"<td>{item.Minimo}</td>");
                html.AppendLine($"<td>{item.Maximo}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Variante)}</td>");
                html.AppendLine($"<td>{item.FechaActualizacion:dd/MM/yyyy HH:mm:ss}</td>");
                
                string badgeClass = item.AlertaStockBajo ? "badge-danger" : "badge-success";
                string badgeText = item.AlertaStockBajo ? "Stock Bajo" : "Normal";
                html.AppendLine($"<td><span class='badge {badgeClass}'>{badgeText}</span></td>");
                
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
        }

        private void GenerarTablaPrestamos(StringBuilder html, object datos)
        {
            var prestamosData = datos as List<EscuelaFelixArcadio.Models.ViewModels.ReportePrestamosViewModel>;
            if (prestamosData == null || !prestamosData.Any())
            {
                html.AppendLine("<p>No hay datos de préstamos disponibles.</p>");
                return;
            }

            html.AppendLine("<table>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>ID</th>");
            html.AppendLine("<th>Número</th>");
            html.AppendLine("<th>Usuario</th>");
            html.AppendLine("<th>Estado</th>");
            html.AppendLine("<th>Fecha Creación</th>");
            html.AppendLine("<th>Fecha Devolución</th>");
            html.AppendLine("<th>Fecha Vencimiento</th>");
            html.AppendLine("<th>Devuelto</th>");
            html.AppendLine("<th>Motivo</th>");
            html.AppendLine("<th>Días Retraso</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var item in prestamosData)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{item.IdPrestamo}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.NumeroPrestamo)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Usuario)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Estado)}</td>");
                html.AppendLine($"<td>{item.FechadeCreacion:dd/MM/yyyy}</td>");
                html.AppendLine($"<td>{item.FechaDevolucion:dd/MM/yyyy}</td>");
                html.AppendLine($"<td>{item.FechaVencimiento:dd/MM/yyyy}</td>");
                html.AppendLine($"<td>{(item.Devolucion ? "Sí" : "No")}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.MotivoPrestamo)}</td>");
                html.AppendLine($"<td>{item.DiasRetraso}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
        }

        private void GenerarTablaSanciones(StringBuilder html, object datos)
        {
            var sancionesData = datos as List<EscuelaFelixArcadio.Models.ViewModels.ReporteSancionesViewModel>;
            if (sancionesData == null || !sancionesData.Any())
            {
                html.AppendLine("<p>No hay datos de sanciones disponibles.</p>");
                return;
            }

            html.AppendLine("<table>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>ID</th>");
            html.AppendLine("<th>Usuario</th>");
            html.AppendLine("<th>Estado</th>");
            html.AppendLine("<th>Tipo</th>");
            html.AppendLine("<th>Motivo</th>");
            html.AppendLine("<th>Monto</th>");
            html.AppendLine("<th>Fecha Inicio</th>");
            html.AppendLine("<th>Fecha Fin</th>");
            html.AppendLine("<th>Préstamo Relacionado</th>");
            html.AppendLine("<th>Activa</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var item in sancionesData)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{item.IdSancion}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Usuario)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Estado)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Tipo)}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.Motivo)}</td>");
                html.AppendLine($"<td>₡{item.Monto:N2}</td>");
                html.AppendLine($"<td>{item.FechaInicio:dd/MM/yyyy}</td>");
                html.AppendLine($"<td>{item.FechaFin:dd/MM/yyyy}</td>");
                html.AppendLine($"<td>{EscapeHtml(item.PrestamoRelacionado)}</td>");
                html.AppendLine($"<td>{(item.SancionActiva ? "Sí" : "No")}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
        }

        private void GenerarTablaHistorialAprobaciones(StringBuilder html, object datos)
        {
            var historialData = datos as List<object>;
            if (historialData == null || !historialData.Any())
            {
                html.AppendLine("<div style='text-align: center; margin: 40px 0;'>");
                html.AppendLine("<p style='font-size: 18px; color: #64748b; margin-bottom: 20px;'>No hay datos de historial de aprobaciones disponibles.</p>");
                html.AppendLine("</div>");
                return;
            }

            html.AppendLine("<table>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>ID Historial</th>");
            html.AppendLine("<th>Número Préstamo</th>");
            html.AppendLine("<th>Solicitante</th>");
            html.AppendLine("<th>Revisor</th>");
            html.AppendLine("<th>Estado Previo</th>");
            html.AppendLine("<th>Estado Nuevo</th>");
            html.AppendLine("<th>Acción</th>");
            html.AppendLine("<th>Motivo Rechazo</th>");
            html.AppendLine("<th>Comentarios</th>");
            html.AppendLine("<th>Fecha Revisión</th>");
            html.AppendLine("<th>Duración</th>");
            html.AppendLine("<th>Prioridad</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var item in historialData)
            {
                var idHistorial = item.GetType().GetProperty("IdHistorial")?.GetValue(item);
                var numeroPrestamo = item.GetType().GetProperty("NumeroPrestamo")?.GetValue(item);
                var solicitante = item.GetType().GetProperty("Solicitante")?.GetValue(item);
                var revisor = item.GetType().GetProperty("Revisor")?.GetValue(item);
                var estadoPrevio = item.GetType().GetProperty("EstadoPrevio")?.GetValue(item);
                var estadoNuevo = item.GetType().GetProperty("EstadoNuevo")?.GetValue(item);
                var accion = item.GetType().GetProperty("Accion")?.GetValue(item);
                var motivoRechazo = item.GetType().GetProperty("MotivoRechazo")?.GetValue(item);
                var comentariosRevisor = item.GetType().GetProperty("ComentariosRevisor")?.GetValue(item);
                var fechaRevision = item.GetType().GetProperty("FechaRevision")?.GetValue(item);
                var duracionRevision = item.GetType().GetProperty("DuracionRevision")?.GetValue(item);
                var prioridad = item.GetType().GetProperty("Prioridad")?.GetValue(item);

                html.AppendLine("<tr>");
                html.AppendLine($"<td>{idHistorial}</td>");
                html.AppendLine($"<td>{EscapeHtml(numeroPrestamo)}</td>");
                html.AppendLine($"<td>{EscapeHtml(solicitante)}</td>");
                html.AppendLine($"<td>{EscapeHtml(revisor)}</td>");
                html.AppendLine($"<td>{EscapeHtml(estadoPrevio)}</td>");
                html.AppendLine($"<td>{EscapeHtml(estadoNuevo)}</td>");
                html.AppendLine($"<td>{EscapeHtml(accion)}</td>");
                html.AppendLine($"<td>{EscapeHtml(motivoRechazo)}</td>");
                html.AppendLine($"<td>{EscapeHtml(comentariosRevisor)}</td>");
                html.AppendLine($"<td>{fechaRevision}</td>");
                html.AppendLine($"<td>{duracionRevision}</td>");
                html.AppendLine($"<td>{EscapeHtml(prioridad)}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
        }

        private string EscapeHtml(object value)
        {
            if (value == null) return "";
            return value.ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private string ConvertirImagenABase64()
        {
            try
            {
                string rutaImagen = HttpContext.Current.Server.MapPath("~/Content/images/escudo_escuela_felix_arcadio.jpg");
                if (System.IO.File.Exists(rutaImagen))
                {
                    byte[] imagenBytes = System.IO.File.ReadAllBytes(rutaImagen);
                    return Convert.ToBase64String(imagenBytes);
                }
            }
            catch
            {
                // Si hay error, usar texto como fallback
            }
            return "";
        }

        /// <summary>
        /// Genera un archivo Excel bonito con formato usando HTML que Excel puede abrir
        /// </summary>
        public byte[] GenerarExcelBonito(string tipoReporte, object datos)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>" + tipoReporte.ToUpper() + "</title>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                body { 
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
                    margin: 20px; 
                    background: white;
                    color: #333;
                }
                .header { 
                    text-align: center; 
                    margin-bottom: 30px; 
                    background: linear-gradient(135deg, #0ea5e9, #3b82f6);
                    color: white;
                    padding: 30px;
                    border-radius: 10px;
                }
                h1 { 
                    font-size: 24px;
                    margin: 0 0 10px 0;
                    font-weight: 700;
                }
                h2 { 
                    font-size: 18px;
                    margin: 0;
                    font-weight: 600;
                    opacity: 0.9;
                }
                .info { 
                    text-align: center; 
                    color: #64748b; 
                    margin-bottom: 20px; 
                    font-size: 14px;
                }
                table { 
                    width: 100%; 
                    border-collapse: collapse; 
                    margin-top: 20px;
                    background: white;
                    border-radius: 10px;
                    overflow: hidden;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                }
                th { 
                    background: linear-gradient(135deg, #0ea5e9, #3b82f6); 
                    color: white; 
                    padding: 15px 12px; 
                    text-align: left; 
                    font-weight: 600;
                    font-size: 12px;
                    text-transform: uppercase;
                    letter-spacing: 0.5px;
                }
                td { 
                    padding: 12px; 
                    border-bottom: 1px solid #f1f5f9; 
                    font-size: 13px;
                }
                tr:nth-child(even) { 
                    background-color: #f8fafc; 
                }
                tr:hover { 
                    background-color: #f1f5f9; 
                }
                .badge {
                    padding: 4px 8px;
                    border-radius: 12px;
                    font-size: 11px;
                    font-weight: 600;
                    text-transform: uppercase;
                }
                .badge-success {
                    background: #d1fae5;
                    color: #065f46;
                }
                .badge-danger {
                    background: #fee2e2;
                    color: #991b1b;
                }
                .footer {
                    text-align: center;
                    margin-top: 30px;
                    padding: 20px;
                    color: #64748b;
                    font-size: 12px;
                    background: #f8f9fa;
                    border-radius: 10px;
                }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>ESCUELA FÉLIX ARCADIO MONTERO MONGE</h1>");
            html.AppendLine("<h2>REPORTE: " + tipoReporte.ToUpper() + "</h2>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='info'>");
            html.AppendLine("FECHA DE GENERACIÓN: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "<br>");
            html.AppendLine("GENERADO POR: Admin");
            html.AppendLine("</div>");

            // Generar contenido según el tipo de reporte
            switch (tipoReporte.ToLower())
            {
                case "inventario":
                    GenerarTablaInventario(html, datos);
                    break;
                case "prestamos":
                    GenerarTablaPrestamos(html, datos);
                    break;
                case "sanciones":
                    GenerarTablaSanciones(html, datos);
                    break;
                case "historialaprobaciones":
                    GenerarTablaHistorialAprobaciones(html, datos);
                    break;
                default:
                    html.AppendLine("<p>No hay datos disponibles para este reporte.</p>");
                    break;
            }

            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Fin del reporte</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body></html>");

            return Encoding.UTF8.GetBytes(html.ToString());
        }

        private void GenerarExcelInventario(StringBuilder csv, object datos)
        {
            var inventarioData = datos as List<EscuelaFelixArcadio.Models.ViewModels.ReporteInventarioViewModel>;
            if (inventarioData == null || !inventarioData.Any())
            {
                csv.AppendLine("No hay datos de inventario disponibles.");
                return;
            }

            csv.AppendLine("DATOS DE INVENTARIO");
            csv.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            csv.AppendLine();

            // Encabezados
            csv.AppendLine("ID,PRODUCTO,CÓDIGO,CATEGORÍA,ESTADO,CANTIDAD,MÍNIMO,MÁXIMO,VARIANTE,ÚLTIMA ACTUALIZACIÓN,ALERTA");

            // Datos
            foreach (var item in inventarioData)
            {
                string alerta = item.AlertaStockBajo ? "STOCK BAJO" : "NORMAL";
                csv.AppendLine($"{item.IdInventario},{EscapeCSV(item.Producto)},{EscapeCSV(item.Codigo)},{EscapeCSV(item.Categoria)},{EscapeCSV(item.Estado)},{item.Cantidad},{item.Minimo},{item.Maximo},{EscapeCSV(item.Variante)},{item.FechaActualizacion:dd/MM/yyyy HH:mm:ss},{alerta}");
            }

            csv.AppendLine();
            csv.AppendLine($"TOTAL DE REGISTROS: {inventarioData.Count}");
        }

        private void GenerarExcelPrestamos(StringBuilder csv, object datos)
        {
            var prestamosData = datos as List<EscuelaFelixArcadio.Models.ViewModels.ReportePrestamosViewModel>;
            if (prestamosData == null || !prestamosData.Any())
            {
                csv.AppendLine("No hay datos de préstamos disponibles.");
                return;
            }

            csv.AppendLine("DATOS DE PRÉSTAMOS");
            csv.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            csv.AppendLine();

            // Encabezados
            csv.AppendLine("ID,NÚMERO,USUARIO,ESTADO,FECHA CREACIÓN,FECHA DEVOLUCIÓN,FECHA VENCIMIENTO,DEVUELTO,MOTIVO,DÍAS RETRASO");

            // Datos
            foreach (var item in prestamosData)
            {
                csv.AppendLine($"{item.IdPrestamo},{EscapeCSV(item.NumeroPrestamo)},{EscapeCSV(item.Usuario)},{EscapeCSV(item.Estado)},{item.FechadeCreacion:dd/MM/yyyy},{item.FechaDevolucion:dd/MM/yyyy},{item.FechaVencimiento:dd/MM/yyyy},{(item.Devolucion ? "SÍ" : "NO")},{EscapeCSV(item.MotivoPrestamo)},{item.DiasRetraso}");
            }

            csv.AppendLine();
            csv.AppendLine($"TOTAL DE REGISTROS: {prestamosData.Count}");
        }

        private void GenerarExcelSanciones(StringBuilder csv, object datos)
        {
            var sancionesData = datos as List<EscuelaFelixArcadio.Models.ViewModels.ReporteSancionesViewModel>;
            if (sancionesData == null || !sancionesData.Any())
            {
                csv.AppendLine("No hay datos de sanciones disponibles.");
                return;
            }

            csv.AppendLine("DATOS DE SANCIONES");
            csv.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            csv.AppendLine();

            // Encabezados
            csv.AppendLine("ID,USUARIO,ESTADO,TIPO,MOTIVO,MONTO,FECHA INICIO,FECHA FIN,PRÉSTAMO RELACIONADO,ACTIVA");

            // Datos
            foreach (var item in sancionesData)
            {
                csv.AppendLine($"{item.IdSancion},{EscapeCSV(item.Usuario)},{EscapeCSV(item.Estado)},{EscapeCSV(item.Tipo)},{EscapeCSV(item.Motivo)},₡{item.Monto:N2},{item.FechaInicio:dd/MM/yyyy},{item.FechaFin:dd/MM/yyyy},{EscapeCSV(item.PrestamoRelacionado)},{(item.SancionActiva ? "SÍ" : "NO")}");
            }

            csv.AppendLine();
            csv.AppendLine($"TOTAL DE REGISTROS: {sancionesData.Count}");
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

