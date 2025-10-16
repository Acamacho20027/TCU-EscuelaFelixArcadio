using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EstadisticasController : Controller
    {
        // GET: Estadisticas
        public ActionResult Index()
        {
            ViewBag.Title = "Estadísticas Generales";
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
    }
}
