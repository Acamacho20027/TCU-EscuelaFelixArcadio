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
    public class ReservasEspacioController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ReservasEspacio - Lista principal de reservas de espacios
        public ActionResult Index(string search = "", string estado = "", string espacio = "", string usuario = "",
            string sortBy = "fecha", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            ViewBag.Title = "Reservas de Espacios";
            
            // Obtener datos para los filtros
            ViewBag.Estados = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            ViewBag.Espacios = new SelectList(
                db.Espacio.OrderBy(e => e.Nombre), 
                "IdEspacio", "Nombre");

            ViewBag.Usuarios = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName");

            // Consulta base
            var query = db.ReservaEspacio
                .Include(r => r.Espacio)
                .Include(r => r.Estado)
                .Include(r => r.ApplicationUser)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.Espacio.Nombre.Contains(search) ||
                    r.Espacio.Codigo.Contains(search) ||
                    r.Notas.Contains(search) ||
                    r.ApplicationUser.UserName.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(r => r.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(espacio))
            {
                int espacioId = int.Parse(espacio);
                query = query.Where(r => r.IdEspacio == espacioId);
            }

            if (!string.IsNullOrEmpty(usuario))
            {
                query = query.Where(r => r.Id == usuario);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "espacio":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.Espacio.Nombre) : 
                        query.OrderByDescending(r => r.Espacio.Nombre);
                    break;
                case "usuario":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.ApplicationUser.UserName) : 
                        query.OrderByDescending(r => r.ApplicationUser.UserName);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.Estado.Descripcion) : 
                        query.OrderByDescending(r => r.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.FechaInicio) : 
                        query.OrderByDescending(r => r.FechaInicio);
                    break;
                default:
                    query = query.OrderByDescending(r => r.FechaCreacion);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var reservas = query.Skip(skip).Take(pageSize).ToList();

            // Pasar datos a la vista
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentEstado = estado;
            ViewBag.CurrentEspacio = espacio;
            ViewBag.CurrentUsuario = usuario;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            return View(reservas);
        }

        // AJAX: Buscar reservas con filtros
        [HttpGet]
        public JsonResult SearchReservas(string search = "", string estado = "", string espacio = "", string usuario = "",
            string sortBy = "fecha", string sortOrder = "desc", int page = 1, int pageSize = 12)
        {
            // Consulta base
            var query = db.ReservaEspacio
                .Include(r => r.Espacio)
                .Include(r => r.Estado)
                .Include(r => r.ApplicationUser)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => 
                    r.Espacio.Nombre.Contains(search) ||
                    r.Espacio.Codigo.Contains(search) ||
                    r.Notas.Contains(search) ||
                    r.ApplicationUser.UserName.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(r => r.IdEstado == estadoId);
            }

            if (!string.IsNullOrEmpty(espacio))
            {
                int espacioId = int.Parse(espacio);
                query = query.Where(r => r.IdEspacio == espacioId);
            }

            if (!string.IsNullOrEmpty(usuario))
            {
                query = query.Where(r => r.Id == usuario);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "espacio":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.Espacio.Nombre) : 
                        query.OrderByDescending(r => r.Espacio.Nombre);
                    break;
                case "usuario":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.ApplicationUser.UserName) : 
                        query.OrderByDescending(r => r.ApplicationUser.UserName);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.Estado.Descripcion) : 
                        query.OrderByDescending(r => r.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(r => r.FechaInicio) : 
                        query.OrderByDescending(r => r.FechaInicio);
                    break;
                default:
                    query = query.OrderByDescending(r => r.FechaCreacion);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var reservas = query.Skip(skip).Take(pageSize).ToList();

            var result = new
            {
                items = reservas.Select(r => new
                {
                    IdReservaEspacio = r.IdReservaEspacio,
                    EspacioNombre = r.Espacio.Nombre,
                    EspacioCodigo = r.Espacio.Codigo ?? "N/A",
                    UsuarioNombre = r.ApplicationUser?.UserName ?? "Usuario no encontrado",
                    EstadoDescripcion = r.Estado.Descripcion,
                    IdEstado = r.IdEstado,
                    FechaInicio = r.FechaInicio.ToString("dd/MM/yyyy HH:mm"),
                    FechaFin = r.FechaFin.ToString("dd/MM/yyyy HH:mm"),
                    Notas = r.Notas ?? "Sin notas",
                    FechaCreacion = r.FechaCreacion.ToString("dd/MM/yyyy")
                }).ToList(),
                totalItems = totalItems,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: ReservasEspacio/GetReservasPorFecha - Obtener reservas para calendario
        [HttpGet]
        public JsonResult GetReservasPorFecha(DateTime? fechaInicio, DateTime? fechaFin, int? idEspacio = null)
        {
            try
            {
                var query = db.ReservaEspacio
                    .Include(r => r.Espacio)
                    .Include(r => r.ApplicationUser)
                    .Include(r => r.Estado)
                    .AsQueryable();

                // Filtrar por rango de fechas si se proporciona
                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    query = query.Where(r => r.FechaInicio <= fechaFin && r.FechaFin >= fechaInicio);
                }

                // Filtrar por espacio específico si se proporciona
                if (idEspacio.HasValue)
                {
                    query = query.Where(r => r.IdEspacio == idEspacio.Value);
                }

                var reservas = query
                    .OrderBy(r => r.FechaInicio)
                    .Select(r => new
                    {
                        id = r.IdReservaEspacio,
                        title = r.Espacio.Nombre + " - " + r.ApplicationUser.UserName,
                        start = r.FechaInicio,
                        end = r.FechaFin,
                        backgroundColor = r.IdEstado == 1 ? "#0ea5e9" : "#6b7280",
                        borderColor = r.IdEstado == 1 ? "#0284c7" : "#4b5563",
                        textColor = "#ffffff",
                        extendedProps = new
                        {
                            idReserva = r.IdReservaEspacio,
                            espacioNombre = r.Espacio.Nombre,
                            espacioCodigo = r.Espacio.Codigo ?? "N/A",
                            usuarioNombre = r.ApplicationUser.UserName,
                            usuarioEmail = r.ApplicationUser.Email,
                            estadoDescripcion = r.Estado.Descripcion,
                            notas = r.Notas ?? "Sin notas"
                        }
                    })
                    .ToList();

                return Json(reservas, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = true, 
                    message = "Error al obtener reservas: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ReservasEspacio/Details/5 - Ver detalles de una reserva
        public ActionResult Details(long id)
        {
            ViewBag.Title = "Detalles de Reserva";
            
            var reserva = db.ReservaEspacio
                .Include(r => r.Espacio)
                .Include(r => r.Estado)
                .Include(r => r.ApplicationUser)
                .FirstOrDefault(r => r.IdReservaEspacio == id);

            if (reserva == null)
            {
                return HttpNotFound();
            }

            return View(reserva);
        }

        // GET: ReservasEspacio/Create - Crear nueva reserva
        public ActionResult Create()
        {
            ViewBag.Title = "Crear Reserva de Espacio";
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            ViewBag.IdEspacio = new SelectList(
                db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                "IdEspacio", "Nombre");

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName");

            return View();
        }

        // POST: ReservasEspacio/Create - Procesar creación de nueva reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdEspacio,Id,FechaInicio,FechaFin,IdEstado,Notas")] ReservaEspacio reserva)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validar que la fecha de fin sea posterior a la fecha de inicio
                    if (reserva.FechaFin <= reserva.FechaInicio)
                    {
                        ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", reserva.IdEstado);

                        ViewBag.IdEspacio = new SelectList(
                            db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                            "IdEspacio", "Nombre", reserva.IdEspacio);

                        ViewBag.Id = new SelectList(
                            db.Users.OrderBy(u => u.UserName), 
                            "Id", "UserName", reserva.Id);

                        return View(reserva);
                    }

                    // Verificar que el espacio esté disponible en el rango de fechas
                    var tieneConflicto = db.ReservaEspacio
                        .Any(r => r.IdEspacio == reserva.IdEspacio && 
                                 r.IdEstado == 1 &&
                                 ((r.FechaInicio <= reserva.FechaFin && r.FechaFin >= reserva.FechaInicio)));

                    if (tieneConflicto)
                    {
                        ModelState.AddModelError("", "El espacio ya esta reservado para estas fechas.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", reserva.IdEstado);

                        ViewBag.IdEspacio = new SelectList(
                            db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                            "IdEspacio", "Nombre", reserva.IdEspacio);

                        ViewBag.Id = new SelectList(
                            db.Users.OrderBy(u => u.UserName), 
                            "Id", "UserName", reserva.Id);

                        return View(reserva);
                    }

                    reserva.FechaCreacion = DateTime.UtcNow;
                    db.ReservaEspacio.Add(reserva);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Reserva creada exitosamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear la reserva: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", reserva.IdEstado);

            ViewBag.IdEspacio = new SelectList(
                db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                "IdEspacio", "Nombre", reserva.IdEspacio);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", reserva.Id);

            return View(reserva);
        }

        // GET: ReservasEspacio/Edit/5 - Editar reserva
        public ActionResult Edit(long id)
        {
            ViewBag.Title = "Editar Reserva";
            
            var reserva = db.ReservaEspacio
                .Include(r => r.Espacio)
                .Include(r => r.Estado)
                .Include(r => r.ApplicationUser)
                .FirstOrDefault(r => r.IdReservaEspacio == id);
                
            if (reserva == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", reserva.IdEstado);

            ViewBag.IdEspacio = new SelectList(
                db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                "IdEspacio", "Nombre", reserva.IdEspacio);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", reserva.Id);

            return View(reserva);
        }

        // POST: ReservasEspacio/Edit/5 - Procesar edición de reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdReservaEspacio,IdEspacio,Id,FechaInicio,FechaFin,IdEstado,Notas,FechaCreacion")] ReservaEspacio reserva)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validar que la fecha de fin sea posterior a la fecha de inicio
                    if (reserva.FechaFin <= reserva.FechaInicio)
                    {
                        ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", reserva.IdEstado);

                        ViewBag.IdEspacio = new SelectList(
                            db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                            "IdEspacio", "Nombre", reserva.IdEspacio);

                        ViewBag.Id = new SelectList(
                            db.Users.OrderBy(u => u.UserName), 
                            "Id", "UserName", reserva.Id);

                        return View(reserva);
                    }

                    // Verificar conflictos de reservas (excluyendo la reserva actual)
                    var tieneConflicto = db.ReservaEspacio
                        .Any(r => r.IdEspacio == reserva.IdEspacio && 
                                 r.IdReservaEspacio != reserva.IdReservaEspacio &&
                                 r.IdEstado == 1 &&
                                 ((r.FechaInicio <= reserva.FechaFin && r.FechaFin >= reserva.FechaInicio)));

                    if (tieneConflicto)
                    {
                        ModelState.AddModelError("", "El espacio ya esta reservado para estas fechas.");
                        
                        ViewBag.IdEstado = new SelectList(
                            db.Estado.OrderBy(e => e.Descripcion), 
                            "IdEstado", "Descripcion", reserva.IdEstado);

                        ViewBag.IdEspacio = new SelectList(
                            db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                            "IdEspacio", "Nombre", reserva.IdEspacio);

                        ViewBag.Id = new SelectList(
                            db.Users.OrderBy(u => u.UserName), 
                            "Id", "UserName", reserva.Id);

                        return View(reserva);
                    }

                    // Cargar la reserva existente y actualizar campos
                    var reservaExistente = db.ReservaEspacio.Find(reserva.IdReservaEspacio);
                    if (reservaExistente == null)
                    {
                        TempData["ErrorMessage"] = "La reserva no fue encontrada.";
                        return RedirectToAction("Index");
                    }

                    reservaExistente.IdEspacio = reserva.IdEspacio;
                    reservaExistente.Id = reserva.Id;
                    reservaExistente.FechaInicio = reserva.FechaInicio;
                    reservaExistente.FechaFin = reserva.FechaFin;
                    reservaExistente.IdEstado = reserva.IdEstado;
                    reservaExistente.Notas = reserva.Notas;

                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Reserva actualizada exitosamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar la reserva: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", reserva.IdEstado);

            ViewBag.IdEspacio = new SelectList(
                db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                "IdEspacio", "Nombre", reserva.IdEspacio);

            ViewBag.Id = new SelectList(
                db.Users.OrderBy(u => u.UserName), 
                "Id", "UserName", reserva.Id);

            return View(reserva);
        }

        // GET: ReservasEspacio/Delete/5 - Confirmar eliminación/cancelación
        public ActionResult Delete(long id)
        {
            ViewBag.Title = "Cancelar Reserva";
            
            var reserva = db.ReservaEspacio
                .Include(r => r.Espacio)
                .Include(r => r.Estado)
                .Include(r => r.ApplicationUser)
                .FirstOrDefault(r => r.IdReservaEspacio == id);

            if (reserva == null)
            {
                return HttpNotFound();
            }

            return View(reserva);
        }

        // POST: ReservasEspacio/Delete/5 - Procesar eliminación/cancelación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                var reserva = db.ReservaEspacio.Find(id);
                if (reserva != null)
                {
                    db.ReservaEspacio.Remove(reserva);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Reserva cancelada exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontro la reserva a cancelar.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cancelar la reserva: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: ReservasEspacio/Calendario - Vista de calendario
        public ActionResult Calendario()
        {
            ViewBag.Title = "Calendario de Reservas";
            
            // Obtener espacios para el filtro
            ViewBag.Espacios = new SelectList(
                db.Espacio.Where(e => e.IdEstado == 1).OrderBy(e => e.Nombre), 
                "IdEspacio", "Nombre");

            return View();
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

