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
    public class PrestamosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Prestamos - Lista principal de préstamos
        public ActionResult Index(string search = "", string estado = "", string usuario = "", 
            string sortBy = "fecha", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            ViewBag.Title = "Prestamos";
            
            // Obtener datos para los filtros
            ViewBag.Estados = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            // Obtener usuarios para filtro
            var usuarios = db.Users
                .Select(u => new { Id = u.Id, Nombre = u.UserName })
                .OrderBy(u => u.Nombre)
                .ToList();
            ViewBag.Usuarios = new SelectList(usuarios, "Id", "Nombre");

            // Consulta base
            var query = db.Prestamo
                .Include(p => p.Estado)
                .Include(p => p.ApplicationUser)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => 
                    p.NumeroPrestamo.Contains(search) ||
                    p.Notas.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(p => p.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(usuario))
            {
                query = query.Where(p => p.Id == usuario);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "numero":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.NumeroPrestamo) : 
                        query.OrderByDescending(p => p.NumeroPrestamo);
                    break;
                case "usuario":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.ApplicationUser.UserName) : 
                        query.OrderByDescending(p => p.ApplicationUser.UserName);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.Estado.Descripcion) : 
                        query.OrderByDescending(p => p.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.FechadeCreacion) : 
                        query.OrderByDescending(p => p.FechadeCreacion);
                    break;
                default:
                    query = query.OrderByDescending(p => p.FechadeCreacion);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var prestamos = query.Skip(skip).Take(pageSize).ToList();

            // Pasar datos a la vista
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentEstado = estado;
            ViewBag.CurrentUsuario = usuario;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            return View(prestamos);
        }

        // AJAX: Buscar préstamos con filtros
        [HttpGet]
        public JsonResult SearchPrestamos(string search = "", string estado = "", string usuario = "", 
            string sortBy = "fecha", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            // Consulta base
            var query = db.Prestamo
                .Include(p => p.Estado)
                .Include(p => p.ApplicationUser)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => 
                    p.NumeroPrestamo.Contains(search) ||
                    p.Notas.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(p => p.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(usuario))
            {
                query = query.Where(p => p.Id == usuario);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "numero":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.NumeroPrestamo) : 
                        query.OrderByDescending(p => p.NumeroPrestamo);
                    break;
                case "usuario":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.ApplicationUser.UserName) : 
                        query.OrderByDescending(p => p.ApplicationUser.UserName);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.Estado.Descripcion) : 
                        query.OrderByDescending(p => p.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(p => p.FechadeCreacion) : 
                        query.OrderByDescending(p => p.FechadeCreacion);
                    break;
                default:
                    query = query.OrderByDescending(p => p.FechadeCreacion);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var prestamos = query.Skip(skip).Take(pageSize).ToList();

            var result = new
            {
                items = prestamos.Select(p => new
                {
                    IdPrestamo = p.IdPrestamo,
                    NumeroPrestamo = p.NumeroPrestamo,
                    UsuarioNombre = p.ApplicationUser?.UserName ?? "Usuario no encontrado",
                    EstadoDescripcion = p.Estado.Descripcion,
                    IdEstado = p.IdEstado,
                    FechadeCreacion = p.FechadeCreacion.ToString("dd/MM/yyyy"),
                    FechaDevolucion = p.FechaDevolucion?.ToString("dd/MM/yyyy") ?? "Pendiente",
                    Notas = p.Notas ?? "Sin notas",
                    Devolucion = p.Devolucion
                }).ToList(),
                totalItems = totalItems,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Prestamos/GetAlertasRetraso - Obtener préstamos vencidos para alertas
        public ActionResult GetAlertasRetraso()
        {
            try
            {
                var prestamosVencidos = db.Prestamo
                    .Include(p => p.ApplicationUser)
                    .Include(p => p.Estado)
                    .Where(p => p.FechaVencimiento.HasValue && 
                               p.FechaVencimiento < DateTime.Now && 
                               !p.Devolucion)
                    .OrderBy(p => p.FechaVencimiento)
                    .Take(10) // Máximo 10 alertas
                    .ToList();

                var alertas = prestamosVencidos.Select(p => new
                {
                    IdPrestamo = p.IdPrestamo,
                    NumeroPrestamo = p.NumeroPrestamo,
                    UsuarioNombre = p.ApplicationUser?.UserName ?? "Usuario no encontrado",
                    UsuarioEmail = p.ApplicationUser?.Email ?? "No disponible",
                    FechaVencimiento = p.FechaVencimiento.Value.ToString("dd/MM/yyyy"),
                    DiasRetraso = (DateTime.Now - p.FechaVencimiento.Value).Days,
                    MotivoPrestamo = p.MotivoPrestamo ?? "Sin motivo especificado",
                    EsUrgente = p.EsUrgente
                }).ToList();

                return Json(new { 
                    success = true, 
                    alertas = alertas,
                    total = alertas.Count 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Error al obtener alertas: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Prestamos/MarcarDevuelto - Marcar préstamo como devuelto desde alerta
        [HttpPost]
        public ActionResult MarcarDevuelto(long id)
        {
            try
            {
                var prestamo = db.Prestamo.Find(id);
                if (prestamo == null)
                {
                    return Json(new { success = false, message = "Préstamo no encontrado" });
                }

                prestamo.Devolucion = true;
                prestamo.FechaDevolucion = DateTime.UtcNow;
                db.SaveChanges();

                return Json(new { 
                    success = true, 
                    message = "Préstamo marcado como devuelto exitosamente" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Error al marcar como devuelto: " + ex.Message 
                });
            }
        }

        // GET: Prestamos/Details/5 - Ver detalles de un préstamo
        public ActionResult Details(long id)
        {
            ViewBag.Title = "Detalles de Prestamo";
            
            var prestamo = db.Prestamo
                .Include(p => p.Estado)
                .Include(p => p.ApplicationUser)
                .FirstOrDefault(p => p.IdPrestamo == id);

            if (prestamo == null)
            {
                return HttpNotFound();
            }

            return View(prestamo);
        }

        // GET: Prestamos/Create - Crear nuevo préstamo
        public ActionResult Create()
        {
            ViewBag.Title = "Crear Prestamo";
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName");

            return View();
        }

        // POST: Prestamos/Create - Procesar creación de nuevo préstamo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NumeroPrestamo,Id,IdEstado,Notas,Devolucion,FechaVencimiento,DuracionEstimada,MotivoPrestamo,FechaInicioUso,FechaFinUso,EsUrgente")] Prestamo prestamo)
        {
            try
            {
                // Generar número de préstamo automático si no se proporciona
                if (string.IsNullOrEmpty(prestamo.NumeroPrestamo))
                {
                    var ultimoPrestamo = db.Prestamo
                        .OrderByDescending(p => p.IdPrestamo)
                        .FirstOrDefault();
                    
                    long siguienteNumero = 1;
                    if (ultimoPrestamo != null && !string.IsNullOrEmpty(ultimoPrestamo.NumeroPrestamo))
                    {
                        // Intentar parsear el número, si falla usar el ID como respaldo
                        if (!long.TryParse(ultimoPrestamo.NumeroPrestamo, out siguienteNumero))
                        {
                            // Si el parseo falla, usar el ID del último préstamo + 1
                            siguienteNumero = ultimoPrestamo.IdPrestamo + 1;
                        }
                        else
                        {
                            siguienteNumero++;
                        }
                    }
                    
                    prestamo.NumeroPrestamo = siguienteNumero.ToString().PadLeft(6, '0');
                    
                    // Limpiar el error del ModelState para NumeroPrestamo
                    ModelState.Remove("NumeroPrestamo");
                }

                if (ModelState.IsValid)
                {
                    prestamo.FechadeCreacion = DateTime.UtcNow;
                    
                    // Calcular fecha de vencimiento si no se proporciona
                    if (!prestamo.FechaVencimiento.HasValue && prestamo.DuracionEstimada.HasValue)
                    {
                        prestamo.FechaVencimiento = prestamo.FechadeCreacion.AddDays(prestamo.DuracionEstimada.Value);
                    }
                    
                    // Si no hay duración estimada ni fecha de vencimiento, usar 7 días por defecto
                    if (!prestamo.FechaVencimiento.HasValue)
                    {
                        prestamo.FechaVencimiento = prestamo.FechadeCreacion.AddDays(7);
                        prestamo.DuracionEstimada = 7;
                    }
                    
                    if (prestamo.Devolucion)
                    {
                        prestamo.FechaDevolucion = DateTime.UtcNow;
                    }
                    
                    db.Prestamo.Add(prestamo);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Prestamo creado exitosamente.";
                return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el prestamo: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", prestamo.IdEstado);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", prestamo.Id);

            return View(prestamo);
        }

        // GET: Prestamos/Edit/5 - Editar préstamo
        public ActionResult Edit(long id)
        {
            ViewBag.Title = "Editar Prestamo";
            
            var prestamo = db.Prestamo
                .Include(p => p.Estado)
                .Include(p => p.ApplicationUser)
                .FirstOrDefault(p => p.IdPrestamo == id);
                
            if (prestamo == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", prestamo.IdEstado);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", prestamo.Id);

            return View(prestamo);
        }

        // POST: Prestamos/Edit/5 - Procesar edición de préstamo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdPrestamo,NumeroPrestamo,IdEstado,Notas,Devolucion,DuracionEstimada,MotivoPrestamo,EsUrgente")] Prestamo prestamo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Obtener el préstamo existente para preservar campos no editables
                    var prestamoExistente = db.Prestamo.Find(prestamo.IdPrestamo);
                    if (prestamoExistente == null)
                    {
                        TempData["ErrorMessage"] = "Prestamo no encontrado.";
                        return RedirectToAction("Index");
                    }

                    // Actualizar solo los campos editables en el préstamo existente
                    prestamoExistente.IdEstado = prestamo.IdEstado;
                    prestamoExistente.Notas = prestamo.Notas;
                    prestamoExistente.Devolucion = prestamo.Devolucion;
                    prestamoExistente.DuracionEstimada = prestamo.DuracionEstimada;
                    prestamoExistente.MotivoPrestamo = prestamo.MotivoPrestamo;
                    prestamoExistente.EsUrgente = prestamo.EsUrgente;

                    // Si se marca como devuelto y no tiene fecha de devolución, asignar fecha actual
                    if (prestamoExistente.Devolucion && !prestamoExistente.FechaDevolucion.HasValue)
                    {
                        prestamoExistente.FechaDevolucion = DateTime.UtcNow;
                    }
                    // Si se desmarca como devuelto, limpiar fecha de devolución
                    else if (!prestamoExistente.Devolucion && prestamoExistente.FechaDevolucion.HasValue)
                    {
                        prestamoExistente.FechaDevolucion = null;
                    }

                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Prestamo actualizado exitosamente.";
                return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar el prestamo: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", prestamo.IdEstado);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", prestamo.Id);

            return View(prestamo);
        }

        // GET: Prestamos/Delete/5 - Confirmar eliminación
        public ActionResult Delete(long id)
        {
            ViewBag.Title = "Eliminar Prestamo";
            
            var prestamo = db.Prestamo
                .Include(p => p.Estado)
                .Include(p => p.ApplicationUser)
                .FirstOrDefault(p => p.IdPrestamo == id);

            if (prestamo == null)
            {
                return HttpNotFound();
            }

            return View(prestamo);
        }

        // POST: Prestamos/Delete/5 - Procesar eliminación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                var prestamo = db.Prestamo.Find(id);
                if (prestamo != null)
                {
                    db.Prestamo.Remove(prestamo);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Prestamo eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró el prestamo a eliminar.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el prestamo: " + ex.Message;
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
