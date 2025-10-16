using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DeportivoController : Controller
    {
        // GET: Deportivo
        public ActionResult Index()
        {
            ViewBag.Title = "Gestión Deportiva";
            return View();
        }

        // GET: Deportivo/Deportes
        public ActionResult Deportes()
        {
            ViewBag.Title = "Gestión de Deportes";
            return View();
        }

        // GET: Deportivo/Equipos
        public ActionResult Equipos()
        {
            ViewBag.Title = "Gestión de Equipos";
            return View();
        }

        // GET: Deportivo/Eventos
        public ActionResult Eventos()
        {
            ViewBag.Title = "Gestión de Eventos";
            return View();
        }

        // GET: Deportivo/Horarios
        public ActionResult Horarios()
        {
            ViewBag.Title = "Gestión de Horarios";
            return View();
        }
    }
}
