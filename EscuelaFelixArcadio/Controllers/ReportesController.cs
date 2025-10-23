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
            
            return Content(htmlConDescarga, "text/html");
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

            ViewBag.Comentarios = comentarios;

            return View(reporte);
        }

        // POST: Reportes/AgregarComentario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarComentario(ComentarioReporte comentario)
        {
            if (ModelState.IsValid)
            {
                comentario.IdUsuario = User.Identity.GetUserId();
                comentario.FechaCreacion = DateTime.Now;
                
                db.ComentarioReporte.Add(comentario);
                db.SaveChanges();
                
                return Json(new { success = true, message = "Comentario agregado exitosamente" });
            }

            return Json(new { success = false, message = "Error al agregar comentario" });
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
