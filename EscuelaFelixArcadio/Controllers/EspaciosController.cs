using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EspaciosController : Controller
    {
        // GET: Espacios
        public ActionResult Index()
        {
            ViewBag.Title = "Gestión de Espacios";
            return View();
        }

        // GET: Espacios/Instalaciones
        public ActionResult Instalaciones()
        {
            ViewBag.Title = "Gestión de Instalaciones";
            return View();
        }

        // GET: Espacios/Reservas
        public ActionResult Reservas()
        {
            ViewBag.Title = "Gestión de Reservas";
            return View();
        }

        // GET: Espacios/Mantenimiento
        public ActionResult Mantenimiento()
        {
            ViewBag.Title = "Gestión de Mantenimiento";
            return View();
        }
    }
}
