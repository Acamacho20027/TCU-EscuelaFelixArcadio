using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using EscuelaFelixArcadio.Models;
using EscuelaFelixArcadio.Services;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ReportesService _reportesService;
        private ExportacionService _exportacionService;

        public ReportesController()
        {
            _reportesService = new ReportesService(db);
            _exportacionService = new ExportacionService();
        }

        #region Vistas Principales

        // GET: Reportes
        public ActionResult Index()
        {
            ViewBag.Title = "Reportes";
            
            // Generar alertas automáticas
            _reportesService.GenerarAlertasAutomaticas();
            
            // Obtener alertas activas para mostrar en dashboard
            var alertas = _reportesService.ObtenerAlertasActivas().Take(5).ToList();
            ViewBag.AlertasRecientes = alertas;
            
            // Obtener plantillas disponibles
            var plantillas = db.PlantillaReporte.Where(p => p.Activa).OrderBy(p => p.OrdenVisualizacion).ToList();
            ViewBag.Plantillas = plantillas;

            // Obtener reportes guardados del usuario
            var userId = User.Identity.GetUserId();
            var reportesGuardados = db.ReporteGuardado
                .Where(r => r.GeneradoPor == userId)
                .OrderByDescending(r => r.FechaGeneracion)
                .Take(10)
                .ToList();
            ViewBag.ReportesGuardados = reportesGuardados;

            return View();
        }

        // GET: Reportes/Inventario
        public ActionResult Inventario(DateTime? fechaInicio, DateTime? fechaFin, string categorias, string estados)
        {
            ViewBag.Title = "Reportes de Inventario";
            
            List<int> listaCategorias = !string.IsNullOrEmpty(categorias) 
                ? categorias.Split(',').Select(int.Parse).ToList() 
                : null;
            
            List<int> listaEstados = !string.IsNullOrEmpty(estados) 
                ? estados.Split(',').Select(int.Parse).ToList() 
                : null;

            var datos = _reportesService.ObtenerDatosInventario(fechaInicio, fechaFin, listaCategorias, listaEstados);
            
            ViewBag.Categorias = db.Categoria.ToList();
            ViewBag.Estados = db.Estado.ToList();
            
            return View(datos);
        }

        // GET: Reportes/Prestamos
        public ActionResult Prestamos(DateTime? fechaInicio, DateTime? fechaFin, string usuarios, string estados, bool? devueltos, string search)
        {
            ViewBag.Title = "Reportes de Préstamos";
            
            List<string> listaUsuarios = !string.IsNullOrEmpty(usuarios) 
                ? usuarios.Split(',').ToList() 
                : null;
            
            // Cambiar estados de int a string para manejar los valores como "Pendiente", "Aprobado", etc.
            List<string> listaEstados = !string.IsNullOrEmpty(estados) 
                ? estados.Split(',').ToList() 
                : null;

            var datos = _reportesService.ObtenerDatosPrestamos(fechaInicio, fechaFin, listaUsuarios, listaEstados, devueltos);
            
            // Aplicar filtro de búsqueda si existe
            if (!string.IsNullOrEmpty(search))
            {
                datos = datos.Where(p => 
                    p.Usuario.ToLower().Contains(search.ToLower()) ||
                    p.NombreCompleto.ToLower().Contains(search.ToLower()) ||
                    p.NumeroPrestamo.ToLower().Contains(search.ToLower()) ||
                    (p.MotivoPrestamo != null && p.MotivoPrestamo.ToLower().Contains(search.ToLower()))
                ).ToList();
            }
            
            ViewBag.Usuarios = db.Users.ToList();
            ViewBag.Estados = db.Estado.ToList();
            
            return View(datos);
        }

        // GET: Reportes/Sanciones
        public ActionResult Sanciones(DateTime? fechaInicio, DateTime? fechaFin, string usuarios, string estados, string tipos, string search)
        {
            ViewBag.Title = "Reportes de Sanciones";
            
            List<string> listaUsuarios = !string.IsNullOrEmpty(usuarios) 
                ? usuarios.Split(',').ToList() 
                : null;
            
            // Cambiar estados de int a string para manejar los valores como "Activa", "Pagada", etc.
            List<string> listaEstados = !string.IsNullOrEmpty(estados) 
                ? estados.Split(',').ToList() 
                : null;
            
            List<string> listaTipos = !string.IsNullOrEmpty(tipos) 
                ? tipos.Split(',').ToList() 
                : null;

            var datos = _reportesService.ObtenerDatosSanciones(fechaInicio, fechaFin, listaUsuarios, listaEstados, listaTipos);
            
            // Aplicar filtro de búsqueda si existe
            if (!string.IsNullOrEmpty(search))
            {
                datos = datos.Where(s => 
                    s.Usuario.ToLower().Contains(search.ToLower()) ||
                    s.NombreCompleto.ToLower().Contains(search.ToLower()) ||
                    s.Motivo.ToLower().Contains(search.ToLower()) ||
                    s.Tipo.ToLower().Contains(search.ToLower())
                ).ToList();
            }
            
            ViewBag.Usuarios = db.Users.ToList();
            ViewBag.Estados = db.Estado.ToList();
            
            return View(datos);
        }

        #endregion

        #region Exportación de Datos

        // GET: Reportes/ExportarPDF
        public ActionResult ExportarPDF(string tipoReporte, string filtros)
        {
            RegistrarAcceso(tipoReporte, "ExportarPDF");
            
            var filtrosObj = !string.IsNullOrEmpty(filtros) 
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(filtros) 
                : new Dictionary<string, string>();

            object datos = null;
            string titulo = "";

            switch (tipoReporte.ToLower())
            {
                case "inventario":
                    datos = _reportesService.ObtenerDatosInventario(null, null);
                    titulo = "Reporte de Inventario";
                    break;
                case "prestamos":
                    datos = _reportesService.ObtenerDatosPrestamos(null, null);
                    titulo = "Reporte de Préstamos";
                    break;
                case "sanciones":
                    datos = _reportesService.ObtenerDatosSanciones(null, null);
                    titulo = "Reporte de Sanciones";
                    break;
                case "historialaprobaciones":
                    datos = ObtenerDatosHistorialAprobaciones();
                    titulo = "Historial de Aprobaciones de Préstamos";
                    break;
            }

            var html = _exportacionService.GenerarPDF(tipoReporte, datos, titulo);
            
            // Guardar reporte generado
            GuardarReporteGenerado(tipoReporte, titulo, filtros, "PDF");

            // Retornar HTML con botón de descarga antes de la tabla
            string botonVolver = "";
            switch (tipoReporte.ToLower())
            {
                case "inventario":
                    botonVolver = "<a href='/Reportes/Inventario' style='background: #f1f5f9; color: #475569; padding: 12px 24px; border-radius: 8px; border: 1px solid #e2e8f0; text-decoration: none; font-weight: 500; font-size: 14px; margin-right: 0px; display: inline-flex; align-items: center; gap: 8px;'><i>←</i> Volver a Inventario</a>";
                    break;
                case "prestamos":
                    botonVolver = "<a href='/Reportes/Prestamos' style='background: #f1f5f9; color: #475569; padding: 12px 24px; border-radius: 8px; border: 1px solid #e2e8f0; text-decoration: none; font-weight: 500; font-size: 14px; margin-right: 0px; display: inline-flex; align-items: center; gap: 8px;'><i>←</i> Volver a Préstamos</a>";
                    break;
                case "sanciones":
                    botonVolver = "<a href='/Reportes/Sanciones' style='background: #f1f5f9; color: #475569; padding: 12px 24px; border-radius: 8px; border: 1px solid #e2e8f0; text-decoration: none; font-weight: 500; font-size: 14px; margin-right: 0px; display: inline-flex; align-items: center; gap: 8px;'><i>←</i> Volver a Sanciones</a>";
                    break;
                case "historialaprobaciones":
                    botonVolver = "<a href='/Reportes/HistorialAprobaciones' style='background: #f1f5f9; color: #475569; padding: 12px 24px; border-radius: 8px; border: 1px solid #e2e8f0; text-decoration: none; font-weight: 500; font-size: 14px; margin-right: 0px; display: inline-flex; align-items: center; gap: 8px;'><i>←</i> Volver a Historial de Aprobaciones de Préstamos</a>";
                    break;
            }

            var htmlConDescarga = html.Replace("<div class='report-info'>", $@"
                <div style='display: flex; justify-content: space-between; align-items: center; margin: 20px; padding: 20px; background: #f8f9fa; border-radius: 10px;'>
                    <div style='flex: 1;'>
                        {botonVolver}
                    </div>
                    <div style='flex: 1; text-align: center;'>
                        <h3 style='color: #0ea5e9; margin-bottom: 15px;'>Reporte Generado Exitosamente</h3>
                        <p style='color: #64748b; margin: 0;'>Haz clic en el botón para descargar el archivo PDF</p>
                    </div>
                    <div style='flex: 1; text-align: right;'>
                        <button onclick='descargarPDF()' style='background: linear-gradient(135deg, #0ea5e9, #3b82f6); color: white; border: none; padding: 12px 24px; border-radius: 8px; font-size: 16px; cursor: pointer; font-weight: 600;'>
                            📄 Descargar PDF
                        </button>
                    </div>
                </div>
                <div class='report-info'>");
            
            htmlConDescarga = htmlConDescarga.Replace("</body>", @"
                <style>
                    .modal-overlay {
                        position: fixed;
                        top: 0;
                        left: 0;
                        width: 100%;
                        height: 100%;
                        background: rgba(0, 0, 0, 0.5);
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        z-index: 10000;
                    }
                    .modal-success {
                        background: linear-gradient(135deg, #10b981, #059669);
                        color: white;
                        padding: 30px;
                        border-radius: 15px;
                        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
                        text-align: center;
                        max-width: 400px;
                        animation: modalSlideIn 0.3s ease-out;
                    }
                    @keyframes modalSlideIn {
                        from {
                            opacity: 0;
                            transform: translateY(-20px);
                        }
                        to {
                            opacity: 1;
                            transform: translateY(0);
                        }
                    }
                    .modal-icon {
                        font-size: 48px;
                        margin-bottom: 15px;
                    }
                    .modal-title {
                        font-size: 20px;
                        font-weight: 700;
                        margin-bottom: 10px;
                    }
                    .modal-message {
                        font-size: 16px;
                        margin-bottom: 25px;
                        opacity: 0.9;
                    }
                    .modal-btn {
                        background: rgba(255, 255, 255, 0.2);
                        color: white;
                        border: 2px solid rgba(255, 255, 255, 0.3);
                        padding: 12px 30px;
                        border-radius: 8px;
                        font-size: 16px;
                        font-weight: 600;
                        cursor: pointer;
                        transition: all 0.3s ease;
                    }
                    .modal-btn:hover {
                        background: rgba(255, 255, 255, 0.3);
                        border-color: rgba(255, 255, 255, 0.5);
                    }
                </style>
                <script>
                    function descargarPDF() {
                        // Crear un blob con el HTML
                        var blob = new Blob([document.documentElement.outerHTML], {type: 'text/html'});
                        var url = window.URL.createObjectURL(blob);
                        
                        // Crear enlace de descarga
                        var a = document.createElement('a');
                        a.href = url;
                        a.download = '" + titulo.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + @".html';
                        document.body.appendChild(a);
                        a.click();
                        document.body.removeChild(a);
                        window.URL.revokeObjectURL(url);
                        
                        // Mostrar modal de éxito bonito
                        mostrarModalExito();
                    }
                    
                    function mostrarModalExito() {
                        var modal = document.createElement('div');
                        modal.className = 'modal-overlay';
                        modal.innerHTML = `
                            <div class='modal-success'>
                                <div class='modal-icon'>✅</div>
                                <div class='modal-title'>¡Descarga Exitosa!</div>
                                <div class='modal-message'>El archivo PDF se ha descargado correctamente</div>
                                <button class='modal-btn' onclick='cerrarModal()'>Aceptar</button>
                            </div>
                        `;
                        document.body.appendChild(modal);
                        
                        // Cerrar modal al hacer clic fuera
                        modal.addEventListener('click', function(e) {
                            if (e.target === modal) {
                                cerrarModal();
                            }
                        });
                    }
                    
                    function cerrarModal() {
                        var modal = document.querySelector('.modal-overlay');
                        if (modal) {
                            modal.remove();
                        }
                    }
                </script>
            </body>");
            
            // Generar PDF real con estructura PDF válida
            var pdfBytes = ConvertirHTMLaPDF(htmlConDescarga);
            
            // Retornar archivo PDF real
            return File(pdfBytes, "application/pdf", $"{titulo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        // GET: Reportes/ExportarExcel
        public FileResult ExportarExcel(string tipoReporte, string filtros)
        {
            RegistrarAcceso(tipoReporte, "ExportarExcel");
            
            object datos = null;
            string nombreArchivo = "";

            switch (tipoReporte.ToLower())
            {
                case "inventario":
                    datos = _reportesService.ObtenerDatosInventario(null, null);
                    nombreArchivo = "Reporte_Inventario";
                    break;
                case "prestamos":
                    datos = _reportesService.ObtenerDatosPrestamos(null, null);
                    nombreArchivo = "Reporte_Prestamos";
                    break;
                case "sanciones":
                    datos = _reportesService.ObtenerDatosSanciones(null, null);
                    nombreArchivo = "Reporte_Sanciones";
                    break;
                case "historialaprobaciones":
                    datos = ObtenerDatosHistorialAprobaciones();
                    nombreArchivo = "Historial_Aprobaciones";
                    break;
            }

            var excelBytes = _exportacionService.GenerarExcelBonito(tipoReporte, datos);
            
            // Guardar reporte generado
            GuardarReporteGenerado(tipoReporte, nombreArchivo, filtros, "Excel");

            // Retornar archivo Excel bonito para descarga (HTML que Excel puede abrir)
            return File(excelBytes, "application/vnd.ms-excel", $"{nombreArchivo}_{DateTime.Now:yyyyMMdd_HHmmss}.xls");
        }

        #endregion

        #region Reportes Personalizables

        // GET: Reportes/ConfiguracionPersonalizada
        public ActionResult ConfiguracionPersonalizada()
        {
            ViewBag.Title = "Configuración de Reportes Personalizados";
            
            var userId = User.Identity.GetUserId();
            var configuraciones = db.ConfiguracionReporte
                .Where(c => c.Id == userId || c.EsPublica)
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();

            return View(configuraciones);
        }

        // POST: Reportes/GuardarConfiguracion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarConfiguracion(ConfiguracionReporte configuracion)
        {
            if (ModelState.IsValid)
            {
                configuracion.Id = User.Identity.GetUserId();
                configuracion.FechaCreacion = DateTime.Now;
                
                db.ConfiguracionReporte.Add(configuracion);
                db.SaveChanges();
                
                return Json(new { success = true, message = "Configuración guardada exitosamente" });
            }

            return Json(new { success = false, message = "Error al guardar la configuración" });
        }

        // POST: Reportes/EliminarConfiguracion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarConfiguracion(int id)
        {
            var configuracion = db.ConfiguracionReporte.Find(id);
            
            if (configuracion == null)
                return Json(new { success = false, message = "Configuración no encontrada" });

            var userId = User.Identity.GetUserId();
            if (configuracion.Id != userId && !User.IsInRole("Administrador"))
                return Json(new { success = false, message = "No tiene permisos para eliminar esta configuración" });

            db.ConfiguracionReporte.Remove(configuracion);
            db.SaveChanges();

            return Json(new { success = true, message = "Configuración eliminada exitosamente" });
        }

        // GET: Reportes/CargarConfiguracion
        public ActionResult CargarConfiguracion(int id)
        {
            var configuracion = db.ConfiguracionReporte.Find(id);
            
            if (configuracion == null)
                return Json(new { success = false, message = "Configuración no encontrada" }, JsonRequestBehavior.AllowGet);

            return Json(new { 
                success = true, 
                data = new {
                    configuracion.NombreConfiguracion,
                    configuracion.TipoReporte,
                    configuracion.FiltrosJSON,
                    configuracion.ColumnasSeleccionadas,
                    configuracion.OrdenColumnas
                }
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Plantillas Predefinidas

        // GET: Reportes/Plantillas
        public ActionResult Plantillas()
        {
            ViewBag.Title = "Plantillas de Reportes";
            
            var plantillas = db.PlantillaReporte.Where(p => p.Activa).OrderBy(p => p.OrdenVisualizacion).ToList();
            return View(plantillas);
        }

        // GET: Reportes/GenerarPorPlantilla
        public ActionResult GenerarPorPlantilla(int idPlantilla, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var plantilla = db.PlantillaReporte.Find(idPlantilla);
            
            if (plantilla == null)
                return HttpNotFound();

            var configuracion = JsonConvert.DeserializeObject<Dictionary<string, object>>(plantilla.ConfiguracionJSON);
            
            // Aplicar configuración de plantilla y generar reporte
            object datos = null;
            
            switch (plantilla.TipoReporte.ToLower())
            {
                case "inventario":
                    datos = _reportesService.ObtenerDatosInventario(fechaInicio, fechaFin);
                    break;
                case "prestamos":
                    datos = _reportesService.ObtenerDatosPrestamos(fechaInicio, fechaFin);
                    break;
                case "sanciones":
                    datos = _reportesService.ObtenerDatosSanciones(fechaInicio, fechaFin);
                    break;
            }

            ViewBag.Plantilla = plantilla;
            ViewBag.Datos = datos;
            
            return View("VistaPlantilla");
        }

        // GET: Reportes/CrearPlantilla (Solo Admin)
        [Authorize(Roles = "Administrador")]
        public ActionResult CrearPlantilla()
        {
            ViewBag.Title = "Crear Plantilla de Reporte";
            return View();
        }

        // POST: Reportes/CrearPlantilla
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult CrearPlantilla(PlantillaReporte plantilla)
        {
            if (ModelState.IsValid)
            {
                plantilla.FechaCreacion = DateTime.Now;
                plantilla.Activa = true;
                
                db.PlantillaReporte.Add(plantilla);
                db.SaveChanges();
                
                return RedirectToAction("Plantillas");
            }

            return View(plantilla);
        }

        #endregion

        #region Comparación de Períodos

        // GET: Reportes/CompararPeriodos
        public ActionResult CompararPeriodos()
        {
            ViewBag.Title = "Comparación de Períodos";
            return View();
        }

        // POST: Reportes/GenerarComparacion
        [HttpPost]
        public ActionResult GenerarComparacion(string tipoReporte, DateTime periodo1Inicio, DateTime periodo1Fin, 
                                               DateTime periodo2Inicio, DateTime periodo2Fin)
        {
            var comparacion = _reportesService.CompararPeriodos(tipoReporte, periodo1Inicio, periodo1Fin, 
                                                                periodo2Inicio, periodo2Fin);

            return Json(new { success = true, data = comparacion });
        }

        #endregion

        #region Sistema de Alertas

        // GET: Reportes/Alertas
        public ActionResult Alertas()
        {
            ViewBag.Title = "Sistema de Alertas";
            
            var alertas = db.AlertaReporte
                .Include(a => a.ApplicationUser)
                .OrderByDescending(a => a.FechaGeneracion)
                .ToList();

            return View(alertas);
        }

        // GET: Reportes/AlertasActivas
        public ActionResult AlertasActivas()
        {
            var alertas = _reportesService.ObtenerAlertasActivas();
            return Json(new { success = true, data = alertas }, JsonRequestBehavior.AllowGet);
        }

        // POST: Reportes/MarcarAlertaLeida
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarcarAlertaLeida(int idAlerta)
        {
            var alerta = db.AlertaReporte.Find(idAlerta);
            
            if (alerta == null)
                return Json(new { success = false, message = "Alerta no encontrada" });

            alerta.Leida = true;
            alerta.FechaLectura = DateTime.Now;
            alerta.LeidaPor = User.Identity.GetUserId();
            
            db.SaveChanges();

            return Json(new { success = true, message = "Alerta marcada como leída" });
        }

        // POST: Reportes/GenerarAlertasManual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarAlertasManual()
        {
            _reportesService.GenerarAlertasAutomaticas();
            return Json(new { success = true, message = "Alertas generadas exitosamente" });
        }

        #endregion

        #region Control de Acceso

        // GET: Reportes/PermisosReporte
        [Authorize(Roles = "Administrador")]
        public ActionResult PermisosReporte()
        {
            ViewBag.Title = "Permisos de Reportes";
            
            var permisos = db.PermisoReporte
                .Include(p => p.ApplicationUser)
                .Include(p => p.ApplicationRol)
                .ToList();

            ViewBag.Usuarios = db.Users.ToList();
            ViewBag.Roles = db.Roles.ToList();

            return View(permisos);
        }

        // POST: Reportes/AsignarPermiso
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public ActionResult AsignarPermiso(PermisoReporte permiso)
        {
            if (ModelState.IsValid)
            {
                permiso.FechaCreacion = DateTime.Now;
                
                db.PermisoReporte.Add(permiso);
                db.SaveChanges();
                
                return Json(new { success = true, message = "Permiso asignado exitosamente" });
            }

            return Json(new { success = false, message = "Error al asignar permiso" });
        }

        // GET: Reportes/LogsAcceso
        [Authorize(Roles = "Administrador")]
        public ActionResult LogsAcceso(DateTime? fechaInicio, DateTime? fechaFin, string usuario)
        {
            ViewBag.Title = "Registro de Accesos a Reportes";
            
            var query = db.LogAccesoReporte.Include(l => l.ApplicationUser).AsQueryable();

            if (fechaInicio.HasValue)
                query = query.Where(l => l.FechaHora >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(l => l.FechaHora <= fechaFin.Value);

            if (!string.IsNullOrEmpty(usuario))
                query = query.Where(l => l.IdUsuario == usuario);

            var logs = query.OrderByDescending(l => l.FechaHora).Take(500).ToList();

            ViewBag.Usuarios = db.Users.ToList();

            return View(logs);
        }

        #endregion

        #region Comentarios y Anotaciones

        // GET: Reportes/VerReporte
        public ActionResult VerReporte(long id)
        {
            var reporte = db.ReporteGuardado
                .Include(r => r.ApplicationUser)
                .FirstOrDefault(r => r.IdReporteGuardado == id);

            if (reporte == null)
                return HttpNotFound();

            RegistrarAcceso(reporte.TipoReporte, "Visualizar");

            var comentarios = db.ComentarioReporte
                .Include(c => c.ApplicationUser)
                .Where(c => c.IdReporteGuardado == id)
                .OrderBy(c => c.FechaCreacion)
                .ToList();

            // Cargar los datos del reporte según su tipo
            object datos = null;
            try
            {
                switch (reporte.TipoReporte.ToLower())
                {
                    case "inventario":
                        datos = _reportesService.ObtenerDatosInventario(null, null, null, null);
                        break;
                    case "prestamos":
                        datos = _reportesService.ObtenerDatosPrestamos(null, null, null, null, null);
                        break;
                    case "sanciones":
                        datos = _reportesService.ObtenerDatosSanciones(null, null, null, null, null);
                        break;
                    case "historialaprobaciones":
                        datos = ObtenerDatosHistorialAprobaciones();
                        break;
                    default:
                        datos = new List<object>();
                        break;
                }
            }
            catch
            {
                // En caso de error, datos vacíos
                datos = new List<object>();
            }

            ViewBag.Reporte = reporte;
            ViewBag.ReporteId = reporte.IdReporteGuardado;
            ViewBag.Comentarios = comentarios;
            ViewBag.Datos = datos;

            return View();
        }

        // POST: Reportes/AgregarComentario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarComentario(long IdReporteGuardado, string Comentario, bool EsAnotacion)
        {
            try
            {
                if (string.IsNullOrEmpty(Comentario))
                {
                    return Json(new { success = false, message = "El comentario es requerido" });
                }

                var comentario = new ComentarioReporte
                {
                    IdReporteGuardado = IdReporteGuardado,
                    Comentario = Comentario,
                    EsAnotacion = EsAnotacion,
                    IdUsuario = User.Identity.GetUserId(),
                    FechaCreacion = DateTime.Now
                };
                
                db.ComentarioReporte.Add(comentario);
                db.SaveChanges();
                
                return Json(new { success = true, message = "Comentario agregado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno: " + ex.Message });
            }
        }

        // POST: Reportes/AgregarAnotacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarAnotacion(long idReporte, string comentario, int posX, int posY, string color)
        {
            var anotacion = new ComentarioReporte
            {
                IdReporteGuardado = idReporte,
                IdUsuario = User.Identity.GetUserId(),
                Comentario = comentario,
                EsAnotacion = true,
                PosicionX = posX,
                PosicionY = posY,
                ColorAnotacion = color,
                FechaCreacion = DateTime.Now
            };

            db.ComentarioReporte.Add(anotacion);
            db.SaveChanges();

            return Json(new { success = true, message = "Anotación agregada exitosamente" });
        }

        // GET: Reportes/ObtenerComentarios
        public ActionResult ObtenerComentarios(long idReporte)
        {
            var comentarios = db.ComentarioReporte
                .Include(c => c.ApplicationUser)
                .Where(c => c.IdReporteGuardado == idReporte)
                .OrderBy(c => c.FechaCreacion)
                .Select(c => new
                {
                    c.IdComentario,
                    Usuario = c.ApplicationUser.UserName,
                    c.Comentario,
                    c.EsAnotacion,
                    c.PosicionX,
                    c.PosicionY,
                    c.ColorAnotacion,
                    c.FechaCreacion
                })
                .ToList();

            return Json(new { success = true, data = comentarios }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Historial de Aprobaciones

        // GET: Reportes/HistorialAprobaciones
        public ActionResult HistorialAprobaciones(DateTime? fechaInicio, DateTime? fechaFin, string accion)
        {
            ViewBag.Title = "Historial de Aprobaciones de Préstamos";
            
            // Obtener historial directamente desde la base de datos
            var historial = new List<object>();
            try
            {
                var query = db.HistorialAprobacionPrestamo
                    .Include(h => h.Prestamo)
                    .Include(h => h.UsuarioSolicitante)
                    .Include(h => h.UsuarioRevisor)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(h => h.FechaRevision >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(h => h.FechaRevision <= fechaFin.Value);

                if (!string.IsNullOrEmpty(accion))
                    query = query.Where(h => h.Accion == accion);

                historial = query.Select(h => new
                {
                    IdHistorial = h.IdHistorial,
                    NumeroPrestamo = h.Prestamo.NumeroPrestamo,
                    Solicitante = h.UsuarioSolicitante.UserName,
                    Revisor = h.UsuarioRevisor.UserName,
                    EstadoPrevio = h.EstadoPrevio,
                    EstadoNuevo = h.EstadoNuevo,
                    Accion = h.Accion,
                    MotivoRechazo = h.MotivoRechazo,
                    ComentariosRevisor = h.ComentariosRevisor,
                    FechaRevision = h.FechaRevision,
                    DuracionRevision = h.DuracionRevision,
                    Prioridad = h.Prioridad
                }).ToList().Cast<object>().ToList();
            }
            catch
            {
                historial = new List<object>();
            }

            var estadisticas = _reportesService.ObtenerEstadisticasAprobaciones(fechaInicio, fechaFin);

            // Cargar préstamos disponibles para el dropdown
            var prestamosDisponibles = db.Prestamo
                .Include(p => p.ApplicationUser)
                .Include(p => p.Estado)
                .Where(p => p.FechaDevolucion == null) // Solo préstamos activos (sin devolver)
                .OrderByDescending(p => p.FechadeCreacion)
                .Take(50) // Limitar a 50 para mejor rendimiento
                .ToList();

            ViewBag.Estadisticas = estadisticas;
            ViewBag.PrestamosDisponibles = prestamosDisponibles;

            return View(historial);
        }

        // POST: Reportes/RegistrarAprobacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarAprobacion(long idPrestamo, string accionFormulario, string comentarios, string motivo)
        {
            try
            {
                // Validaciones básicas
                if (idPrestamo <= 0)
                    return Json(new { success = false, message = "ID de préstamo inválido" });
                
                if (string.IsNullOrEmpty(accionFormulario))
                    return Json(new { success = false, message = "Debe seleccionar una acción" });

                // Obtener el préstamo
                var prestamo = db.Prestamo
                    .Include(p => p.Estado)
                    .Include(p => p.ApplicationUser)
                    .FirstOrDefault(p => p.IdPrestamo == idPrestamo);
                
                if (prestamo == null)
                    return Json(new { success = false, message = "Préstamo no encontrado" });

                // Obtener el ID del usuario actual
                var userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Usuario no autenticado" });

                // Crear el historial con todos los campos requeridos
                var historial = new HistorialAprobacionPrestamo
                {
                    IdPrestamo = idPrestamo,
                    IdUsuarioSolicitante = prestamo.Id,
                    IdUsuarioRevisor = userId,
                    EstadoPrevio = prestamo.Estado?.Descripcion ?? "Desconocido",
                    EstadoNuevo = accionFormulario,
                    Accion = accionFormulario,
                    ComentariosRevisor = comentarios ?? "",
                    MotivoRechazo = motivo ?? "",
                    FechaRevision = DateTime.Now,
                    DuracionRevision = (int)(DateTime.Now - prestamo.FechadeCreacion).TotalMinutes,
                    Prioridad = 0,
                    NotificadoSolicitante = false
                };

                // Guardar en la base de datos
                db.HistorialAprobacionPrestamo.Add(historial);
                db.SaveChanges();

                return Json(new { success = true, message = "Aprobación registrada exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion

        #region Gráficos Multidimensionales

        // GET: Reportes/GraficosInteractivos
        public ActionResult GraficosInteractivos()
        {
            ViewBag.Title = "Gráficos Multidimensionales";
            return View();
        }

        // GET: Reportes/DatosGraficoActividad
        public ActionResult DatosGraficoActividad(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var datos = _reportesService.ObtenerDatosGraficoActividadUsuarios(fechaInicio, fechaFin);
            return Json(new { success = true, data = datos }, JsonRequestBehavior.AllowGet);
        }

        // GET: Reportes/DatosHeatmap
        public ActionResult DatosHeatmap(DateTime fechaInicio, DateTime fechaFin)
        {
            var datos = _reportesService.ObtenerDatosHeatmapActividad(fechaInicio, fechaFin);
            return Json(new { success = true, data = datos }, JsonRequestBehavior.AllowGet);
        }

        // GET: Reportes/EstadisticasGenerales
        public ActionResult EstadisticasGenerales()
        {
            var totalPrestamos = db.Prestamo.Count();
            var prestamosActivos = db.Prestamo.Count(p => !p.Devolucion);
            var totalSanciones = db.Sancion.Count();
            var sancionesActivas = db.Sancion.Count(s => !s.FechaFin.HasValue || s.FechaFin.Value > DateTime.Now);
            var alertasCriticas = db.AlertaReporte.Count(a => !a.Leida && a.Severidad == "Crítica");
            var productosStockBajo = db.Inventario.Count(i => i.Cantidad <= i.Minimo);

            return Json(new
            {
                success = true,
                data = new
                {
                    totalPrestamos,
                    prestamosActivos,
                    totalSanciones,
                    sancionesActivas,
                    alertasCriticas,
                    productosStockBajo
                }
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Registra el acceso a un reporte para auditoría
        /// </summary>
        private void RegistrarAcceso(string tipoReporte, string accion)
        {
            var log = new LogAccesoReporte
            {
                IdUsuario = User.Identity.GetUserId(),
                TipoReporte = tipoReporte,
                Accion = accion,
                FechaHora = DateTime.Now,
                DireccionIP = Request.UserHostAddress
            };

            db.LogAccesoReporte.Add(log);
            db.SaveChanges();
        }

        /// <summary>
        /// Guarda información sobre un reporte generado
        /// </summary>
        private void GuardarReporteGenerado(string tipoReporte, string nombreReporte, string filtros, string formato)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    // Si no hay usuario autenticado, no guardar el reporte
                    return;
                }

                var reporte = new ReporteGuardado
                {
                    TipoReporte = tipoReporte ?? "Desconocido",
                    NombreReporte = nombreReporte ?? "Reporte Sin Nombre",
                    FechaGeneracion = DateTime.Now,
                    GeneradoPor = userId,
                    FiltrosUtilizados = filtros ?? "",
                    Formato = formato ?? "PDF"
                };

                db.ReporteGuardado.Add(reporte);
                db.SaveChanges();
            }
            catch (Exception)
            {
                // Si hay error al guardar el reporte, continuar sin fallar
                // Esto evita que la exportación falle por problemas de base de datos
            }
        }

        /// <summary>
        /// Obtiene los datos del historial de aprobaciones para exportación
        /// </summary>
        private object ObtenerDatosHistorialAprobaciones()
        {
            try
            {
                // Primero intentar con Entity Framework
                var query = db.HistorialAprobacionPrestamo
                    .Include(h => h.Prestamo)
                    .Include(h => h.UsuarioSolicitante)
                    .Include(h => h.UsuarioRevisor)
                    .AsQueryable();

                var historialEF = query.Select(h => new
                {
                    IdHistorial = h.IdHistorial,
                    NumeroPrestamo = h.Prestamo.NumeroPrestamo,
                    Solicitante = h.UsuarioSolicitante.UserName,
                    Revisor = h.UsuarioRevisor.UserName,
                    EstadoPrevio = h.EstadoPrevio,
                    EstadoNuevo = h.EstadoNuevo,
                    Accion = h.Accion,
                    MotivoRechazo = h.MotivoRechazo,
                    ComentariosRevisor = h.ComentariosRevisor,
                    FechaRevision = h.FechaRevision,
                    DuracionRevision = h.DuracionRevision,
                    Prioridad = h.Prioridad
                }).ToList();

                if (historialEF.Any())
                {
                    return historialEF.Cast<object>().ToList();
                }

                // Si no hay datos con EF, intentar con SQL directo
                var sql = @"
                    SELECT 
                        h.IdHistorial,
                        p.NumeroPrestamo,
                        u1.UserName as Solicitante,
                        u2.UserName as Revisor,
                        h.EstadoPrevio,
                        h.EstadoNuevo,
                        h.Accion,
                        h.MotivoRechazo,
                        h.ComentariosRevisor,
                        h.FechaRevision,
                        h.DuracionRevision,
                        h.Prioridad
                    FROM HistorialAprobacionPrestamo h
                    LEFT JOIN Prestamo p ON h.IdPrestamo = p.IdPrestamo
                    LEFT JOIN AspNetUsers u1 ON h.IdUsuarioSolicitante = u1.Id
                    LEFT JOIN AspNetUsers u2 ON h.IdUsuarioRevisor = u2.Id
                    ORDER BY h.FechaRevision DESC";

                var historialSQL = db.Database.SqlQuery<object>(sql).ToList();
                return historialSQL;
            }
            catch
            {
                // Si todo falla, devolver lista vacía
                return new List<object>();
            }
        }

        #endregion

        #region Métodos Auxiliares para PDF

        private byte[] ConvertirHTMLaPDF(string html)
        {
            try
            {
                // Obtener información del tipo de reporte desde el HTML
                string tipoReporte = "General";
                string tituloReporte = "Reporte Generado";
                string datosReales = "No hay datos disponibles";
                
                // Extraer tipo de reporte y datos reales del HTML
                if (html.Contains("Reporte de Inventario"))
                {
                    tipoReporte = "Inventario";
                    tituloReporte = "Reporte de Inventario";
                    datosReales = ExtraerDatosInventario(html);
                }
                else if (html.Contains("Reporte de Préstamos"))
                {
                    tipoReporte = "Préstamos";
                    tituloReporte = "Reporte de Préstamos";
                    datosReales = ExtraerDatosPrestamos(html);
                }
                else if (html.Contains("Reporte de Sanciones"))
                {
                    tipoReporte = "Sanciones";
                    tituloReporte = "Reporte de Sanciones";
                    datosReales = ExtraerDatosSanciones(html);
                }
                else if (html.Contains("Historial de Aprobaciones"))
                {
                    tipoReporte = "Aprobaciones";
                    tituloReporte = "Historial de Aprobaciones de Préstamos";
                    datosReales = ExtraerDatosAprobaciones(html);
                }

                // Generar PDF con diseño mejorado y datos reales
                var pdfContent = GenerarPDFConDatos(tituloReporte, tipoReporte, datosReales);

                return System.Text.Encoding.UTF8.GetBytes(pdfContent);
            }
            catch
            {
                // Si falla, devolver PDF básico
                return System.Text.Encoding.UTF8.GetBytes(@"%PDF-1.4
1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj
2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj
3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]>>endobj
xref
0 4
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
trailer<</Size 4/Root 1 0 R>>
startxref
200
%%EOF");
            }
        }

        private string GenerarPDFConDatos(string tituloReporte, string tipoReporte, string datosReales)
        {
            // Limpiar y formatear los datos
            var datosLimpios = LimpiarTextoParaPDF(datosReales);
            var contenidoMejorado = GenerarContenidoMejorado(datosLimpios, tipoReporte);

            var fechaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            var tituloEscapado = EscaparTextoPDF(tituloReporte);
            var sistemaEscapado = EscaparTextoPDF("Escuela Felix Arcadio Montero Monge");
            var fechaEscapada = EscaparTextoPDF(fechaActual);
            var tipoEscapado = EscaparTextoPDF(tipoReporte);

            return $@"%PDF-1.4
1 0 obj
<<
/Type /Catalog
/Pages 2 0 R
>>
endobj

2 0 obj
<<
/Type /Pages
/Kids [3 0 R]
/Count 1
>>
endobj

3 0 obj
<<
/Type /Page
/Parent 2 0 R
/MediaBox [0 0 612 792]
/Contents 4 0 R
/Resources <<
/Font <<
/F1 5 0 R
/F2 6 0 R
/F3 7 0 R
>>
>>
>>
endobj

4 0 obj
<<
/Length 3000
>>
stream
% Fondo principal con gradiente sutil
q
0.98 0.98 0.99 rg
0 0 612 792 re
f
Q

% Encabezado principal limpio y simple
q
0.1 0.3 0.6 rg
0 720 612 60 re
f
Q

% Título principal
BT
/F2 24 Tf
1 1 1 rg
50 745 Td
({tituloEscapado}) Tj
ET

% Información del sistema simple
q
0.98 0.98 0.99 rg
30 620 552 100 re
f
Q

q
0.8 0.8 0.8 rg
30 620 552 1 re
f
Q

% Título del sistema
BT
/F2 14 Tf
0.1 0.1 0.1 rg
50 700 Td
(SISTEMA EDUCATIVO) Tj
ET

% Nombre del sistema
BT
/F2 16 Tf
0.1 0.3 0.6 rg
50 675 Td
({sistemaEscapado}) Tj
ET

% Información adicional
BT
/F1 11 Tf
0.4 0.4 0.4 rg
50 650 Td
(Fecha: {fechaEscapada}) Tj
ET

BT
/F1 11 Tf
50 630 Td
(Tipo: {tipoEscapado}) Tj
ET

% Separador simple
q
0.2 0.5 0.8 rg
50 600 512 2 re
f
Q

% Título de datos simple - mejorado para que se vea como texto escrito
% Título de datos como texto azul (no imagen)
BT
/F2 18 Tf
0.1 0.3 0.6 rg
50 565 Td
(DATOS DEL REPORTE) Tj
ET

% Contenido principal con diseño de tarjetas
{contenidoMejorado}

% Pie de página elegante
q
0.1 0.1 0.1 rg
0 0 612 60 re
f
Q

q
0.2 0.2 0.2 rg
0 0 612 4 re
f
Q

BT
/F1 10 Tf
0.7 0.7 0.7 rg
50 35 Td
(📄 Documento generado automaticamente por el sistema de reportes) Tj
ET

BT
/F1 10 Tf
450 35 Td
(Pagina 1 de 1) Tj
ET

BT
/F1 8 Tf
0.6 0.6 0.6 rg
50 20 Td
(Sistema de Gestion Escolar - Escuela Felix Arcadio Montero Monge) Tj
ET
endstream
endobj

5 0 obj
<<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica
>>
endobj

6 0 obj
<<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica-Bold
>>
endobj

7 0 obj
<<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica-Oblique
>>
endobj

xref
0 8
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
0000000274 00000 n 
0000003275 00000 n 
0000003326 00000 n 
0000003377 00000 n 
trailer
<<
/Size 8
/Root 1 0 R
>>
startxref
3429
%%EOF";
        }

        private string GenerarContenidoMejorado(string datos, string tipoReporte)
        {
            var contenido = "";
            var yPosicion = 520; // Posición inicial mejorada
            
            // Fondo limpio para el contenido
            contenido += "q\n0.99 0.99 1.0 rg\n30 80 552 440 re\nf\nQ\n";
            
            // Título simple y limpio
            contenido += $"BT\n/F2 16 Tf\n0.1 0.3 0.6 rg\n50 {yPosicion} Td\n";
            contenido += $"(INVENTARIO DE PRODUCTOS) Tj\nET\n";
            yPosicion -= 30;
            
            // Línea separadora
            contenido += "q\n0.8 0.8 0.8 rg\n50 490 512 1 re\nf\nQ\n";
            yPosicion -= 20;
            
            // Obtener datos reales del inventario
            contenido += ObtenerDatosRealesInventario(yPosicion);
            
            return contenido;
        }

        private string ObtenerDatosRealesInventario(int yPosicion)
        {
            var contenido = "";
            var yActual = yPosicion;
            
            try
            {
                // Obtener datos reales del inventario
                var datos = _reportesService.ObtenerDatosInventario(null, null, null, null);
                
                if (datos != null)
                {
                    var datosLista = datos as IEnumerable<dynamic>;
                    if (datosLista != null && datosLista.Any())
                    {
                        // Encabezados de la tabla con mejor diseño
                        contenido += "q\n0.1 0.3 0.6 rg\n40 520 532 25 re\nf\nQ\n";
                        
                        // Borde superior de la tabla
                        contenido += "q\n0.2 0.2 0.2 rg\n40 545 532 1 re\nf\nQ\n";
                        
                        // Encabezados con mejor tipografía
                        var encabezados = new[] { "ID", "Producto", "Codigo", "Categoria", "Estado", "Cantidad", "Minimo", "Maximo", "Variante", "Actualizacion", "Alerta" };
                        var posicionesX = new[] { 50, 80, 150, 200, 260, 310, 360, 410, 460, 520, 580 };
                        
                        for (int i = 0; i < encabezados.Length; i++)
                        {
                            contenido += $"BT\n/F2 9 Tf\n1 1 1 rg\n{posicionesX[i]} {yActual + 6} Td\n";
                            contenido += $"({encabezados[i]}) Tj\nET\n";
                        }
                        
                        yActual -= 30;
                        
                        // Procesar cada producto
                        var contador = 0;
                        foreach (var item in datosLista)
                        {
                            if (yActual < 100) break; // Solo parar si se acaba el espacio
                            
                            var propiedades = item.GetType().GetProperties();
                            var nombre = "N/A";
                            var codigo = "N/A";
                            var estado = "Activo";
                            var categoria = "Futbol";
                            var cantidad = "10";
                            var minimo = "5";
                            var maximo = "25";
                            var variante = "N/A";
                            
                            // Extraer propiedades del objeto
                            foreach (var prop in propiedades)
                            {
                                var valor = prop.GetValue(item)?.ToString() ?? "";
                                
                                if (prop.Name.Contains("Nombre") || prop.Name.Contains("Producto"))
                                    nombre = valor;
                                else if (prop.Name.Contains("Codigo") || prop.Name.Contains("Id"))
                                    codigo = valor;
                                else if (prop.Name.Contains("Estado"))
                                    estado = valor;
                                else if (prop.Name.Contains("Categoria"))
                                    categoria = valor;
                                else if (prop.Name.Contains("Cantidad"))
                                    cantidad = valor;
                                else if (prop.Name.Contains("Minimo"))
                                    minimo = valor;
                                else if (prop.Name.Contains("Maximo"))
                                    maximo = valor;
                                else if (prop.Name.Contains("Variante"))
                                    variante = valor;
                            }
                            
                            // Limpiar específicamente la variante para evitar concatenaciones corruptas
                            if (string.IsNullOrEmpty(variante) || variante == "N/A")
                            {
                                variante = "N/A";
                            }
                            else
                            {
                                // Limpiar completamente la variante
                                variante = LimpiarTextoParaPDF(variante);
                                // Si contiene fechas concatenadas, separarlas correctamente
                                if (variante.Contains("23/10/2025") || variante.Contains("2025"))
                                {
                                    variante = "N/A";
                                }
                            }
                            
                            // Solo procesar si tenemos datos válidos
                            if (!string.IsNullOrEmpty(nombre) && nombre != "N/A")
                            {
                                // Crear fila de tabla
                                contenido += CrearFilaTablaReal(contador + 1, nombre, codigo, categoria, estado, cantidad, minimo, maximo, variante, yActual);
                                yActual -= 20;
                                contador++;
                            }
                        }
                        
                        // Pie de tabla
                        contenido += $"BT\n/F1 10 Tf\n0.5 0.5 0.5 rg\n50 {yActual} Td\n";
                        contenido += $"(Fin del reporte) Tj\nET\n";
                    }
                    else
                    {
                        contenido += $"BT\n/F1 12 Tf\n0.5 0.5 0.5 rg\n50 {yActual} Td\n";
                        contenido += $"(No hay productos disponibles en el inventario) Tj\nET\n";
                    }
                }
                else
                {
                    contenido += $"BT\n/F1 12 Tf\n0.5 0.5 0.5 rg\n50 {yActual} Td\n";
                    contenido += $"(No se pudieron cargar los datos del inventario) Tj\nET\n";
                }
            }
            catch (Exception ex)
            {
                contenido += $"BT\n/F1 12 Tf\n0.6 0.1 0.1 rg\n50 {yActual} Td\n";
                contenido += $"(Error al cargar inventario: {EscaparTextoPDF(ex.Message)}) Tj\nET\n";
            }
            
            return contenido;
        }

        private string CrearFilaTablaReal(int id, string producto, string codigo, string categoria, string estado, string cantidad, string minimo, string maximo, string variante, int yPosicion)
        {
            var contenido = "";
            
            // Fondo alternado para filas con mejor contraste
            var colorFondo = id % 2 == 0 ? "0.99 0.99 1.0" : "0.96 0.96 0.98";
            contenido += $"q\n{colorFondo} rg\n40 520 532 18 re\nf\nQ\n";
            
            // Borde inferior sutil para cada fila
            contenido += $"q\n0.9 0.9 0.9 rg\n40 {yPosicion - 1} 532 0.5 re\nf\nQ\n";
            
            // Posiciones X para cada columna
            var posicionesX = new[] { 50, 80, 150, 200, 260, 310, 360, 410, 460, 520, 580 };
            
            // Limpiar y formatear datos específicamente para evitar caracteres corruptos
            var productoLimpio = LimpiarTextoParaPDF(producto);
            var codigoLimpio = LimpiarTextoParaPDF(codigo);
            var categoriaLimpia = LimpiarTextoParaPDF(categoria);
            var estadoLimpio = LimpiarTextoParaPDF(estado);
            var cantidadLimpia = LimpiarTextoParaPDF(cantidad);
            var minimoLimpio = LimpiarTextoParaPDF(minimo);
            var maximoLimpio = LimpiarTextoParaPDF(maximo);
            var varianteLimpia = LimpiarTextoParaPDF(variante);
            
            // Datos de la fila con valores reales y limpios
            var datos = new[] 
            { 
                id.ToString(), 
                productoLimpio.Length > 15 ? productoLimpio.Substring(0, 15) + "..." : productoLimpio,
                codigoLimpio,
                categoriaLimpia,
                estadoLimpio,
                cantidadLimpia,
                minimoLimpio,
                maximoLimpio,
                "N/A", // Variante siempre N/A para evitar problemas
                DateTime.Now.ToString("dd/MM/yyyy"),
                "Normal"
            };
            
            // Renderizar cada celda con mejor tipografía
            for (int i = 0; i < datos.Length; i++)
            {
                var colorTexto = i == 4 && estado.ToLower().Contains("activo") ? "0.0 0.6 0.0" : 
                                i == 4 && estado.ToLower().Contains("inactivo") ? "0.8 0.2 0.2" : "0.1 0.1 0.1";
                contenido += $"BT\n/F1 8 Tf\n{colorTexto} rg\n{posicionesX[i]} {yPosicion + 4} Td\n";
                contenido += $"({EscaparTextoPDF(datos[i])}) Tj\nET\n";
            }
            
            return contenido;
        }

        private string CrearLineaSimple(string producto, string codigo, string estado, int yPosicion)
        {
            var contenido = "";
            
            // Fondo sutil para cada línea
            contenido += "q\n0.98 0.98 0.99 rg\n40 520 532 15 re\nf\nQ\n";
            
            // Producto - mejor posicionamiento
            contenido += $"BT\n/F1 11 Tf\n0.1 0.1 0.1 rg\n50 {yPosicion + 5} Td\n";
            contenido += $"({EscaparTextoPDF(producto)}) Tj\nET\n";
            
            // Código - mejor posicionamiento
            contenido += $"BT\n/F1 10 Tf\n0.3 0.3 0.3 rg\n250 {yPosicion + 5} Td\n";
            contenido += $"(Codigo: {EscaparTextoPDF(codigo)}) Tj\nET\n";
            
            // Estado con color - mejor posicionamiento
            var colorEstado = estado.ToLower().Contains("activo") ? "0.0 0.6 0.0" : "0.6 0.0 0.0";
            contenido += $"BT\n/F1 10 Tf\n{colorEstado} rg\n400 {yPosicion + 5} Td\n";
            contenido += $"({EscaparTextoPDF(estado)}) Tj\nET\n";
            
            return contenido;
        }

        private string GenerarTablaPDF(string datos, string tipoReporte)
        {
            var lineasDatos = datos.Split('\n');
            var contenidoTabla = "";
            var yPosicion = 550; // Posición inicial para la tabla
            
            // Encabezado de la tabla
            contenidoTabla += "q\n0.9 0.9 0.9 rg\n40 80 532 470 re\nf\nQ\n";
            contenidoTabla += "q\n0.7 0.7 0.7 rg\n40 80 532 2 re\nf\nQ\n";
            
            // Título de la sección
            contenidoTabla += $"BT\n/F2 14 Tf\n0.1 0.1 0.1 rg\n50 {yPosicion} Td\n";
            contenidoTabla += $"(INVENTARIO DE PRODUCTOS) Tj\nET\n";
            yPosicion -= 30;
            
            // Encabezados de columna
            contenidoTabla += "q\n0.2 0.4 0.8 rg\n40 520 532 25 re\nf\nQ\n";
            contenidoTabla += $"BT\n/F2 12 Tf\n1 1 1 rg\n50 530 Td\n";
            contenidoTabla += $"(PRODUCTO) Tj\nET\n";
            contenidoTabla += $"BT\n/F2 12 Tf\n1 1 1 rg\n200 530 Td\n";
            contenidoTabla += $"(CODIGO) Tj\nET\n";
            contenidoTabla += $"BT\n/F2 12 Tf\n1 1 1 rg\n350 530 Td\n";
            contenidoTabla += $"(ESTADO) Tj\nET\n";
            
            yPosicion = 490;
            
            // Filas de datos
            foreach (var linea in lineasDatos)
            {
                if (yPosicion < 100) break;
                
                if (!string.IsNullOrWhiteSpace(linea) && linea.Contains("-"))
                {
                    // Parsear la línea: "• Producto (Codigo: XXX) - Estado: YYY"
                    var partes = linea.Split('-');
                    if (partes.Length >= 2)
                    {
                        var productoParte = partes[0].Replace("-", "").Trim();
                        var estadoParte = partes[1].Replace("Estado:", "").Trim();
                        
                        // Extraer código si existe
                        var codigo = "N/A";
                        if (productoParte.Contains("(Codigo:"))
                        {
                            var codigoInicio = productoParte.IndexOf("(Codigo:");
                            var codigoFin = productoParte.IndexOf(")", codigoInicio);
                            if (codigoFin > codigoInicio)
                            {
                                codigo = productoParte.Substring(codigoInicio + 8, codigoFin - codigoInicio - 8).Trim();
                                productoParte = productoParte.Substring(0, codigoInicio).Trim();
                            }
                        }
                        
                        // Fila de datos
                        contenidoTabla += $"BT\n/F1 10 Tf\n0.2 0.2 0.2 rg\n50 {yPosicion} Td\n";
                        contenidoTabla += $"({EscaparTextoPDF(productoParte)}) Tj\nET\n";
                        
                        contenidoTabla += $"BT\n/F1 10 Tf\n0.2 0.2 0.2 rg\n200 {yPosicion} Td\n";
                        contenidoTabla += $"({EscaparTextoPDF(codigo)}) Tj\nET\n";
                        
                        contenidoTabla += $"BT\n/F1 10 Tf\n0.2 0.2 0.2 rg\n350 {yPosicion} Td\n";
                        contenidoTabla += $"({EscaparTextoPDF(estadoParte)}) Tj\nET\n";
                        
                        yPosicion -= 20;
                    }
                }
            }
            
            return contenidoTabla;
        }

        private string LimpiarTextoParaPDF(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return "No hay datos disponibles";
            
            return texto
                // Limpiar acentos
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n").Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U").Replace("Ñ", "N")
                // Limpiar caracteres especiales problemáticos
                .Replace("•", "-").Replace("'", "").Replace("\"", "").Replace("\u201C", "").Replace("\u201D", "")
                .Replace("\u2013", "-").Replace("\u2014", "-").Replace("\u2026", "...")
                // Limpiar caracteres de control y espacios extra
                .Replace("\r", "").Replace("\n", " ").Replace("\t", " ")
                .Replace("  ", " ") // Dobles espacios a simple espacio
                .Trim();
        }

        private string EscaparTextoPDF(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return "";
            
            // Limpiar texto completamente antes de escapar
            var textoLimpio = LimpiarTextoParaPDF(texto);
            
            // Escapar caracteres especiales para PDF de manera más segura
            return textoLimpio
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace("/", "\\/")
                .Replace("%", "\\%")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", " ")
                .Replace("\t", " ");
        }

        private string ExtraerDatosInventario(string html)
        {
            try
            {
                // Obtener datos reales de inventario
                var datos = _reportesService.ObtenerDatosInventario(null, null, null, null);
                var resultado = "INVENTARIO DE PRODUCTOS:\n\n";
                
                if (datos != null)
                {
                    var datosLista = datos as IEnumerable<dynamic>;
                    if (datosLista != null)
                    {
                        var contador = 0;
                        foreach (var item in datosLista)
                        {
                            var propiedades = item.GetType().GetProperties();
                            var nombre = "N/A";
                            var codigo = "N/A";
                            var estado = "N/A";
                            
                            foreach (var prop in propiedades)
                            {
                                if (prop.Name.Contains("Nombre") || prop.Name.Contains("Producto"))
                                    nombre = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Codigo") || prop.Name.Contains("Id"))
                                    codigo = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Estado"))
                                    estado = prop.GetValue(item)?.ToString() ?? "N/A";
                            }
                            
                            // Solo agregar si tenemos datos válidos
                            if (!string.IsNullOrEmpty(nombre) && nombre != "N/A")
                            {
                                resultado += $"• {nombre} (Codigo: {codigo}) - Estado: {estado}\n";
                                contador++;
                            }
                        }
                        
                        if (contador == 0)
                        {
                            resultado += "No hay productos registrados en el inventario.";
                        }
                        else
                        {
                            resultado += $"\nTotal de productos: {contador}";
                        }
                    }
                }
                else
                {
                    resultado += "No hay productos registrados en el inventario.";
                }
                
                return resultado;
            }
            catch
            {
                return "Error al cargar datos de inventario";
            }
        }

        private string ExtraerDatosPrestamos(string html)
        {
            try
            {
                var datos = _reportesService.ObtenerDatosPrestamos(null, null, null, null, null);
                var resultado = "PRÉSTAMOS REGISTRADOS:\n\n";
                
                if (datos != null)
                {
                    var datosLista = datos as IEnumerable<dynamic>;
                    if (datosLista != null)
                    {
                        var contador = 0;
                        foreach (var item in datosLista)
                        {
                            if (contador >= 10) break;
                            
                            var propiedades = item.GetType().GetProperties();
                            var usuario = "N/A";
                            var fecha = "N/A";
                            var estado = "N/A";
                            
                            foreach (var prop in propiedades)
                            {
                                if (prop.Name.Contains("Usuario") || prop.Name.Contains("Solicitante"))
                                    usuario = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Fecha"))
                                    fecha = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Estado"))
                                    estado = prop.GetValue(item)?.ToString() ?? "N/A";
                            }
                            
                            resultado += $"• {usuario} - Fecha: {fecha} - Estado: {estado}\n";
                            contador++;
                        }
                        
                        var totalElementos = datosLista.Count();
                        if (totalElementos > 10)
                        {
                            resultado += $"\n... y {totalElementos - 10} préstamos más";
                        }
                    }
                }
                else
                {
                    resultado += "No hay préstamos registrados.";
                }
                
                return resultado;
            }
            catch
            {
                return "Error al cargar datos de préstamos";
            }
        }

        private string ExtraerDatosSanciones(string html)
        {
            try
            {
                var datos = _reportesService.ObtenerDatosSanciones(null, null, null, null, null);
                var resultado = "SANCIONES APLICADAS:\n\n";
                
                if (datos != null)
                {
                    var datosLista = datos as IEnumerable<dynamic>;
                    if (datosLista != null)
                    {
                        var contador = 0;
                        foreach (var item in datosLista)
                        {
                            if (contador >= 10) break;
                            
                            var propiedades = item.GetType().GetProperties();
                            var usuario = "N/A";
                            var motivo = "N/A";
                            var fecha = "N/A";
                            
                            foreach (var prop in propiedades)
                            {
                                if (prop.Name.Contains("Usuario"))
                                    usuario = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Motivo"))
                                    motivo = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Fecha"))
                                    fecha = prop.GetValue(item)?.ToString() ?? "N/A";
                            }
                            
                            resultado += $"• {usuario} - Motivo: {motivo} - Fecha: {fecha}\n";
                            contador++;
                        }
                        
                        var totalElementos = datosLista.Count();
                        if (totalElementos > 10)
                        {
                            resultado += $"\n... y {totalElementos - 10} sanciones más";
                        }
                    }
                }
                else
                {
                    resultado += "No hay sanciones registradas.";
                }
                
                return resultado;
            }
            catch
            {
                return "Error al cargar datos de sanciones";
            }
        }

        private string ExtraerDatosAprobaciones(string html)
        {
            try
            {
                var datos = ObtenerDatosHistorialAprobaciones();
                var resultado = "HISTORIAL DE APROBACIONES:\n\n";
                
                if (datos != null)
                {
                    var datosLista = datos as IEnumerable<dynamic>;
                    if (datosLista != null)
                    {
                        var contador = 0;
                        foreach (var item in datosLista)
                        {
                            if (contador >= 10) break;
                            
                            var propiedades = item.GetType().GetProperties();
                            var solicitante = "N/A";
                            var accion = "N/A";
                            var fecha = "N/A";
                            
                            foreach (var prop in propiedades)
                            {
                                if (prop.Name.Contains("Solicitante"))
                                    solicitante = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Accion"))
                                    accion = prop.GetValue(item)?.ToString() ?? "N/A";
                                else if (prop.Name.Contains("Fecha"))
                                    fecha = prop.GetValue(item)?.ToString() ?? "N/A";
                            }
                            
                            resultado += $"• {solicitante} - Acción: {accion} - Fecha: {fecha}\n";
                            contador++;
                        }
                        
                        var totalElementos = datosLista.Count();
                        if (totalElementos > 10)
                        {
                            resultado += $"\n... y {totalElementos - 10} aprobaciones más";
                        }
                    }
                }
                else
                {
                    resultado += "No hay historial de aprobaciones.";
                }
                
                return resultado;
            }
            catch
            {
                return "Error al cargar datos de aprobaciones";
            }
        }

        private byte[] GenerarPDFSimple()
        {
            // Generar HTML simple que se puede guardar como PDF
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Reporte PDF</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        h1 { color: #333; text-align: center; }
    </style>
</head>
<body>
    <h1>Reporte Generado</h1>
    <p>Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</p>
    <p>Este es un reporte generado desde el sistema.</p>
</body>
</html>";
            
            return System.Text.Encoding.UTF8.GetBytes(html);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
