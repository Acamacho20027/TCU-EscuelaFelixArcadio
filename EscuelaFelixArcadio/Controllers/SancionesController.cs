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
    public class SancionesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Sanciones - Lista principal de sanciones
        public ActionResult Index(string search = "", string estado = "", string usuario = "", string tipo = "",
            string sortBy = "fecha", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            ViewBag.Title = "Gestión de Sanciones";
            
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

            // Obtener tipos de sanción para filtro
            var tipos = db.Sancion
                .Where(s => !string.IsNullOrEmpty(s.Tipo))
                .Select(s => s.Tipo)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            ViewBag.Tipos = new SelectList(tipos);

            // Consulta base
            var query = db.Sancion
                .Include(s => s.Estado)
                .Include(s => s.ApplicationUser)
                .Include(s => s.Prestamo)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => 
                    s.Motivo.Contains(search) ||
                    s.Tipo.Contains(search) ||
                    s.ApplicationUser.UserName.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(s => s.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(usuario))
            {
                query = query.Where(s => s.Id == usuario);
            }

            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(s => s.Tipo == tipo);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "usuario":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.ApplicationUser.UserName) : 
                        query.OrderByDescending(s => s.ApplicationUser.UserName);
                    break;
                case "tipo":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.Tipo) : 
                        query.OrderByDescending(s => s.Tipo);
                    break;
                case "monto":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.Monto) : 
                        query.OrderByDescending(s => s.Monto);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.Estado.Descripcion) : 
                        query.OrderByDescending(s => s.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.FechaInicio) : 
                        query.OrderByDescending(s => s.FechaInicio);
                    break;
                default:
                    query = query.OrderByDescending(s => s.FechaInicio);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var sanciones = query.Skip(skip).Take(pageSize).ToList();

            // Pasar datos a la vista
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentEstado = estado;
            ViewBag.CurrentUsuario = usuario;
            ViewBag.CurrentTipo = tipo;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            return View(sanciones);
        }

        // AJAX: Buscar sanciones con filtros
        [HttpGet]
        public JsonResult SearchSanciones(string search = "", string estado = "", string usuario = "", string tipo = "",
            string sortBy = "fecha", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            // Consulta base
            var query = db.Sancion
                .Include(s => s.Estado)
                .Include(s => s.ApplicationUser)
                .Include(s => s.Prestamo)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => 
                    s.Motivo.Contains(search) ||
                    s.Tipo.Contains(search) ||
                    s.ApplicationUser.UserName.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(s => s.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(usuario))
            {
                query = query.Where(s => s.Id == usuario);
            }

            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(s => s.Tipo == tipo);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "usuario":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.ApplicationUser.UserName) : 
                        query.OrderByDescending(s => s.ApplicationUser.UserName);
                    break;
                case "tipo":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.Tipo) : 
                        query.OrderByDescending(s => s.Tipo);
                    break;
                case "monto":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.Monto) : 
                        query.OrderByDescending(s => s.Monto);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.Estado.Descripcion) : 
                        query.OrderByDescending(s => s.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(s => s.FechaInicio) : 
                        query.OrderByDescending(s => s.FechaInicio);
                    break;
                default:
                    query = query.OrderByDescending(s => s.FechaInicio);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var sanciones = query.Skip(skip).Take(pageSize).ToList();

            var result = new
            {
                items = sanciones.Select(s => new
                {
                    IdSancion = s.IdSancion,
                    UsuarioNombre = s.ApplicationUser?.UserName ?? "Usuario no encontrado",
                    Tipo = s.Tipo ?? "Sin tipo",
                    Motivo = s.Motivo ?? "Sin motivo",
                    Monto = s.Monto.ToString("C"),
                    EstadoDescripcion = s.Estado.Descripcion,
                    IdEstado = s.IdEstado,
                    FechaInicio = s.FechaInicio.ToString("dd/MM/yyyy"),
                    FechaFin = s.FechaFin?.ToString("dd/MM/yyyy") ?? "En curso",
                    IdPrestamo = s.IdPrestamo,
                    PrestamoNumero = s.Prestamo?.NumeroPrestamo ?? "N/A"
                }).ToList(),
                totalItems = totalItems,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Sanciones/GetSancionesActivas - Obtener sanciones activas
        [HttpGet]
        public JsonResult GetSancionesActivas()
        {
            try
            {
                // Obtener el estado "Activo" (asumiendo que IdEstado = 1 es Activo)
                var sancionesActivas = db.Sancion
                    .Include(s => s.ApplicationUser)
                    .Include(s => s.Estado)
                    .Where(s => s.IdEstado == 1 && !s.FechaFin.HasValue)
                    .OrderByDescending(s => s.FechaInicio)
                    .Take(10)
                    .ToList();

                var sanciones = sancionesActivas.Select(s => new
                {
                    IdSancion = s.IdSancion,
                    UsuarioNombre = s.ApplicationUser?.UserName ?? "Usuario no encontrado",
                    UsuarioEmail = s.ApplicationUser?.Email ?? "No disponible",
                    Tipo = s.Tipo ?? "Sin tipo",
                    Motivo = s.Motivo ?? "Sin motivo",
                    Monto = s.Monto,
                    FechaInicio = s.FechaInicio.ToString("dd/MM/yyyy"),
                    DiasActiva = (DateTime.Now - s.FechaInicio).Days
                }).ToList();

                return Json(new { 
                    success = true, 
                    sanciones = sanciones,
                    total = sanciones.Count 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Error al obtener sanciones activas: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Sanciones/Details/5 - Ver detalles de una sanción
        public ActionResult Details(long id)
        {
            ViewBag.Title = "Detalles de Sanción";
            
            var sancion = db.Sancion
                .Include(s => s.Estado)
                .Include(s => s.ApplicationUser)
                .Include(s => s.Prestamo)
                .FirstOrDefault(s => s.IdSancion == id);

            if (sancion == null)
            {
                return HttpNotFound();
            }

            return View(sancion);
        }

        // GET: Sanciones/Create - Crear nueva sanción
        public ActionResult Create()
        {
            ViewBag.Title = "Crear Sanción";
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName");

            // Obtener préstamos para vincular (opcional)
            ViewBag.IdPrestamo = new SelectList(
                db.Prestamo.OrderByDescending(p => p.FechadeCreacion), 
                "IdPrestamo", "NumeroPrestamo");

            return View();
        }

        // POST: Sanciones/Create - Procesar creación de nueva sanción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdPrestamo,IdEstado,Motivo,Tipo,Monto,FechaInicio,FechaFin")] Sancion sancion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validar que la fecha de fin sea posterior a la fecha de inicio
                    if (sancion.FechaFin.HasValue && sancion.FechaFin.Value < sancion.FechaInicio)
                    {
                        ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", sancion.IdEstado);

                        ViewBag.Id = new SelectList(
                            db.Users.OrderBy(u => u.UserName), 
                            "Id", "UserName", sancion.Id);

                        ViewBag.IdPrestamo = new SelectList(
                            db.Prestamo.OrderByDescending(p => p.FechadeCreacion), 
                            "IdPrestamo", "NumeroPrestamo", sancion.IdPrestamo);

                        return View(sancion);
                    }

                    db.Sancion.Add(sancion);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Sanción creada exitosamente.";
                return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear la sanción: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", sancion.IdEstado);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", sancion.Id);

            ViewBag.IdPrestamo = new SelectList(
                db.Prestamo.OrderByDescending(p => p.FechadeCreacion), 
                "IdPrestamo", "NumeroPrestamo", sancion.IdPrestamo);

            return View(sancion);
        }

        // GET: Sanciones/Edit/5 - Editar sanción
        public ActionResult Edit(long id)
        {
            ViewBag.Title = "Editar Sanción";
            
            var sancion = db.Sancion
                .Include(s => s.Estado)
                .Include(s => s.ApplicationUser)
                .Include(s => s.Prestamo)
                .FirstOrDefault(s => s.IdSancion == id);
                
            if (sancion == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", sancion.IdEstado);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", sancion.Id);

            ViewBag.IdPrestamo = new SelectList(
                db.Prestamo.OrderByDescending(p => p.FechadeCreacion), 
                "IdPrestamo", "NumeroPrestamo", sancion.IdPrestamo);

            return View(sancion);
        }

        // POST: Sanciones/Edit/5 - Procesar edición de sanción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdSancion,Id,IdPrestamo,IdEstado,Motivo,Tipo,Monto,FechaInicio,FechaFin")] Sancion sancion)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validar que la fecha de fin sea posterior a la fecha de inicio
                    if (sancion.FechaFin.HasValue && sancion.FechaFin.Value < sancion.FechaInicio)
                    {
                        ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", sancion.IdEstado);

                        ViewBag.Id = new SelectList(
                            db.Users.OrderBy(u => u.UserName), 
                            "Id", "UserName", sancion.Id);

                        ViewBag.IdPrestamo = new SelectList(
                            db.Prestamo.OrderByDescending(p => p.FechadeCreacion), 
                            "IdPrestamo", "NumeroPrestamo", sancion.IdPrestamo);

                        return View(sancion);
                    }

                    // Cargar la sanción existente desde la base de datos
                    var sancionExistente = db.Sancion.Find(sancion.IdSancion);
                    if (sancionExistente == null)
                    {
                        TempData["ErrorMessage"] = "La sanción no fue encontrada.";
                        return RedirectToAction("Index");
                    }

                    // Actualizar solo los campos permitidos
                    sancionExistente.IdEstado = sancion.IdEstado;
                    sancionExistente.Motivo = sancion.Motivo;
                    sancionExistente.Tipo = sancion.Tipo;
                    sancionExistente.Monto = sancion.Monto;
                    sancionExistente.FechaInicio = sancion.FechaInicio;
                    sancionExistente.FechaFin = sancion.FechaFin;
                    sancionExistente.IdPrestamo = sancion.IdPrestamo;

                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Sanción actualizada exitosamente.";
                return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar la sanción: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", sancion.IdEstado);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", sancion.Id);

            ViewBag.IdPrestamo = new SelectList(
                db.Prestamo.OrderByDescending(p => p.FechadeCreacion), 
                "IdPrestamo", "NumeroPrestamo", sancion.IdPrestamo);

            return View(sancion);
        }

        // GET: Sanciones/Delete/5 - Confirmar eliminación
        public ActionResult Delete(long id)
        {
            ViewBag.Title = "Eliminar Sanción";
            
            var sancion = db.Sancion
                .Include(s => s.Estado)
                .Include(s => s.ApplicationUser)
                .Include(s => s.Prestamo)
                .FirstOrDefault(s => s.IdSancion == id);

            if (sancion == null)
            {
                return HttpNotFound();
            }

            return View(sancion);
        }

        // POST: Sanciones/Delete/5 - Procesar eliminación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                var sancion = db.Sancion.Find(id);
                if (sancion != null)
                {
                    db.Sancion.Remove(sancion);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Sanción eliminada exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró la sanción a eliminar.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar la sanción: " + ex.Message;
            }

                return RedirectToAction("Index");
        }

        // GET: Sanciones/GetSancionesPorUsuario - Obtener sanciones de un usuario específico
        [HttpGet]
        public JsonResult GetSancionesPorUsuario(string userId)
        {
            try
            {
                var sanciones = db.Sancion
                    .Include(s => s.Estado)
                    .Include(s => s.Prestamo)
                    .Where(s => s.Id == userId)
                    .OrderByDescending(s => s.FechaInicio)
                    .Select(s => new
                    {
                        IdSancion = s.IdSancion,
                        Tipo = s.Tipo ?? "Sin tipo",
                        Motivo = s.Motivo ?? "Sin motivo",
                        Monto = s.Monto,
                        EstadoDescripcion = s.Estado.Descripcion,
                        FechaInicio = s.FechaInicio,
                        FechaFin = s.FechaFin,
                        PrestamoNumero = s.Prestamo != null ? s.Prestamo.NumeroPrestamo : "N/A"
                    })
                    .ToList();

                return Json(new { 
                    success = true, 
                    sanciones = sanciones,
                    total = sanciones.Count 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Error al obtener sanciones del usuario: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
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

