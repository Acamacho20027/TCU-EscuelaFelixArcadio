using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EscuelaFelixArcadio.Models;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class InventarioController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Inventario - Lista principal del inventario
        public ActionResult Index(string search = "", string categoria = "", string estado = "", string marca = "", 
            string sortBy = "nombre", string sortOrder = "asc", int page = 1, int pageSize = 12)
        {
            ViewBag.Title = "Inventario";
            
            // Obtener datos para los filtros
            ViewBag.Categorias = new SelectList(
                db.Categoria.Where(c => c.IdEstado == 1).OrderBy(c => c.Nombre), 
                "IdCategoria", "Nombre");
            
            ViewBag.Estados = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");
            
            var marcas = db.Producto
                .Where(p => !p.Eliminado && !string.IsNullOrEmpty(p.Marca))
                .Select(p => p.Marca)
                .Distinct()
                .OrderBy(m => m)
                .ToList();
            
            ViewBag.Marcas = new SelectList(marcas);

            // Consulta base
            var query = db.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .Include(i => i.Variante)
                .Where(i => !i.Producto.Eliminado);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => 
                    i.Producto.Nombre.Contains(search) ||
                    i.Producto.Codigo.Contains(search) ||
                    i.Producto.Descripcion.Contains(search));
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                int categoriaId = int.Parse(categoria);
                query = query.Where(i => i.Producto.IdCategoria == categoriaId);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(i => i.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(marca))
            {
                query = query.Where(i => i.Producto.Marca == marca);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "nombre":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Producto.Nombre) : 
                        query.OrderByDescending(i => i.Producto.Nombre);
                    break;
                case "cantidad":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Cantidad) : 
                        query.OrderByDescending(i => i.Cantidad);
                    break;
                case "categoria":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Producto.Categoria.Nombre) : 
                        query.OrderByDescending(i => i.Producto.Categoria.Nombre);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Estado.Descripcion) : 
                        query.OrderByDescending(i => i.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.FechaActualizacion) : 
                        query.OrderByDescending(i => i.FechaActualizacion);
                    break;
                default:
                    query = query.OrderBy(i => i.Producto.Nombre);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var inventario = query.Skip(skip).Take(pageSize).ToList();

            // Pasar datos a la vista
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoria = categoria;
            ViewBag.CurrentEstado = estado;
            ViewBag.CurrentMarca = marca;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            return View(inventario);
        }

        // AJAX: Buscar inventario con filtros
        [HttpGet]
        public JsonResult SearchInventario(string search = "", string categoria = "", string estado = "", string marca = "", 
            string sortBy = "nombre", string sortOrder = "asc", int page = 1, int pageSize = 12)
        {
            // Consulta base
            var query = db.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .Include(i => i.Variante)
                .Where(i => !i.Producto.Eliminado);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => 
                    i.Producto.Nombre.Contains(search) ||
                    i.Producto.Codigo.Contains(search) ||
                    i.Producto.Descripcion.Contains(search));
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                int categoriaId = int.Parse(categoria);
                query = query.Where(i => i.Producto.IdCategoria == categoriaId);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(i => i.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(marca))
            {
                query = query.Where(i => i.Producto.Marca == marca);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "nombre":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Producto.Nombre) : 
                        query.OrderByDescending(i => i.Producto.Nombre);
                    break;
                case "cantidad":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Cantidad) : 
                        query.OrderByDescending(i => i.Cantidad);
                    break;
                case "categoria":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Producto.Categoria.Nombre) : 
                        query.OrderByDescending(i => i.Producto.Categoria.Nombre);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.Estado.Descripcion) : 
                        query.OrderByDescending(i => i.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(i => i.FechaActualizacion) : 
                        query.OrderByDescending(i => i.FechaActualizacion);
                    break;
                default:
                    query = query.OrderBy(i => i.Producto.Nombre);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var inventario = query.Skip(skip).Take(pageSize).ToList();

            var result = new
            {
                items = inventario.Select(i => new
                {
                    IdInventario = i.IdInventario,
                    ProductoNombre = i.Producto.Nombre,
                    ProductoCodigo = i.Producto.Codigo,
                    CategoriaNombre = i.Producto.Categoria.Nombre,
                    EstadoDescripcion = i.Estado.Descripcion,
                    VarianteNombre = i.Variante != null ? i.Variante.NombreVariante : "Sin variante",
                    Cantidad = i.Cantidad,
                    Minimo = i.Minimo,
                    Maximo = i.Maximo,
                    Marca = i.Producto.Marca,
                    FechaActualizacion = i.FechaActualizacion.ToString("dd/MM/yyyy")
                }).ToList(),
                totalItems = totalItems,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Método de prueba completamente simple
        [HttpGet]
        public ActionResult TestSimple()
        {
            return Content("Funciona correctamente", "text/plain");
        }

        // GET: Obtener números de serie de un producto - Versión simple
        [HttpGet]
        public JsonResult GetNumerosSerie(int idProducto, int? idVariante = null)
        {
            try
            {
                // Log de parámetros para debugging
                System.Diagnostics.Debug.WriteLine($"GetNumerosSerie llamado con: idProducto={idProducto}, idVariante={idVariante}");
                
                // Validar parámetros
                if (idProducto <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Error: idProducto inválido: {idProducto}");
                    return Json(new { error = "ID de producto inválido" }, JsonRequestBehavior.AllowGet);
                }

                // Retornar datos estáticos por ahora para probar
                var datosEstaticos = new[]
                {
                    new
                    {
                        IdSerie = 1,
                        NumeroSerie = "SERIE-001",
                        EstadoDescripcion = "Activo",
                        Ubicacion = "Almacen Principal",
                        FechaCreacion = "16/10/2025"
                    },
                    new
                    {
                        IdSerie = 2,
                        NumeroSerie = "SERIE-002",
                        EstadoDescripcion = "Activo",
                        Ubicacion = "Almacen Principal",
                        FechaCreacion = "16/10/2025"
                    }
                };

                System.Diagnostics.Debug.WriteLine($"Retornando {datosEstaticos.Length} números de serie");
                return Json(datosEstaticos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetNumerosSerie: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { error = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Agregar número de serie
        [HttpPost]
        public JsonResult AgregarNumeroSerie(int idProducto, int? idVariante, string numeroSerie, string ubicacion, int idEstado)
        {
            try
            {
                var nuevoNumeroSerie = new NumeroSerieProducto
                {
                    IdProducto = idProducto,
                    IdVariante = idVariante,
                    NumeroSerie = numeroSerie,
                    Ubicacion = ubicacion,
                    IdEstado = idEstado,
                    FechaCreacion = DateTime.UtcNow
                };

                db.NumeroSerieProducto.Add(nuevoNumeroSerie);
                db.SaveChanges();

                return Json(new { success = true, message = "Número de serie agregado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al agregar número de serie: " + ex.Message });
            }
        }

        // POST: Actualizar número de serie
        [HttpPost]
        public JsonResult ActualizarNumeroSerie(int idSerie, string numeroSerie, string ubicacion, int idEstado)
        {
            try
            {
                var numeroSerieObj = db.NumeroSerieProducto.Find(idSerie);
                if (numeroSerieObj == null)
                {
                    return Json(new { success = false, message = "Número de serie no encontrado" });
                }

                numeroSerieObj.NumeroSerie = numeroSerie;
                numeroSerieObj.Ubicacion = ubicacion;
                numeroSerieObj.IdEstado = idEstado;

                db.SaveChanges();

                return Json(new { success = true, message = "Número de serie actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar número de serie: " + ex.Message });
            }
        }

        // POST: Eliminar número de serie
        [HttpPost]
        public JsonResult EliminarNumeroSerie(int idSerie)
        {
            try
            {
                var numeroSerieObj = db.NumeroSerieProducto.Find(idSerie);
                if (numeroSerieObj == null)
                {
                    return Json(new { success = false, message = "Número de serie no encontrado" });
                }

                db.NumeroSerieProducto.Remove(numeroSerieObj);
                db.SaveChanges();

                return Json(new { success = true, message = "Número de serie eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar número de serie: " + ex.Message });
            }
        }

        // GET: Inventario/Details/5 - Ver detalles de un item del inventario
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Detalles Inventario";
            
            var inventario = db.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .Include(i => i.Variante)
                .FirstOrDefault(i => i.IdInventario == id);

            if (inventario == null)
            {
                return HttpNotFound();
            }

            return View(inventario);
        }

        // GET: Inventario/Create - Crear nuevo item en inventario
        public ActionResult Create()
        {
            ViewBag.Title = "Agregar Inventario";
            
            ViewBag.IdProducto = new SelectList(
                db.Producto.Where(p => !p.Eliminado).OrderBy(p => p.Nombre), 
                "IdProducto", "Nombre");
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");
            
            ViewBag.IdVariante = new SelectList(
                db.VarianteProducto.OrderBy(v => v.NombreVariante), 
                "IdVariante", "NombreVariante");

            return View();
        }

        // POST: Inventario/Create - Procesar creación de nuevo item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdProducto,IdEstado,IdVariante,Cantidad,Minimo,Maximo")] Inventario inventario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si ya existe inventario para este producto y variante
                    var existeInventario = db.Inventario
                        .FirstOrDefault(i => i.IdProducto == inventario.IdProducto && 
                                            i.IdVariante == inventario.IdVariante);

                    if (existeInventario != null)
                    {
                        ModelState.AddModelError("", "Ya existe inventario para este producto y variante.");
                        
                        ViewBag.IdProducto = new SelectList(
                            db.Producto.Where(p => !p.Eliminado).OrderBy(p => p.Nombre), 
                            "IdProducto", "Nombre", inventario.IdProducto);
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", inventario.IdEstado);
                        
                        ViewBag.IdVariante = new SelectList(
                            db.VarianteProducto.OrderBy(v => v.NombreVariante), 
                            "IdVariante", "NombreVariante", inventario.IdVariante);

                        return View(inventario);
                    }

                    inventario.FechaActualizacion = DateTime.UtcNow;
                    db.Inventario.Add(inventario);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Item agregado al inventario exitosamente.";
                return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el item: " + ex.Message);
            }

            ViewBag.IdProducto = new SelectList(
                db.Producto.Where(p => !p.Eliminado).OrderBy(p => p.Nombre), 
                "IdProducto", "Nombre", inventario.IdProducto);
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", inventario.IdEstado);
            
            ViewBag.IdVariante = new SelectList(
                db.VarianteProducto.OrderBy(v => v.NombreVariante), 
                "IdVariante", "NombreVariante", inventario.IdVariante);

            return View(inventario);
        }

        // GET: Inventario/Edit/5 - Editar item del inventario
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Editar Inventario";
            
            var inventario = db.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .Include(i => i.Variante)
                .FirstOrDefault(i => i.IdInventario == id);
                
            if (inventario == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdProducto = new SelectList(
                db.Producto.Where(p => !p.Eliminado).OrderBy(p => p.Nombre), 
                "IdProducto", "Nombre", inventario.IdProducto);
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", inventario.IdEstado);
            
            ViewBag.IdVariante = new SelectList(
                db.VarianteProducto.OrderBy(v => v.NombreVariante), 
                "IdVariante", "NombreVariante", inventario.IdVariante);

            return View(inventario);
        }

        // POST: Inventario/Edit/5 - Procesar edición de item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdInventario,IdProducto,IdEstado,IdVariante,Cantidad,Minimo,Maximo")] Inventario inventario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar si ya existe otro inventario para este producto y variante
                    var existeInventario = db.Inventario
                        .FirstOrDefault(i => i.IdProducto == inventario.IdProducto && 
                                            i.IdVariante == inventario.IdVariante &&
                                            i.IdInventario != inventario.IdInventario);

                    if (existeInventario != null)
                    {
                        ModelState.AddModelError("", "Ya existe inventario para este producto y variante.");
                        
                        ViewBag.IdProducto = new SelectList(
                            db.Producto.Where(p => !p.Eliminado).OrderBy(p => p.Nombre), 
                            "IdProducto", "Nombre", inventario.IdProducto);
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", inventario.IdEstado);
                        
                        ViewBag.IdVariante = new SelectList(
                            db.VarianteProducto.OrderBy(v => v.NombreVariante), 
                            "IdVariante", "NombreVariante", inventario.IdVariante);

                        return View(inventario);
                    }

                    inventario.FechaActualizacion = DateTime.UtcNow;
                    db.Entry(inventario).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Item del inventario actualizado exitosamente.";
                return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar el item: " + ex.Message);
            }

            ViewBag.IdProducto = new SelectList(
                db.Producto.Where(p => !p.Eliminado).OrderBy(p => p.Nombre), 
                "IdProducto", "Nombre", inventario.IdProducto);
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", inventario.IdEstado);
            
            ViewBag.IdVariante = new SelectList(
                db.VarianteProducto.OrderBy(v => v.NombreVariante), 
                "IdVariante", "NombreVariante", inventario.IdVariante);

            return View(inventario);
        }

        // GET: Inventario/Delete/5 - Confirmar eliminación
        public ActionResult Delete(int id)
        {
            ViewBag.Title = "Eliminar Inventario";
            
            var inventario = db.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .Include(i => i.Variante)
                .FirstOrDefault(i => i.IdInventario == id);

            if (inventario == null)
            {
                return HttpNotFound();
            }

            return View(inventario);
        }

        // POST: Inventario/Delete/5 - Procesar eliminación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var inventario = db.Inventario.Find(id);
                if (inventario != null)
                {
                    db.Inventario.Remove(inventario);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Item eliminado del inventario exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró el item a eliminar.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el item: " + ex.Message;
            }

                return RedirectToAction("Index");
        }

        // Método para obtener variantes de un producto (AJAX)
        [HttpGet]
        public JsonResult GetVariantesByProducto(int idProducto)
        {
            var variantes = db.VarianteProducto
                .Where(v => v.IdProducto == idProducto)
                .Select(v => new { v.IdVariante, v.NombreVariante })
                .OrderBy(v => v.NombreVariante)
                .ToList();

            return Json(variantes, JsonRequestBehavior.AllowGet);
        }

        // Método para obtener información del producto (AJAX)
        [HttpGet]
        public JsonResult GetProductoInfo(int idProducto)
        {
            var producto = db.Producto
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.IdProducto == idProducto);

            if (producto != null)
            {
                var info = new
                {
                    Nombre = producto.Nombre,
                    Codigo = producto.Codigo,
                    Descripcion = producto.Descripcion,
                    Marca = producto.Marca,
                    Categoria = producto.Categoria?.Nombre ?? "Sin categoria",
                    EsServicio = producto.EsServicio
                };

                return Json(info, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        // Método para verificar stock bajo
        [HttpGet]
        public ActionResult StockBajo()
        {
            ViewBag.Title = "Stock Bajo";
            
            var stockBajo = db.Inventario
                .Include(i => i.Producto)
                .Include(i => i.Producto.Categoria)
                .Include(i => i.Estado)
                .Include(i => i.Variante)
                .Where(i => i.Cantidad <= i.Minimo && !i.Producto.Eliminado)
                .OrderBy(i => i.Cantidad)
                .ToList();

            return View(stockBajo);
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
