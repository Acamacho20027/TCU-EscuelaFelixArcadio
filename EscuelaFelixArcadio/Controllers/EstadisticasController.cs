using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EscuelaFelixArcadio.Models;
using EscuelaFelixArcadio.Services;
using Microsoft.AspNet.Identity;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EstadisticasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private EstadisticasService _estadisticasService;

        public EstadisticasController()
        {
            _estadisticasService = new EstadisticasService(db);
        }

        #region Vistas Principales

        // GET: Estadisticas
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard Estadístico Deportivo";
            return View();
        }

        // GET: Estadisticas/Inventario
        public ActionResult Inventario()
        {
            ViewBag.Title = "Estadísticas de Inventario";
            return View();
        }

        // GET: Estadisticas/Usuarios
        public ActionResult Usuarios()
        {
            ViewBag.Title = "Estadísticas de Usuarios";
            return View();
        }

        // GET: Estadisticas/Actividad
        public ActionResult Actividad()
        {
            ViewBag.Title = "Estadísticas de Actividad";
            return View();
        }

        // GET: Estadisticas/Prestamos
        public ActionResult Prestamos()
        {
            ViewBag.Title = "Análisis de Préstamos Deportivos";
            return View();
        }

        // GET: Estadisticas/Espacios
        public ActionResult Espacios()
        {
            ViewBag.Title = "Análisis de Reservaciones de Espacios";
            return View();
        }

        // GET: Estadisticas/Recomendaciones
        public ActionResult Recomendaciones()
        {
            ViewBag.Title = "Recomendaciones Inteligentes";
            return View();
        }

        // GET: Estadisticas/Comparativo
        public ActionResult Comparativo()
        {
            ViewBag.Title = "Análisis Comparativo";
            return View();
        }

        // GET: Estadisticas/Alertas
        public ActionResult Alertas()
        {
            ViewBag.Title = "Alertas Estadísticas";
            return View();
        }

        #endregion

        #region Dashboard Principal - Indicadores Clave

        // GET: Estadisticas/DashboardData
        public ActionResult DashboardData()
        {
            try
            {
                var dashboardData = _estadisticasService.ObtenerDatosDashboard();
                return Json(new { success = true, data = dashboardData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/KPIs
        public ActionResult KPIs()
        {
            try
            {
                var kpis = _estadisticasService.ObtenerKPIs();
                return Json(new { success = true, data = kpis }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Análisis de Tendencias de Préstamos

        // GET: Estadisticas/TendenciasPrestamos
        public ActionResult TendenciasPrestamos(DateTime? fechaInicio, DateTime? fechaFin, string tipoAnalisis = "diario")
        {
            try
            {
                var tendencias = _estadisticasService.ObtenerTendenciasPrestamos(fechaInicio, fechaFin, tipoAnalisis);
                return Json(new { success = true, data = tendencias }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/PatronesPrestamos
        public ActionResult PatronesPrestamos(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var patrones = _estadisticasService.ObtenerPatronesPrestamos(fechaInicio, fechaFin);
                return Json(new { success = true, data = patrones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/PrediccionesPrestamos
        public ActionResult PrediccionesPrestamos(int diasPrediccion = 30)
        {
            try
            {
                var predicciones = _estadisticasService.GenerarPrediccionesPrestamos(diasPrediccion);
                return Json(new { success = true, data = predicciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Análisis de Reservaciones de Espacios

        // GET: Estadisticas/TendenciasEspacios
        public ActionResult TendenciasEspacios(DateTime? fechaInicio, DateTime? fechaFin, int? idEspacio)
        {
            try
            {
                var tendencias = _estadisticasService.ObtenerTendenciasEspacios(fechaInicio, fechaFin, idEspacio);
                return Json(new { success = true, data = tendencias }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/OcupacionEspacios
        public ActionResult OcupacionEspacios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var ocupacion = _estadisticasService.ObtenerOcupacionEspacios(fechaInicio, fechaFin);
                return Json(new { success = true, data = ocupacion }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/HeatmapEspacios
        public ActionResult HeatmapEspacios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var heatmap = _estadisticasService.ObtenerHeatmapEspacios(fechaInicio, fechaFin);
                return Json(new { success = true, data = heatmap }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/EficienciaEspacios
        public ActionResult EficienciaEspacios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var eficiencia = _estadisticasService.ObtenerEficienciaEspacios(fechaInicio, fechaFin);
                return Json(new { success = true, data = eficiencia }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Sistema de Recomendaciones

        // GET: Estadisticas/RecomendacionesHorarios
        public ActionResult RecomendacionesHorarios(int? idEspacio)
        {
            try
            {
                var recomendaciones = _estadisticasService.GenerarRecomendacionesHorarios(idEspacio);
                return Json(new { success = true, data = recomendaciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/RecomendacionesEquipamiento
        public ActionResult RecomendacionesEquipamiento()
        {
            try
            {
                var recomendaciones = _estadisticasService.GenerarRecomendacionesEquipamiento();
                return Json(new { success = true, data = recomendaciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/RecomendacionesMantenimiento
        public ActionResult RecomendacionesMantenimiento()
        {
            try
            {
                var recomendaciones = _estadisticasService.GenerarRecomendacionesMantenimiento();
                return Json(new { success = true, data = recomendaciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Visualizaciones Interactivas

        // GET: Estadisticas/DatosGraficoPrestamos
        public ActionResult DatosGraficoPrestamos(DateTime? fechaInicio, DateTime? fechaFin, string tipoGrafico = "linea")
        {
            try
            {
                var datos = _estadisticasService.ObtenerDatosGraficoPrestamos(fechaInicio, fechaFin, tipoGrafico);
                return Json(new { success = true, data = datos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/DatosGraficoEspacios
        public ActionResult DatosGraficoEspacios(DateTime? fechaInicio, DateTime? fechaFin, string tipoGrafico = "barras")
        {
            try
            {
                var datos = _estadisticasService.ObtenerDatosGraficoEspacios(fechaInicio, fechaFin, tipoGrafico);
                return Json(new { success = true, data = datos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/DatosGraficoUsuarios
        public ActionResult DatosGraficoUsuarios(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var datos = _estadisticasService.ObtenerDatosGraficoUsuarios(fechaInicio, fechaFin);
                return Json(new { success = true, data = datos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/DatosGraficoInventario
        public ActionResult DatosGraficoInventario()
        {
            try
            {
                var datos = _estadisticasService.ObtenerDatosGraficoInventario();
                return Json(new { success = true, data = datos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Análisis Comparativo

        // GET: Estadisticas/CompararPeriodos
        public ActionResult CompararPeriodos(string tipoAnalisis, DateTime periodo1Inicio, DateTime periodo1Fin, DateTime periodo2Inicio, DateTime periodo2Fin)
        {
            try
            {
                var comparacion = _estadisticasService.CompararPeriodos(tipoAnalisis, periodo1Inicio, periodo1Fin, periodo2Inicio, periodo2Fin);
                return Json(new { success = true, data = comparacion }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/VariacionesTemporales
        public ActionResult VariacionesTemporales(string tipoAnalisis, string periodo = "mensual")
        {
            try
            {
                var variaciones = _estadisticasService.ObtenerVariacionesTemporales(tipoAnalisis, periodo);
                return Json(new { success = true, data = variaciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Sistema de Alertas Estadísticas

        // GET: Estadisticas/AlertasEstadisticas
        public ActionResult AlertasEstadisticas()
        {
            try
            {
                var alertas = _estadisticasService.ObtenerAlertasEstadisticas();
                return Json(new { success = true, data = alertas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/Anomalias
        public ActionResult Anomalias(string tipoAnalisis = "prestamos")
        {
            try
            {
                var anomalias = _estadisticasService.DetectarAnomalias(tipoAnalisis);
                return Json(new { success = true, data = anomalias }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/PatronesInusuales
        public ActionResult PatronesInusuales(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var patrones = _estadisticasService.DetectarPatronesInusuales(fechaInicio, fechaFin);
                return Json(new { success = true, data = patrones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Estadisticas/ConfigurarAlertas
        [HttpPost]
        public ActionResult ConfigurarAlertas(string tipoAlerta, decimal umbral, bool activo)
        {
            try
            {
                var resultado = _estadisticasService.ConfigurarAlerta(tipoAlerta, umbral, activo);
                return Json(new { success = true, data = resultado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Métodos de Filtrado y Búsqueda

        // GET: Estadisticas/FiltrarDatos
        public ActionResult FiltrarDatos(string tipoDatos, DateTime? fechaInicio, DateTime? fechaFin, string filtros)
        {
            try
            {
                var datosFiltrados = _estadisticasService.FiltrarDatos(tipoDatos, fechaInicio, fechaFin, filtros);
                return Json(new { success = true, data = datosFiltrados }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/ExportarDatos
        public ActionResult ExportarDatos(string tipoDatos, DateTime? fechaInicio, DateTime? fechaFin, string formato = "excel")
        {
            try
            {
                var archivo = _estadisticasService.ExportarDatos(tipoDatos, fechaInicio, fechaFin, formato);
                return File(archivo.Contenido, archivo.TipoMIME, archivo.NombreArchivo);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Métodos de Utilidad

        // GET: Estadisticas/ObtenerFiltros
        public ActionResult ObtenerFiltros(string tipoDatos)
        {
            try
            {
                var filtros = _estadisticasService.ObtenerFiltrosDisponibles(tipoDatos);
                return Json(new { success = true, data = filtros }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Estadisticas/ValidarRangoFechas
        public ActionResult ValidarRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var validacion = _estadisticasService.ValidarRangoFechas(fechaInicio, fechaFin);
                return Json(new { success = true, data = validacion }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
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
