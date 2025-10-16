using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministrativoController : Controller
    {
        // GET: Administrativo
        public ActionResult Index()
        {
            ViewBag.Title = "Gestión Administrativa";
            return View();
        }

        // GET: Administrativo/Usuarios
        public ActionResult Usuarios()
        {
            ViewBag.Title = "Gestión de Usuarios";
            return View();
        }

        // GET: Administrativo/Roles
        public ActionResult Roles()
        {
            ViewBag.Title = "Gestión de Roles";
            return View();
        }

        // GET: Administrativo/Configuracion
        public ActionResult Configuracion()
        {
            ViewBag.Title = "Configuración del Sistema";
            return View();
        }
    }
}
