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
    public class EspaciosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Espacios - Lista principal de espacios
        public ActionResult Index(string search = "", string estado = "", 
            string sortBy = "nombre", string sortOrder = "asc", int page = 1, int pageSize = 12)
        {
            ViewBag.Title = "Gestion de Espacios";
            
            // Obtener datos para los filtros
            ViewBag.Estados = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            // Consulta base
            var query = db.Espacio
                .Include(e => e.Estado)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => 
                    e.Nombre.Contains(search) ||
                    e.Codigo.Contains(search) ||
                    e.Descripcion.Contains(search) ||
                    e.Ubicacion.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(e => e.IdEstado == estadoId);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "nombre":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Nombre) : 
                        query.OrderByDescending(e => e.Nombre);
                    break;
                case "codigo":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Codigo) : 
                        query.OrderByDescending(e => e.Codigo);
                    break;
                case "capacidad":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Capacidad) : 
                        query.OrderByDescending(e => e.Capacidad);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Estado.Descripcion) : 
                        query.OrderByDescending(e => e.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.FechaCreacion) : 
                        query.OrderByDescending(e => e.FechaCreacion);
                    break;
                default:
                    query = query.OrderBy(e => e.Nombre);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var espacios = query.Skip(skip).Take(pageSize).ToList();

            // Pasar datos a la vista
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentEstado = estado;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;

            return View(espacios);
        }

        // AJAX: Buscar espacios con filtros
        [HttpGet]
        public JsonResult SearchEspacios(string search = "", string estado = "", 
            string sortBy = "nombre", string sortOrder = "asc", int page = 1, int pageSize = 12)
        {
            // Consulta base
            var query = db.Espacio
                .Include(e => e.Estado)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => 
                    e.Nombre.Contains(search) ||
                    e.Codigo.Contains(search) ||
                    e.Descripcion.Contains(search) ||
                    e.Ubicacion.Contains(search));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                int estadoId = int.Parse(estado);
                query = query.Where(e => e.IdEstado == estadoId);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "nombre":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Nombre) : 
                        query.OrderByDescending(e => e.Nombre);
                    break;
                case "codigo":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Codigo) : 
                        query.OrderByDescending(e => e.Codigo);
                    break;
                case "capacidad":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Capacidad) : 
                        query.OrderByDescending(e => e.Capacidad);
                    break;
                case "estado":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.Estado.Descripcion) : 
                        query.OrderByDescending(e => e.Estado.Descripcion);
                    break;
                case "fecha":
                    query = sortOrder == "asc" ? 
                        query.OrderBy(e => e.FechaCreacion) : 
                        query.OrderByDescending(e => e.FechaCreacion);
                    break;
                default:
                    query = query.OrderBy(e => e.Nombre);
                    break;
            }

            // Calcular paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;

            var espacios = query.Skip(skip).Take(pageSize).ToList();

            var result = new
            {
                items = espacios.Select(e => new
                {
                    IdEspacio = e.IdEspacio,
                    Codigo = e.Codigo ?? "N/A",
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion ?? "Sin descripcion",
                    Capacidad = e.Capacidad ?? 0,
                    Ubicacion = e.Ubicacion ?? "No especificada",
                    EstadoDescripcion = e.Estado.Descripcion,
                    IdEstado = e.IdEstado,
                    FechaCreacion = e.FechaCreacion.ToString("dd/MM/yyyy")
                }).ToList(),
                totalItems = totalItems,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Espacios/Details/5 - Ver detalles de un espacio
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Detalles de Espacio";
            
            var espacio = db.Espacio
                .Include(e => e.Estado)
                .FirstOrDefault(e => e.IdEspacio == id);

            if (espacio == null)
            {
                return HttpNotFound();
            }

            return View(espacio);
        }

        // GET: Espacios/Create - Crear nuevo espacio
        public ActionResult Create()
        {
            ViewBag.Title = "Crear Espacio";
            
            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion");

            return View();
        }

        // POST: Espacios/Create - Procesar creación de nuevo espacio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Codigo,Nombre,Descripcion,Capacidad,Ubicacion,IdEstado")] Espacio espacio)
        {
            try
            {
                // Validación del código - solo letras
                if (!string.IsNullOrEmpty(espacio.Codigo))
                {
                    var codigoRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$");
                    if (!codigoRegex.IsMatch(espacio.Codigo))
                    {
                        ModelState.AddModelError("Codigo", "El código del espacio solo puede contener letras.");
                    }
                }

                // Validación del nombre - no números
                if (!string.IsNullOrEmpty(espacio.Nombre))
                {
                    var nombreRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
                    if (!nombreRegex.IsMatch(espacio.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El nombre del espacio no puede contener números.");
                    }
                }

                // Validación de la ubicación - solo letras
                if (!string.IsNullOrEmpty(espacio.Ubicacion))
                {
                    var ubicacionRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
                    if (!ubicacionRegex.IsMatch(espacio.Ubicacion))
                    {
                        ModelState.AddModelError("Ubicacion", "La ubicación solo puede contener letras.");
                    }
                }

                if (ModelState.IsValid)
                {
                    // Siempre generar código automáticamente si está vacío o es null
                    if (string.IsNullOrEmpty(espacio.Codigo) || espacio.Codigo.Trim() == "" || espacio.Codigo == "N/A")
                    {
                        // Generar código basado en el nombre
                        var nombreSinEspacios = espacio.Nombre.Replace(" ", "").ToUpper();
                        var codigoBase = nombreSinEspacios.Length > 6 ? nombreSinEspacios.Substring(0, 6) : nombreSinEspacios;
                        
                        // Si el nombre está vacío, usar un código por defecto
                        if (string.IsNullOrEmpty(codigoBase))
                        {
                            codigoBase = "ESPACIO";
                        }
                        
                        // Verificar si el código generado ya existe y agregar número si es necesario
                        var codigoFinal = codigoBase;
                        var contador = 1;
                        
                        while (db.Espacio.Any(e => e.Codigo.ToLower() == codigoFinal.ToLower()))
                        {
                            codigoFinal = codigoBase + contador.ToString();
                            contador++;
                        }
                        
                        espacio.Codigo = codigoFinal;
                    }
                    else
                    {
                        // Verificar si ya existe un espacio con el mismo código
                        var existeEspacio = db.Espacio
                            .FirstOrDefault(e => e.Codigo.ToLower() == espacio.Codigo.ToLower());

                        if (existeEspacio != null)
                        {
                            ModelState.AddModelError("Codigo", "Ya existe un espacio con este codigo.");
                            
                            ViewBag.IdEstado = new SelectList(
                                db.Estado.OrderBy(e => e.Descripcion), 
                                "IdEstado", "Descripcion", espacio.IdEstado);

                            return View(espacio);
                        }
                    }

                    // Asegurar que siempre tenga un código válido
                    if (string.IsNullOrEmpty(espacio.Codigo))
                    {
                        var nombreSinEspacios = espacio.Nombre.Replace(" ", "").ToUpper();
                        var codigoBase = nombreSinEspacios.Length > 6 ? nombreSinEspacios.Substring(0, 6) : nombreSinEspacios;
                        if (string.IsNullOrEmpty(codigoBase))
                        {
                            codigoBase = "ESPACIO";
                        }
                        
                        var codigoFinal = codigoBase;
                        var contador = 1;
                        
                        while (db.Espacio.Any(e => e.Codigo.ToLower() == codigoFinal.ToLower()))
                        {
                            codigoFinal = codigoBase + contador.ToString();
                            contador++;
                        }
                        
                        espacio.Codigo = codigoFinal;
                    }

                    espacio.FechaCreacion = DateTime.UtcNow;
                    db.Espacio.Add(espacio);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Espacio creado exitosamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al crear el espacio: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", espacio.IdEstado);

            return View(espacio);
        }

        // GET: Espacios/Edit/5 - Editar espacio
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Editar Espacio";
            
            var espacio = db.Espacio
                .Include(e => e.Estado)
                .FirstOrDefault(e => e.IdEspacio == id);
                
            if (espacio == null)
            {
                return HttpNotFound();
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", espacio.IdEstado);

            return View(espacio);
        }

        // POST: Espacios/Edit/5 - Procesar edición de espacio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdEspacio,Codigo,Nombre,Descripcion,Capacidad,Ubicacion,IdEstado,FechaCreacion")] Espacio espacio)
        {
            try
            {
                // Validación del código - solo letras
                if (!string.IsNullOrEmpty(espacio.Codigo))
                {
                    var codigoRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$");
                    if (!codigoRegex.IsMatch(espacio.Codigo))
                    {
                        ModelState.AddModelError("Codigo", "El código del espacio solo puede contener letras.");
                    }
                }

                // Validación del nombre - no números
                if (!string.IsNullOrEmpty(espacio.Nombre))
                {
                    var nombreRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
                    if (!nombreRegex.IsMatch(espacio.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El nombre del espacio no puede contener números.");
                    }
                }

                // Validación de la ubicación - solo letras
                if (!string.IsNullOrEmpty(espacio.Ubicacion))
                {
                    var ubicacionRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$");
                    if (!ubicacionRegex.IsMatch(espacio.Ubicacion))
                    {
                        ModelState.AddModelError("Ubicacion", "La ubicación solo puede contener letras.");
                    }
                }

                if (ModelState.IsValid)
                {
                    // Verificar si ya existe otro espacio con el mismo código
                    if (!string.IsNullOrEmpty(espacio.Codigo))
                    {
                        var existeEspacio = db.Espacio
                            .FirstOrDefault(e => e.Codigo.ToLower() == espacio.Codigo.ToLower() &&
                                                e.IdEspacio != espacio.IdEspacio);

                        if (existeEspacio != null)
                        {
                            ModelState.AddModelError("Codigo", "Ya existe otro espacio con este codigo.");
                            
                            ViewBag.IdEstado = new SelectList(
                                db.Estado.OrderBy(e => e.Descripcion), 
                                "IdEstado", "Descripcion", espacio.IdEstado);

                            return View(espacio);
                        }
                    }

                    db.Entry(espacio).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Espacio actualizado exitosamente.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar el espacio: " + ex.Message);
            }

            ViewBag.IdEstado = new SelectList(
                db.Estado.OrderBy(e => e.Descripcion), 
                "IdEstado", "Descripcion", espacio.IdEstado);

            return View(espacio);
        }

        // GET: Espacios/Delete/5 - Confirmar eliminación
        public ActionResult Delete(int id)
        {
            ViewBag.Title = "Eliminar Espacio";
            
            var espacio = db.Espacio
                .Include(e => e.Estado)
                .FirstOrDefault(e => e.IdEspacio == id);

            if (espacio == null)
            {
                return HttpNotFound();
            }

            // Verificar si hay reservas activas para este espacio
            var tieneReservasActivas = db.ReservaEspacio
                .Any(r => r.IdEspacio == id && r.IdEstado == 1 && r.FechaFin >= DateTime.Now);
            
            ViewBag.TieneReservasActivas = tieneReservasActivas;

            return View(espacio);
        }

        // POST: Espacios/Delete/5 - Procesar eliminación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var espacio = db.Espacio.Find(id);
                if (espacio != null)
                {
                    // Verificar si hay reservas activas
                    var tieneReservasActivas = db.ReservaEspacio
                        .Any(r => r.IdEspacio == id && r.IdEstado == 1 && r.FechaFin >= DateTime.Now);
                    
                    if (tieneReservasActivas)
                    {
                        TempData["ErrorMessage"] = "No se puede eliminar el espacio porque tiene reservas activas.";
                        return RedirectToAction("Delete", new { id = id });
                    }

                    db.Espacio.Remove(espacio);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Espacio eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontro el espacio a eliminar.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el espacio: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: Espacios/GetEspaciosDisponibles - Obtener espacios disponibles para una fecha
        [HttpGet]
        public JsonResult GetEspaciosDisponibles(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // Obtener IDs de espacios que ya están reservados en el rango de fechas
                var espaciosReservados = db.ReservaEspacio
                    .Where(r => r.IdEstado == 1 && 
                               ((r.FechaInicio <= fechaFin && r.FechaFin >= fechaInicio)))
                    .Select(r => r.IdEspacio)
                    .Distinct()
                    .ToList();

                // Obtener espacios disponibles (activos y no reservados)
                var espaciosDisponibles = db.Espacio
                    .Include(e => e.Estado)
                    .Where(e => e.IdEstado == 1 && !espaciosReservados.Contains(e.IdEspacio))
                    .OrderBy(e => e.Nombre)
                    .Select(e => new
                    {
                        IdEspacio = e.IdEspacio,
                        Codigo = e.Codigo ?? "N/A",
                        Nombre = e.Nombre,
                        Descripcion = e.Descripcion ?? "Sin descripcion",
                        Capacidad = e.Capacidad ?? 0,
                        Ubicacion = e.Ubicacion ?? "No especificada"
                    })
                    .ToList();

                return Json(new { 
                    success = true, 
                    espacios = espaciosDisponibles,
                    total = espaciosDisponibles.Count 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Error al obtener espacios disponibles: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Espacios/VerificarDisponibilidad - Verificar si un espacio está disponible
        [HttpGet]
        public JsonResult VerificarDisponibilidad(int idEspacio, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // Verificar si el espacio está activo
                var espacio = db.Espacio.Find(idEspacio);
                if (espacio == null || espacio.IdEstado != 1)
                {
                    return Json(new { 
                        disponible = false, 
                        message = "El espacio no esta disponible o no existe" 
                    }, JsonRequestBehavior.AllowGet);
                }

                // Verificar si hay reservas que se traslapen
                var tieneReservas = db.ReservaEspacio
                    .Any(r => r.IdEspacio == idEspacio && 
                             r.IdEstado == 1 &&
                             ((r.FechaInicio <= fechaFin && r.FechaFin >= fechaInicio)));

                return Json(new { 
                    disponible = !tieneReservas,
                    message = tieneReservas ? "El espacio ya esta reservado para estas fechas" : "El espacio esta disponible"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    disponible = false, 
                    message = "Error al verificar disponibilidad: " + ex.Message 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Espacios/Reservas - Vista de reservas de espacios
        public ActionResult Reservas()
        {
            ViewBag.Title = "Reservas de Espacios";
            return View();
        }

        // GET: Espacios/Mantenimiento - Vista de mantenimiento (conservada)
        public ActionResult Mantenimiento()
        {
            ViewBag.Title = "Gestion de Mantenimiento";
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
