using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EscuelaFelixArcadio.Models;
using System.Data.Entity;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriaController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Categoria - Lista principal de categorías
        public ActionResult Index(string search = "", string estado = "", 
            string sortBy = "nombre", string sortOrder = "asc", int page = 1, int pageSize = 12)
        {
            ViewBag.Title = "Gestión de Categorías";
            
            // Obtener datos para los filtros
            ViewBag.Estados = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            // Consulta base
            var query = db.Categoria
                .Include(c => c.Estado)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    c.Nombre.Contains(search) ||
                    c.Descripcion.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(c => c.IdEstado == estadoId);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "nombre":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(c => c.Nombre) : 
                        query.OrderByDescending(c => c.Nombre);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(c => c.Estado.Descripcion) : 
                        query.OrderByDescending(c => c.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(c => c.FechaCreacion) : 
                        query.OrderByDescending(c => c.FechaCreacion);
                    break;
                default:
                    query = query.OrderBy(c => c.Nombre);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var categorias = query.Skip(skip).Take(pageSize).ToList();

            // Pasar datos a la vista
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentEstado = estado;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            return View(categorias);
        }

        // AJAX: Buscar categorías con filtros
        [HttpGet]
        public JsonResult SearchCategoria(string search = "", string estado = "", 
            string sortBy = "nombre", string sortOrder = "asc", int page = 1, int pageSize = 12)
        {
            // Consulta base
            var query = db.Categoria
                .Include(c => c.Estado)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    c.Nombre.Contains(search) ||
                    c.Descripcion.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(c => c.IdEstado == estadoId);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "nombre":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(c => c.Nombre) : 
                        query.OrderByDescending(c => c.Nombre);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(c => c.Estado.Descripcion) : 
                        query.OrderByDescending(c => c.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(c => c.FechaCreacion) : 
                        query.OrderByDescending(c => c.FechaCreacion);
                    break;
                default:
                    query = query.OrderBy(c => c.Nombre);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var categorias = query.Skip(skip).Take(pageSize).ToList();

            var result = new
            {
                items = categorias.Select(c => new
                {
                    IdCategoria = c.IdCategoria,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion ?? "Sin descripción",
                    EstadoDescripcion = c.Estado.Descripcion,
                    IdEstado = c.IdEstado,
                    FechaCreacion = c.FechaCreacion.ToString("dd/MM/yyyy")
                }).ToList(),
                totalItems = totalItems,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Categoria/Details/5 - Ver detalles de una categoría
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Detalles de Categoría";
            
            var categoria = db.Categoria
                .Include(c => c.Estado)
                .FirstOrDefault(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return HttpNotFound();
            }

            return View(categoria);
        }

        // GET: Categoria/Create - Crear nueva categoría
        public ActionResult Create()
        {
            ViewBag.Title = "Crear Categoría";
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            return View();
        }

        // POST: Categoria/Create - Procesar creación de nueva categoría
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,IdEstado,IdCategoriaPadre")] Categoria categoria)
        {
            try
            {
                // Validación del nombre - solo letras y espacios
                if (!string.IsNullOrEmpty(categoria.Nombre))
                {
                    var nombreRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
                    if (!nombreRegex.IsMatch(categoria.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El nombre de la categoría solo puede contener letras y espacios.");
                    }
                }

                if (ModelState.IsValid)
                {
                    // Verificar si ya existe una categoría con el mismo nombre
                    var existeCategoria = db.Categoria
                        .FirstOrDefault(c => c.Nombre.ToLower() == categoria.Nombre.ToLower());

                    if (existeCategoria != null)
                    {
                        ModelState.AddModelError("", "Ya existe una categoría con este nombre.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", categoria.IdEstado);

                        return View(categoria);
                    }

                    categoria.FechaCreacion = DateTime.UtcNow;
                    db.Categoria.Add(categoria);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Categoría creada exitosamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear la categoría: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", categoria.IdEstado);

            return View(categoria);
        }

        // GET: Categoria/Edit/5 - Editar categoría
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Editar Categoría";
            
            var categoria = db.Categoria
                .Include(c => c.Estado)
                .FirstOrDefault(c => c.IdCategoria == id);
                
            if (categoria == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", categoria.IdEstado);

            return View(categoria);
        }

        // POST: Categoria/Edit/5 - Procesar edición de categoría
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCategoria,Nombre,Descripcion,IdEstado,IdCategoriaPadre,FechaCreacion")] Categoria categoria)
        {
            try
            {
                // Validación del nombre - solo letras y espacios
                if (!string.IsNullOrEmpty(categoria.Nombre))
                {
                    var nombreRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
                    if (!nombreRegex.IsMatch(categoria.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El nombre de la categoría solo puede contener letras y espacios.");
                    }
                }

                if (ModelState.IsValid)
                {
                    // Verificar si ya existe otra categoría con el mismo nombre
                    var existeCategoria = db.Categoria
                        .FirstOrDefault(c => c.Nombre.ToLower() == categoria.Nombre.ToLower() &&
                                            c.IdCategoria != categoria.IdCategoria);

                    if (existeCategoria != null)
                    {
                        ModelState.AddModelError("", "Ya existe otra categoría con este nombre.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", categoria.IdEstado);

                        return View(categoria);
                    }

                    db.Entry(categoria).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Categoría actualizada exitosamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar la categoría: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", categoria.IdEstado);

            return View(categoria);
        }

        // GET: Categoria/Delete/5 - Confirmar eliminación
        public ActionResult Delete(int id)
        {
            ViewBag.Title = "Eliminar Categoría";
            
            var categoria = db.Categoria
                .Include(c => c.Estado)
                .FirstOrDefault(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return HttpNotFound();
            }

            // Verificar si hay productos asociados a esta categoría
            var tieneProductos = db.Producto.Any(p => p.IdCategoria == id && !p.Eliminado);
            ViewBag.TieneProductos = tieneProductos;

            return View(categoria);
        }

        // POST: Categoria/Delete/5 - Procesar eliminación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var categoria = db.Categoria.Find(id);
                if (categoria != null)
                {
                    // Verificar si hay productos asociados
                    var tieneProductos = db.Producto.Any(p => p.IdCategoria == id && !p.Eliminado);
                    
                    if (tieneProductos)
                    {
                        TempData["ErrorMessage"] = "No se puede eliminar la categoría porque tiene productos asociados.";
                        return RedirectToAction("Delete", new { id = id });
                    }

                    db.Categoria.Remove(categoria);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Categoría eliminada exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró la categoría a eliminar.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la categoría: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
