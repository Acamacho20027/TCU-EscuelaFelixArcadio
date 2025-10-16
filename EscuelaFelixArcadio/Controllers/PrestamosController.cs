using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PrestamosController : Controller
    {
        // GET: Prestamos
        public ActionResult Index()
        {
            ViewBag.Title = "Gestión de Préstamos";
            return View();
        }

        // GET: Prestamos/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Prestamos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Prestamos/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Prestamos/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Prestamos/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Prestamos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Prestamos/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
