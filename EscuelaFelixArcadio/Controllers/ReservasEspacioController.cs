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
                    .ToList()
                    .Select(r => new Dictionary<string, object>
                    {
                        {"id", r.IdReservaEspacio},
                        {"title", (r.Espacio != null ? r.Espacio.Nombre : "Espacio desconocido") + " - " + (r.ApplicationUser != null ? r.ApplicationUser.UserName : "Usuario desconocido")},
                        {"start", r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss")},
                        {"end", r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss")},
                        {"backgroundColor", r.IdEstado == 1 ? "#10b981" : "#ef4444"},
                        {"borderColor", r.IdEstado == 1 ? "#059669" : "#dc2626"},
                        {"textColor", "#ffffff"},
                        {"extendedProps", new Dictionary<string, object>
                        {
                            {"idReserva", r.IdReservaEspacio},
                            {"espacioNombre", r.Espacio != null ? r.Espacio.Nombre : "N/A"},
                            {"espacioCodigo", r.Espacio != null ? (r.Espacio.Codigo ?? "N/A") : "N/A"},
                            {"usuarioNombre", r.ApplicationUser != null ? r.ApplicationUser.UserName : "N/A"},
                            {"usuarioEmail", r.ApplicationUser != null ? r.ApplicationUser.Email : "N/A"},
                            {"estadoDescripcion", r.Estado != null ? r.Estado.Descripcion : "N/A"},
                            {"notas", r.Notas ?? "Sin notas"}
                        }}
                    })
                    .ToList();

                // Debug: Log para verificar qué se está devolviendo
                System.Diagnostics.Debug.WriteLine($"Reservas encontradas: {reservas.Count}");
                foreach (var reserva in reservas)
                {
                    System.Diagnostics.Debug.WriteLine($"Reserva: {reserva["title"]} - {reserva["start"]} a {reserva["end"]}");
                }

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

                    // Verificar que el espacio esté disponible en el mismo día
                    var fechaInicioDia = reserva.FechaInicio.Date;
                    var fechaFinDia = reserva.FechaFin.Date;
                    
                    // Si las fechas son diferentes, no permitir
                    if (fechaInicioDia != fechaFinDia)
                    {
                        ModelState.AddModelError("FechaFin", "Las reservas solo pueden ser para el mismo día.");
                        
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
                    
                    // Verificar que no haya otra reserva en el mismo día para el mismo espacio
                    var tieneConflicto = db.ReservaEspacio
                        .Any(r => r.IdEspacio == reserva.IdEspacio && 
                                 r.IdEstado == 1 &&
                                 DbFunctions.TruncateTime(r.FechaInicio) == fechaInicioDia);

                    if (tieneConflicto)
                    {
                        ModelState.AddModelError("", "El espacio ya está reservado para este día. Solo se permite una reserva por día.");
                        
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
                    // Cargar la reserva existente para obtener los datos originales
                    var reservaOriginal = db.ReservaEspacio.Find(reserva.IdReservaEspacio);
                    if (reservaOriginal == null)
                    {
                        TempData["ErrorMessage"] = "La reserva no fue encontrada.";
                        return RedirectToAction("Index");
                    }

                    // Solo validar si se cambió el espacio (las fechas no se pueden cambiar)
                    if (reserva.IdEspacio != reservaOriginal.IdEspacio)
                    {
                        // Verificar que no haya otra reserva en el mismo día para el nuevo espacio
                        var fechaInicioDia = reservaOriginal.FechaInicio.Date;
                        
                    var tieneConflicto = db.ReservaEspacio
                        .Any(r => r.IdEspacio == reserva.IdEspacio && 
                                 r.IdReservaEspacio != reserva.IdReservaEspacio &&
                                 r.IdEstado == 1 &&
                                     DbFunctions.TruncateTime(r.FechaInicio) == fechaInicioDia);

                    if (tieneConflicto)
                    {
                            ModelState.AddModelError("", "El espacio ya está reservado para este día. Solo se permite una reserva por día.");
                        
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
                    }

                    // Actualizar solo los campos permitidos
                    reservaOriginal.IdEspacio = reserva.IdEspacio;
                    // reservaOriginal.Id = reserva.Id; // No se puede cambiar el usuario
                    // reservaOriginal.FechaInicio = reserva.FechaInicio; // No se puede cambiar la fecha de inicio
                    // reservaOriginal.FechaFin = reserva.FechaFin; // No se puede cambiar la fecha de fin
                    reservaOriginal.IdEstado = reserva.IdEstado;
                    reservaOriginal.Notas = reserva.Notas;

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

        // GET: ReservasEspacio/GetEspacios - Obtener espacios para el modal
        [HttpGet]
        public JsonResult GetEspacios()
        {
            try
            {
                var espacios = db.Espacio
                    .Where(e => e.IdEstado == 1)
                    .OrderBy(e => e.Nombre)
                    .Select(e => new
                    {
                        IdEspacio = e.IdEspacio,
                        Nombre = e.Nombre,
                        Codigo = e.Codigo ?? "N/A"
                    })
                    .ToList();

                return Json(espacios, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    error = true, 
                    message = "Error al obtener espacios: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ReservasEspacio/GetUsuarios - Obtener usuarios para el modal
        [HttpGet]
        public JsonResult GetUsuarios()
        {
            try
            {
                var usuarios = db.Users
                    .OrderBy(u => u.UserName)
                    .Select(u => new
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email
                    })
                    .ToList();

                // Si no hay usuarios, devolver un mensaje informativo
                if (usuarios.Count == 0)
                {
                    return Json(new List<object> { new { 
                        Id = "", 
                        UserName = "No hay usuarios disponibles", 
                        Email = "" 
                    }}, JsonRequestBehavior.AllowGet);
                }

                return Json(usuarios, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                System.Diagnostics.Debug.WriteLine("Error en GetUsuarios: " + ex.Message);
                
                return Json(new List<object> { new { 
                    Id = "", 
                    UserName = "Error al cargar usuarios", 
                    Email = "" 
                }}, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ReservasEspacio/CreateFromCalendar - Crear reserva desde el calendario
        [HttpPost]
        public JsonResult CreateFromCalendar(int IdEspacio, string IdUsuario, DateTime FechaInicio, DateTime FechaFin, string Notas, int IdEstado = 1)
        {
            try
            {
                // Validar que la fecha de fin sea posterior a la fecha de inicio
                if (FechaFin <= FechaInicio)
                {
                    return Json(new { 
                        success = false, 
                        message = "La fecha de fin debe ser posterior a la fecha de inicio." 
                    });
                }

                // Verificar que el usuario existe
                var usuario = db.Users.Find(IdUsuario);
                if (usuario == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "El usuario seleccionado no existe." 
                    });
                }

                // Verificar que el espacio existe
                var espacio = db.Espacio.Find(IdEspacio);
                if (espacio == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "El espacio seleccionado no existe." 
                    });
                }

                // Verificar que el espacio esté disponible en el mismo día
                var fechaInicioDia = FechaInicio.Date;
                var fechaFinDia = FechaFin.Date;
                
                // Si las fechas son diferentes, no permitir
                if (fechaInicioDia != fechaFinDia)
                {
                    return Json(new { 
                        success = false, 
                        message = "Las reservas solo pueden ser para el mismo día." 
                    });
                }
                
                // Verificar que no haya otra reserva en el mismo día para el mismo espacio
                var tieneConflicto = db.ReservaEspacio
                    .Any(r => r.IdEspacio == IdEspacio && 
                             r.IdEstado == 1 &&
                             DbFunctions.TruncateTime(r.FechaInicio) == fechaInicioDia);

                if (tieneConflicto)
                {
                    return Json(new { 
                        success = false, 
                        message = "El espacio ya está reservado para este día. Solo se permite una reserva por día." 
                    });
                }

                var reserva = new ReservaEspacio
                {
                    IdEspacio = IdEspacio,
                    Id = IdUsuario,
                    FechaInicio = FechaInicio,
                    FechaFin = FechaFin,
                    IdEstado = IdEstado,
                    Notas = Notas ?? "",
                    FechaCreacion = DateTime.UtcNow
                };

                db.ReservaEspacio.Add(reserva);
                db.SaveChanges();

                // Recargar la reserva con las propiedades de navegación
                var reservaCreada = db.ReservaEspacio
                    .Include(r => r.Espacio)
                    .Include(r => r.ApplicationUser)
                    .Include(r => r.Estado)
                    .FirstOrDefault(r => r.IdReservaEspacio == reserva.IdReservaEspacio);

                return Json(new { 
                    success = true, 
                    message = "Reserva creada exitosamente.",
                    idReserva = reserva.IdReservaEspacio
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en CreateFromCalendar: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("StackTrace: " + ex.StackTrace);
                
                return Json(new { 
                    success = false, 
                    message = "Error al crear la reserva: " + ex.Message 
                });
            }
        }

        // GET: ReservasEspacio/CheckUsers - Verificar si hay usuarios (método temporal para debugging)
        [HttpGet]
        public JsonResult CheckUsers()
        {
            try
            {
                var userCount = db.Users.Count();
                var users = db.Users.Take(5).Select(u => new { 
                    Id = u.Id, 
                    UserName = u.UserName, 
                    Email = u.Email 
                }).ToList();
                
                return Json(new { 
                    count = userCount,
                    users = users,
                    message = userCount > 0 ? "Usuarios encontrados" : "No hay usuarios en la base de datos"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    count = 0,
                    users = new List<object>(),
                    message = "Error al verificar usuarios: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ReservasEspacio/TestEvents - Método temporal para probar eventos
        [HttpGet]
        public JsonResult TestEvents()
        {
            try
            {
                var reservas = db.ReservaEspacio
                    .Include(r => r.Espacio)
                    .Include(r => r.ApplicationUser)
                    .Include(r => r.Estado)
                    .OrderBy(r => r.FechaInicio)
                    .ToList()
                    .Select(r => new
                    {
                        id = r.IdReservaEspacio,
                        title = r.Espacio.Nombre + " - " + r.ApplicationUser.UserName,
                        start = r.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = r.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss"),
                        backgroundColor = r.IdEstado == 1 ? "#10b981" : "#ef4444",
                        borderColor = r.IdEstado == 1 ? "#059669" : "#dc2626",
                        textColor = "#ffffff"
                    })
                    .ToList();

                return Json(new { 
                    count = reservas.Count,
                    reservas = reservas,
                    message = $"Se encontraron {reservas.Count} reservas"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    count = 0,
                    reservas = new List<object>(),
                    message = "Error al obtener reservas: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ReservasEspacio/TestDateFormat - Método temporal para probar formato de fechas
        [HttpGet]
        public JsonResult TestDateFormat()
        {
            try
            {
                var reserva = db.ReservaEspacio
                    .Include(r => r.Espacio)
                    .Include(r => r.ApplicationUser)
                    .Include(r => r.Estado)
                    .FirstOrDefault();

                if (reserva == null)
                {
                    return Json(new { message = "No hay reservas en la base de datos" }, JsonRequestBehavior.AllowGet);
                }

                var testData = new Dictionary<string, object>
                {
                    {"id", reserva.IdReservaEspacio},
                    {"title", reserva.Espacio.Nombre + " - " + reserva.ApplicationUser.UserName},
                    {"start", reserva.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss")},
                    {"end", reserva.FechaFin.ToString("yyyy-MM-ddTHH:mm:ss")},
                    {"backgroundColor", reserva.IdEstado == 1 ? "#10b981" : "#ef4444"},
                    {"borderColor", reserva.IdEstado == 1 ? "#059669" : "#dc2626"},
                    {"textColor", "#ffffff"}
                };

                return Json(new { 
                    message = "Formato de fechas probado",
                    reserva = testData,
                    fechaOriginalInicio = reserva.FechaInicio,
                    fechaOriginalFin = reserva.FechaFin
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    message = "Error al probar formato: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

