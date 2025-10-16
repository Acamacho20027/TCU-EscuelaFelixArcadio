using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SancionesController : Controller
    {
        // GET: Sanciones
        public ActionResult Index()
        {
            ViewBag.Title = "Gestión de Sanciones";
            return View();
        }

        // GET: Sanciones/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Sanciones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Sanciones/Create
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

        // GET: Sanciones/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Sanciones/Edit/5
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

        // GET: Sanciones/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Sanciones/Delete/5
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
