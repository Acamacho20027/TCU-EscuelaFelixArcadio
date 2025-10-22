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
            ViewBag.Title = "Reportes y Estadísticas";
            
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
        public ActionResult Prestamos(DateTime? fechaInicio, DateTime? fechaFin, string usuarios, string estados, bool? devueltos)
        {
            ViewBag.Title = "Reportes de Préstamos";
            
            List<string> listaUsuarios = !string.IsNullOrEmpty(usuarios) 
                ? usuarios.Split(',').ToList() 
                : null;
            
            List<int> listaEstados = !string.IsNullOrEmpty(estados) 
                ? estados.Split(',').Select(int.Parse).ToList() 
                : null;

            var datos = _reportesService.ObtenerDatosPrestamos(fechaInicio, fechaFin, listaUsuarios, listaEstados, devueltos);
            
            ViewBag.Usuarios = db.Users.ToList();
            ViewBag.Estados = db.Estado.ToList();
            
            return View(datos);
        }

        // GET: Reportes/Sanciones
        public ActionResult Sanciones(DateTime? fechaInicio, DateTime? fechaFin, string usuarios, string estados, string tipos)
        {
            ViewBag.Title = "Reportes de Sanciones";
            
            List<string> listaUsuarios = !string.IsNullOrEmpty(usuarios) 
                ? usuarios.Split(',').ToList() 
                : null;
            
            List<int> listaEstados = !string.IsNullOrEmpty(estados) 
                ? estados.Split(',').Select(int.Parse).ToList() 
                : null;
            
            List<string> listaTipos = !string.IsNullOrEmpty(tipos) 
                ? tipos.Split(',').ToList() 
                : null;

            var datos = _reportesService.ObtenerDatosSanciones(fechaInicio, fechaFin, listaUsuarios, listaEstados, listaTipos);
            
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
            }

            var html = _exportacionService.GenerarPDF(tipoReporte, datos, titulo);
            
            // Guardar reporte generado
            GuardarReporteGenerado(tipoReporte, titulo, filtros, "PDF");

            return Content(html, "text/html");
        }

        // GET: Reportes/ExportarExcel
        public FileResult ExportarExcel(string tipoReporte, string filtros)
        {
            RegistrarAcceso(tipoReporte, "ExportarExcel");
            
            byte[] archivoBytes = null;
            string nombreArchivo = "";

            switch (tipoReporte.ToLower())
            {
                case "inventario":
                    var datosInventario = _reportesService.ObtenerDatosInventario(null, null);
                    archivoBytes = _exportacionService.GenerarCSVInventario(datosInventario as IEnumerable<dynamic>);
                    nombreArchivo = $"Reporte_Inventario_{DateTime.Now:yyyyMMdd}.csv";
                    break;
                    
                case "prestamos":
                    var datosPrestamos = _reportesService.ObtenerDatosPrestamos(null, null);
                    archivoBytes = _exportacionService.GenerarCSVPrestamos(datosPrestamos as IEnumerable<dynamic>);
                    nombreArchivo = $"Reporte_Prestamos_{DateTime.Now:yyyyMMdd}.csv";
                    break;
                    
                case "sanciones":
                    var datosSanciones = _reportesService.ObtenerDatosSanciones(null, null);
                    archivoBytes = _exportacionService.GenerarCSVSanciones(datosSanciones as IEnumerable<dynamic>);
                    nombreArchivo = $"Reporte_Sanciones_{DateTime.Now:yyyyMMdd}.csv";
                    break;
            }

            // Guardar reporte generado
            GuardarReporteGenerado(tipoReporte, nombreArchivo, filtros, "Excel");

            return File(archivoBytes, "text/csv", nombreArchivo);
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
            
            var historial = _reportesService.ObtenerHistorialAprobaciones(fechaInicio, fechaFin, accion);
            var estadisticas = _reportesService.ObtenerEstadisticasAprobaciones(fechaInicio, fechaFin);

            ViewBag.Estadisticas = estadisticas;

            return View(historial);
        }

        // POST: Reportes/RegistrarAprobacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarAprobacion(long idPrestamo, string accion, string comentarios, string motivo)
        {
            var prestamo = db.Prestamo.Find(idPrestamo);
            
            if (prestamo == null)
                return Json(new { success = false, message = "Préstamo no encontrado" });

            var historial = new HistorialAprobacionPrestamo
            {
                IdPrestamo = idPrestamo,
                IdUsuarioSolicitante = prestamo.Id,
                IdUsuarioRevisor = User.Identity.GetUserId(),
                EstadoPrevio = db.Estado.Find(prestamo.IdEstado).Descripcion,
                Accion = accion,
                ComentariosRevisor = comentarios,
                MotivoRechazo = motivo,
                FechaRevision = DateTime.Now,
                NotificadoSolicitante = false
            };

            // Calcular duración de revisión si es aplicable
            var duracionMinutos = (int)(DateTime.Now - prestamo.FechadeCreacion).TotalMinutes;
            historial.DuracionRevision = duracionMinutos;

            db.HistorialAprobacionPrestamo.Add(historial);
            db.SaveChanges();

            return Json(new { success = true, message = "Aprobación registrada exitosamente" });
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
            var reporte = new ReporteGuardado
            {
                TipoReporte = tipoReporte,
                NombreReporte = nombreReporte,
                FechaGeneracion = DateTime.Now,
                GeneradoPor = User.Identity.GetUserId(),
                FiltrosUtilizados = filtros,
                Formato = formato
            };

            db.ReporteGuardado.Add(reporte);
            db.SaveChanges();
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
