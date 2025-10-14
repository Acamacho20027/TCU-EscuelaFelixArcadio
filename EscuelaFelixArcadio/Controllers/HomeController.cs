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