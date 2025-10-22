using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EscuelaFelixArcadio.Models;
using Microsoft.AspNet.Identity;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize]
    public class DocumentoController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Documento - Listado general de documentos
        public ActionResult Index(string search = "", string tipo = "", string sortBy = "fecha", string sortOrder = "desc", int page = 1, int pageSize = 10)
        {
            ViewBag.Title = "Documentos Deportivos";

            // Obtener tipos de documento únicos para el filtro
            var tipos = db.Documento
                .Where(d => d.Activo)
                .Select(d => d.TipoDocumento)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Si no hay tipos en la base de datos, agregar algunos por defecto
            if (!tipos.Any())
            {
                tipos = new List<string> { "Rutina de Entrenamiento", "Ejercicios", "Guía Nutricional", "Plan de Ejercicios" };
            }

            ViewBag.TiposDocumento = tipos;

            // Consulta base - solo documentos activos
            var query = db.Documento
                .Include(d => d.ApplicationUser)
                .Where(d => d.Activo);

            // Filtrar por búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(d => d.Titulo.ToLower().Contains(search) ||
                                       d.Descripcion.ToLower().Contains(search) ||
                                       d.TipoDocumento.ToLower().Contains(search));
            }

            // Filtrar por tipo
            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(d => d.TipoDocumento == tipo);
            }

            // Aplicar ordenamiento
            switch (sortBy.ToLower())
            {
                case "titulo":
                    query = sortOrder == "asc" ? query.OrderBy(d => d.Titulo) : query.OrderByDescending(d => d.Titulo);
                    break;
                case "tipo":
                    query = sortOrder == "asc" ? query.OrderBy(d => d.TipoDocumento) : query.OrderByDescending(d => d.TipoDocumento);
                    break;
                case "tamano":
                    query = sortOrder == "asc" ? query.OrderBy(d => d.TamanoArchivo) : query.OrderByDescending(d => d.TamanoArchivo);
                    break;
                default: // fecha
                    query = sortOrder == "asc" ? query.OrderBy(d => d.FechaSubida) : query.OrderByDescending(d => d.FechaSubida);
                    break;
            }

            // Paginación
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;
            ViewBag.Tipo = tipo;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            var documentos = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View(documentos);
        }

        // GET: Documento/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var documento = db.Documento
                .Include(d => d.ApplicationUser)
                .FirstOrDefault(d => d.IdDocumento == id && d.Activo);

            if (documento == null)
            {
                return HttpNotFound();
            }

            return View(documento);
        }

        // GET: Documento/Create
        [Authorize(Roles = "Administrador,Profesor")]
        public ActionResult Create()
        {
            System.Diagnostics.Debug.WriteLine("=== MÉTODO GET CREATE ===");
            System.Diagnostics.Debug.WriteLine($"Usuario: {User.Identity.Name}");
            System.Diagnostics.Debug.WriteLine($"IsInRole Admin: {User.IsInRole("Administrador")}");
            System.Diagnostics.Debug.WriteLine($"IsInRole Profesor: {User.IsInRole("Profesor")}");
            
            ViewBag.Title = "Subir Documento";
            
            // Lista de tipos de documento predefinidos
            ViewBag.TiposDocumento = new SelectList(new List<string>
            {
                "Rutina de Entrenamiento",
                "Plan de Ejercicios",
                "Guía Nutricional",
                "Calendario Deportivo",
                "Reglamento Deportivo",
                "Manual de Técnicas",
                "Evaluación Física",
                "Otros"
            });

            return View();
        }

        // POST: Documento/Create - Carga de archivo PDF
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Profesor")]
        public ActionResult Create(string Titulo, string Descripcion, string TipoDocumento, HttpPostedFileBase archivo, string Publico)
        {
            try
            {
                // Debug: Log de entrada al método
                System.Diagnostics.Debug.WriteLine("=== INICIO MÉTODO CREATE ===");
                System.Diagnostics.Debug.WriteLine($"Título: {Titulo}");
                System.Diagnostics.Debug.WriteLine($"Tipo: {TipoDocumento}");
                System.Diagnostics.Debug.WriteLine($"Público: {Publico}");
                System.Diagnostics.Debug.WriteLine($"Archivo: {archivo?.FileName}");
                
                // Validaciones básicas
                if (string.IsNullOrEmpty(Titulo))
                {
                    ModelState.AddModelError("Titulo", "El título es obligatorio");
                }
                
                if (string.IsNullOrEmpty(TipoDocumento))
                {
                    ModelState.AddModelError("TipoDocumento", "El tipo de documento es obligatorio");
                }
                
                if (archivo == null || archivo.ContentLength == 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar un archivo PDF para subir.");
                }
                
                if (ModelState.IsValid)
                {
                    // Validar que se haya subido un archivo
                    if (archivo == null || archivo.ContentLength == 0)
                    {
                        ModelState.AddModelError("", "Debe seleccionar un archivo PDF para subir.");
                        CargarTiposDocumento();
                        return View(new Documento { Titulo = Titulo, Descripcion = Descripcion, TipoDocumento = TipoDocumento });
                    }

                    // Validar que sea un archivo PDF
                    var extension = Path.GetExtension(archivo.FileName).ToLower();
                    if (extension != ".pdf")
                    {
                        ModelState.AddModelError("", "Solo se permiten archivos PDF.");
                        CargarTiposDocumento();
                        return View(new Documento { Titulo = Titulo, Descripcion = Descripcion, TipoDocumento = TipoDocumento });
                    }

                    // Validar tamaño (máximo 10 MB)
                    if (archivo.ContentLength > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "El archivo no puede exceder 10 MB.");
                        CargarTiposDocumento();
                        return View(new Documento { Titulo = Titulo, Descripcion = Descripcion, TipoDocumento = TipoDocumento });
                    }

                    // Crear directorio si no existe
                    string directorioBase = Server.MapPath("~/Documentos");
                    if (!Directory.Exists(directorioBase))
                    {
                        Directory.CreateDirectory(directorioBase);
                    }

                    // Crear subdirectorio para documentos deportivos
                    string directorioDeportivo = Path.Combine(directorioBase, "Deportivos");
                    if (!Directory.Exists(directorioDeportivo))
                    {
                        Directory.CreateDirectory(directorioDeportivo);
                    }

                    // Generar nombre único para el archivo
                    string nombreUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}_{Path.GetFileName(archivo.FileName)}";
                    string rutaCompleta = Path.Combine(directorioDeportivo, nombreUnico);

                    // Guardar archivo
                    archivo.SaveAs(rutaCompleta);

                    // Crear nuevo objeto Documento
                    var nuevoDocumento = new Documento
                    {
                        Titulo = Titulo,
                        Descripcion = Descripcion,
                        TipoDocumento = TipoDocumento,
                        NombreArchivo = Path.GetFileName(archivo.FileName),
                        RutaArchivo = $"~/Documentos/Deportivos/{nombreUnico}",
                        TamanoArchivo = archivo.ContentLength,
                        FechaSubida = DateTime.Now,
                        Id = User.Identity.GetUserId(),
                        Activo = true,
                        Publico = !string.IsNullOrEmpty(Publico) && Publico.ToLower() == "true"
                    };

                    db.Documento.Add(nuevoDocumento);
                    db.SaveChanges();
                    
                    // Debug: Confirmar que se guardó
                    System.Diagnostics.Debug.WriteLine($"Documento guardado con ID: {nuevoDocumento.IdDocumento}");

                    TempData["Success"] = "Documento subido exitosamente.";
                    return RedirectToAction("Index");
                }

                CargarTiposDocumento();
                return View(new Documento { Titulo = Titulo, Descripcion = Descripcion, TipoDocumento = TipoDocumento });
            }
            catch (Exception ex)
            {
                // Log detallado del error
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", $"Error al subir el documento: {ex.Message}");
                CargarTiposDocumento();
                return View(new Documento { Titulo = Titulo, Descripcion = Descripcion, TipoDocumento = TipoDocumento });
            }
        }

        // GET: Documento/Edit/5
        [Authorize(Roles = "Administrador,Profesor")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var documento = db.Documento.Find(id);
            if (documento == null || !documento.Activo)
            {
                return HttpNotFound();
            }

            // Verificar que el usuario sea el creador o administrador
            var userId = User.Identity.GetUserId();
            if (documento.Id != userId && !User.IsInRole("Administrador"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            ViewBag.TiposDocumento = new SelectList(new List<string>
            {
                "Rutina de Entrenamiento",
                "Plan de Ejercicios",
                "Guía Nutricional",
                "Calendario Deportivo",
                "Reglamento Deportivo",
                "Manual de Técnicas",
                "Evaluación Física",
                "Otros"
            }, documento.TipoDocumento);

            return View(documento);
        }

        // POST: Documento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Profesor")]
        public ActionResult Edit([Bind(Include = "IdDocumento,Titulo,Descripcion,TipoDocumento,Publico,NombreArchivo,RutaArchivo,TamanoArchivo,FechaSubida,Id,Activo")] Documento documento, HttpPostedFileBase archivo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var documentoExistente = db.Documento.Find(documento.IdDocumento);
                    if (documentoExistente == null)
                    {
                        return HttpNotFound();
                    }

                    // Verificar permisos
                    var userId = User.Identity.GetUserId();
                    if (documentoExistente.Id != userId && !User.IsInRole("Administrador"))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                    }

                    // Si se subió un nuevo archivo
                    if (archivo != null && archivo.ContentLength > 0)
                    {
                        // Validar que sea PDF
                        var extension = Path.GetExtension(archivo.FileName).ToLower();
                        if (extension != ".pdf")
                        {
                            ModelState.AddModelError("", "Solo se permiten archivos PDF.");
                            CargarTiposDocumentoEdit(documento.TipoDocumento);
                            return View(documento);
                        }

                        // Validar tamaño
                        if (archivo.ContentLength > 10 * 1024 * 1024)
                        {
                            ModelState.AddModelError("", "El archivo no puede exceder 10 MB.");
                            CargarTiposDocumentoEdit(documento.TipoDocumento);
                            return View(documento);
                        }

                        // Eliminar archivo anterior
                        var rutaAnterior = Server.MapPath(documentoExistente.RutaArchivo);
                        if (System.IO.File.Exists(rutaAnterior))
                        {
                            System.IO.File.Delete(rutaAnterior);
                        }

                        // Guardar nuevo archivo
                        string directorioDeportivo = Server.MapPath("~/Documentos/Deportivos");
                        if (!Directory.Exists(directorioDeportivo))
                        {
                            Directory.CreateDirectory(directorioDeportivo);
                        }

                        string nombreUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}_{Path.GetFileName(archivo.FileName)}";
                        string rutaCompleta = Path.Combine(directorioDeportivo, nombreUnico);
                        archivo.SaveAs(rutaCompleta);

                        documentoExistente.NombreArchivo = Path.GetFileName(archivo.FileName);
                        documentoExistente.RutaArchivo = $"~/Documentos/Deportivos/{nombreUnico}";
                        documentoExistente.TamanoArchivo = archivo.ContentLength;
                    }

                    // Actualizar otros campos
                    documentoExistente.Titulo = documento.Titulo;
                    documentoExistente.Descripcion = documento.Descripcion;
                    documentoExistente.TipoDocumento = documento.TipoDocumento;
                    documentoExistente.Publico = documento.Publico;

                    db.Entry(documentoExistente).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Success"] = "Documento actualizado exitosamente.";
                    return RedirectToAction("Index");
                }

                CargarTiposDocumentoEdit(documento.TipoDocumento);
                return View(documento);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar el documento: {ex.Message}");
                CargarTiposDocumentoEdit(documento.TipoDocumento);
                return View(documento);
            }
        }

        // GET: Documento/Delete/5
        [Authorize(Roles = "Administrador,Profesor")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var documento = db.Documento
                .Include(d => d.ApplicationUser)
                .FirstOrDefault(d => d.IdDocumento == id && d.Activo);

            if (documento == null)
            {
                return HttpNotFound();
            }

            // Verificar permisos
            var userId = User.Identity.GetUserId();
            if (documento.Id != userId && !User.IsInRole("Administrador"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(documento);
        }

        // POST: Documento/Delete/5 - Eliminación física de la base de datos
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Profesor")]
        public ActionResult DeleteConfirmed(long id)
        {
            try
            {
                var documento = db.Documento.Find(id);
                if (documento == null)
                {
                    TempData["Error"] = "El documento no existe.";
                    return RedirectToAction("Index");
                }

                // Verificar permisos
                var userId = User.Identity.GetUserId();
                if (documento.Id != userId && !User.IsInRole("Administrador"))
                {
                    TempData["Error"] = "No tiene permisos para eliminar este documento.";
                    return RedirectToAction("Index");
                }

                // Log para debug
                System.Diagnostics.Debug.WriteLine($"=== ELIMINANDO DOCUMENTO ID: {id} ===");
                System.Diagnostics.Debug.WriteLine($"Título: {documento.Titulo}");
                System.Diagnostics.Debug.WriteLine($"Ruta archivo: {documento.RutaArchivo}");

                // Intentar eliminar el archivo físico del servidor
                try
                {
                    var rutaArchivo = Server.MapPath(documento.RutaArchivo);
                    if (System.IO.File.Exists(rutaArchivo))
                    {
                        System.IO.File.Delete(rutaArchivo);
                        System.Diagnostics.Debug.WriteLine($"Archivo físico eliminado: {rutaArchivo}");
                    }
                }
                catch (Exception exArchivo)
                {
                    System.Diagnostics.Debug.WriteLine($"No se pudo eliminar el archivo físico: {exArchivo.Message}");
                    // Continuar con la eliminación de la base de datos aunque falle el archivo
                }

                // Eliminación FÍSICA del registro en la base de datos
                db.Documento.Remove(documento);
                
                // Guardar los cambios en la base de datos
                int filasAfectadas = db.SaveChanges();
                
                // Verificar que se eliminó correctamente
                System.Diagnostics.Debug.WriteLine($"Filas eliminadas: {filasAfectadas}");

                if (filasAfectadas > 0)
                {
                    TempData["Success"] = "Documento eliminado exitosamente de la base de datos.";
                }
                else
                {
                    TempData["Warning"] = "El documento no pudo ser eliminado correctamente.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR al eliminar documento: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                TempData["Error"] = $"Error al eliminar el documento: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Documento/Download/5 - Descarga de archivo
        public ActionResult Download(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var documento = db.Documento.Find(id);
            if (documento == null || !documento.Activo)
            {
                return HttpNotFound();
            }

            var rutaArchivo = Server.MapPath(documento.RutaArchivo);
            if (!System.IO.File.Exists(rutaArchivo))
            {
                TempData["Error"] = "El archivo no se encuentra disponible.";
                return RedirectToAction("Index");
            }

            // Leer archivo y enviarlo como descarga
            var bytes = System.IO.File.ReadAllBytes(rutaArchivo);
            return File(bytes, "application/pdf", documento.NombreArchivo);
        }

        // GET: Documento/View/5 - Visor de PDF en línea
        public ActionResult View(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var documento = db.Documento
                .Include(d => d.ApplicationUser)
                .FirstOrDefault(d => d.IdDocumento == id && d.Activo);

            if (documento == null)
            {
                return HttpNotFound();
            }

            var rutaArchivo = Server.MapPath(documento.RutaArchivo);
            if (!System.IO.File.Exists(rutaArchivo))
            {
                TempData["Error"] = "El archivo no se encuentra disponible.";
                return RedirectToAction("Index");
            }

            ViewBag.Title = documento.Titulo;
            return View(documento);
        }

        // GET: Documento/GetPDF/5 - Obtener PDF para visualización (endpoint para el visor)
        public ActionResult GetPDF(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var documento = db.Documento.Find(id);
            if (documento == null || !documento.Activo)
            {
                return HttpNotFound();
            }

            var rutaArchivo = Server.MapPath(documento.RutaArchivo);
            if (!System.IO.File.Exists(rutaArchivo))
            {
                return HttpNotFound();
            }

            // Devolver el PDF directamente para que el visor lo pueda cargar
            var bytes = System.IO.File.ReadAllBytes(rutaArchivo);
            return File(bytes, "application/pdf");
        }

        // Método auxiliar para cargar tipos de documento
        private void CargarTiposDocumento()
        {
            ViewBag.TiposDocumento = new SelectList(new List<string>
            {
                "Rutina de Entrenamiento",
                "Plan de Ejercicios",
                "Guía Nutricional",
                "Calendario Deportivo",
                "Reglamento Deportivo",
                "Manual de Técnicas",
                "Evaluación Física",
                "Otros"
            });
        }

        // Método auxiliar para cargar tipos de documento en Edit
        private void CargarTiposDocumentoEdit(string tipoSeleccionado)
        {
            ViewBag.TiposDocumento = new SelectList(new List<string>
            {
                "Rutina de Entrenamiento",
                "Plan de Ejercicios",
                "Guía Nutricional",
                "Calendario Deportivo",
                "Reglamento Deportivo",
                "Manual de Técnicas",
                "Evaluación Física",
                "Otros"
            }, tipoSeleccionado);
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
