using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using EscuelaFelixArcadio.Models;

namespace EscuelaFelixArcadio.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationUserManager _userManager;
        
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        
        public ActionResult Index()
        {
            // Si el usuario está autenticado, redirigir según su rol
            if (User.Identity.IsAuthenticated)
            {
                // Obtener los roles del usuario actual
                var userId = User.Identity.GetUserId();
                var roles = UserManager.GetRoles(userId);
                
                if (roles.Contains("Administrador"))
                {
                    // Administrador: Redirigir al panel de inventario (tiene acceso a todos los módulos)
                    return RedirectToAction("Index", "Inventario");
                }
                else if (roles.Contains("Docente"))
                {
                    // Docente: Redirigir al panel de inventario (tiene acceso a todos excepto Administrativo)
                    return RedirectToAction("Index", "Inventario");
                }
                else if (roles.Contains("Estudiante"))
                {
                    // Estudiante: Redirigir directamente a Deportivo (solo tiene acceso a Deportivo y Espacios)
                    return RedirectToAction("Index", "Documento");
                }
            }
            
            // Para usuarios no autenticados, mostrar la página principal
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}