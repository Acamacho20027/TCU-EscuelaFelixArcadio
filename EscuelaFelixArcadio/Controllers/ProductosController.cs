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
    public class ProductosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Productos
        public ActionResult Index(string search = "", string categoria = "", string estado = "", 
            string sortBy = "nombre", string sortOrder = "asc", int page = 1, int pageSize = 12)
        {
            ViewBag.Title = "Productos";
            
            // Obtener datos para los filtros
            ViewBag.Categorias = new SelectList(
                db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                "IdCategoria", "Nombre");
            
            ViewBag.Estados = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            // Construir la consulta base
            var query = db.Producto
                .Include(p => p.Categoria)
                .Include(p => p.Estado)
                .Where(p => p.Eliminado == false);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Nombre.Contains(search) || 
                                       p.Codigo.Contains(search) || 
                                       p.Descripcion.Contains(search) ||
                                       p.Marca.Contains(search));
            }

            if (!string.IsNullOrEmpty(categoria) && categoria != "0")
            {
                int categoriaId = int.Parse(categoria);
                query = query.Where(p => p.IdCategoria == categoriaId);
            }

            if (!string.IsNullOrEmpty(estado) && estado != "0")
            {
                int estadoId = int.Parse(estado);
                query = query.Where(p => p.IdEstado == estadoId);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "codigo":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Codigo) : query.OrderByDescending(p => p.Codigo);
                    break;
                case "categoria":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Categoria.Nombre) : query.OrderByDescending(p => p.Categoria.Nombre);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Estado.Descripcion) : query.OrderByDescending(p => p.Estado.Descripcion);
                    break;
                case "marca":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Marca) : query.OrderByDescending(p => p.Marca);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.FechaCreacion) : query.OrderByDescending(p => p.FechaCreacion);
                    break;
                default:
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Nombre) : query.OrderByDescending(p => p.Nombre);
                    break;
            }

            // Aplicar paginación
            var totalItems = query.Count();
            var productos = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Search = search;
            ViewBag.Categoria = categoria;
            ViewBag.Estado = estado;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(productos);
        }

        // GET: Productos/Details/5
        public ActionResult Details(int id)
        {
            var producto = db.Producto
                .Include(p => p.Categoria)
                .Include(p => p.Estado)
                .FirstOrDefault(p => p.IdProducto == id && p.Eliminado == false);

            if (producto == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Detalles del Producto";
            return View(producto);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Crear Producto";
            ViewBag.IdCategoria = new SelectList(
                db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                "IdCategoria", "Nombre");
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");
            
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Codigo,Nombre,Descripcion,Marca,EsServicio,IdEstado,IdCategoria")] Producto producto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Generar código automático si está vacío
                    if (string.IsNullOrWhiteSpace(producto.Codigo))
                    {
                        producto.Codigo = GenerateProductCode();
                    }

                    // Verificar que el código no exista
                    if (db.Producto.Any(p => p.Codigo == producto.Codigo && p.Eliminado == false))
                    {
                        ModelState.AddModelError("Codigo", "Ya existe un producto con este código.");
                        ViewBag.IdCategoria = new SelectList(
                            db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                            "IdCategoria", "Nombre", producto.IdCategoria);
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", producto.IdEstado);
                        return View(producto);
                    }

                    producto.FechaCreacion = DateTime.UtcNow;
                    producto.Eliminado = false;
                    
                    db.Producto.Add(producto);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Producto creado exitosamente con código: " + producto.Codigo;
                    return RedirectToAction("Index");
                }

                ViewBag.IdCategoria = new SelectList(
                    db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                    "IdCategoria", "Nombre", producto.IdCategoria);
                ViewBag.IdEstado = new SelectList(
                    db.Estado.OrderBy(e => e.Descripcion), 
                    "IdEstado", "Descripcion", producto.IdEstado);
                
                return View(producto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear el producto: " + ex.Message;
                ViewBag.IdCategoria = new SelectList(
                    db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                    "IdCategoria", "Nombre", producto.IdCategoria);
                ViewBag.IdEstado = new SelectList(
                    db.Estado.OrderBy(e => e.Descripcion), 
                    "IdEstado", "Descripcion", producto.IdEstado);
                return View(producto);
            }
        }

        // GET: Productos/Edit/5
        public ActionResult Edit(int id)
        {
            var producto = db.Producto
                .Include(p => p.Categoria)
                .Include(p => p.Estado)
                .FirstOrDefault(p => p.IdProducto == id && p.Eliminado == false);

            if (producto == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Editar Producto";
            ViewBag.IdCategoria = new SelectList(
                db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                "IdCategoria", "Nombre", producto.IdCategoria);
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", producto.IdEstado);
            
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdProducto,Codigo,Nombre,Descripcion,Marca,EsServicio,IdEstado,IdCategoria,FechaCreacion")] Producto producto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar que el código no exista en otro producto
                    if (db.Producto.Any(p => p.Codigo == producto.Codigo && p.IdProducto != producto.IdProducto && p.Eliminado == false))
                    {
                        ModelState.AddModelError("Codigo", "Ya existe un producto con este código.");
                        ViewBag.IdCategoria = new SelectList(
                            db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                            "IdCategoria", "Nombre", producto.IdCategoria);
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", producto.IdEstado);
                        return View(producto);
                    }

                    db.Entry(producto).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                    return RedirectToAction("Index");
                }

                ViewBag.IdCategoria = new SelectList(
                    db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                    "IdCategoria", "Nombre", producto.IdCategoria);
                ViewBag.IdEstado = new SelectList(
                    db.Estado.OrderBy(e => e.Descripcion), 
                    "IdEstado", "Descripcion", producto.IdEstado);
                
                return View(producto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el producto: " + ex.Message;
                ViewBag.IdCategoria = new SelectList(
                    db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                    "IdCategoria", "Nombre", producto.IdCategoria);
                ViewBag.IdEstado = new SelectList(
                    db.Estado.OrderBy(e => e.Descripcion), 
                    "IdEstado", "Descripcion", producto.IdEstado);
                return View(producto);
            }
        }

        // GET: Productos/Delete/5
        public ActionResult Delete(int id)
        {
            var producto = db.Producto
                .Include(p => p.Categoria)
                .Include(p => p.Estado)
                .FirstOrDefault(p => p.IdProducto == id && p.Eliminado == false);

            if (producto == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Eliminar Producto";
            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var producto = db.Producto.Find(id);
                if (producto != null)
                {
                    // Soft delete - marcar como eliminado en lugar de eliminar físicamente
                    producto.Eliminado = true;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Producto eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el producto: " + ex.Message;
            }
            
            return RedirectToAction("Index");
        }

        // Método para generar código automático
        private string GenerateProductCode()
        {
            string codigo;
            int intentos = 0;
            const int maxIntentos = 10;

            do
            {
                // Generar código con 3 letras + 4 números
                var random = new Random();
                var letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var codigoLetras = new string(Enumerable.Repeat(letras, 3)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                var codigoNumeros = random.Next(1000, 9999).ToString();
                codigo = codigoLetras + codigoNumeros;
                
                intentos++;
            }
            while (db.Producto.Any(p => p.Codigo == codigo && p.Eliminado == false) && intentos < maxIntentos);

            // Si después de varios intentos no se encuentra un código único, usar timestamp
            if (intentos >= maxIntentos)
            {
                var timestamp = DateTime.Now.Ticks.ToString().Substring(10, 7);
                var letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var random = new Random();
                var codigoLetras = new string(Enumerable.Repeat(letras, 2)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                codigo = codigoLetras + timestamp;
            }

            return codigo;
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
