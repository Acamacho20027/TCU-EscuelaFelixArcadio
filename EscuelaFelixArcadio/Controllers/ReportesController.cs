using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        // GET: Reportes
        public ActionResult Index()
        {
            ViewBag.Title = "Reportes y Estadísticas";
            return View();
        }

        // GET: Reportes/Inventario
        public ActionResult Inventario()
        {
            ViewBag.Title = "Reportes de Inventario";
            return View();
        }

        // GET: Reportes/Prestamos
        public ActionResult Prestamos()
        {
            ViewBag.Title = "Reportes de Préstamos";
            return View();
        }

        // GET: Reportes/Sanciones
        public ActionResult Sanciones()
        {
            ViewBag.Title = "Reportes de Sanciones";
            return View();
        }
    }
}
