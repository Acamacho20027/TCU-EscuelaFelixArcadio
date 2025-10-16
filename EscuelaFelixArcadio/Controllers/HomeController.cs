using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Si el usuario está autenticado y es administrador, mostrar el dashboard de administración
            if (User.Identity.IsAuthenticated && User.IsInRole("Administrador"))
            {
                ViewBag.Title = "Panel de Administración";
                return View("AdminPanel");
            }
            
            // Para usuarios no autenticados o no administradores, mostrar la página principal
            ViewBag.Message = "Bienvenido a la Escuela Félix Arcadio - Gestión Deportiva";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Conoce nuestros deportes y programas deportivos.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contáctanos para más información sobre nuestros servicios deportivos.";
            return View();
        }

    }
}