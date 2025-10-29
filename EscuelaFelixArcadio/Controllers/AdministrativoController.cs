using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using EscuelaFelixArcadio.Models;
using EscuelaFelixArcadio.Models.ViewModels;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministrativoController : Controller
    {
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            }
        }

        // GET: Administrativo
        public ActionResult Index()
        {
            ViewBag.Title = "Gestión Administrativa";
            return View();
        }

        // GET: Administrativo/Usuarios
        public ActionResult Usuarios()
        {
            ViewBag.Title = "Gestión de Usuarios";
            return View();
        }

        // GET: Administrativo/Roles
        public ActionResult Roles()
        {
            ViewBag.Title = "Gestión de Roles";
            return View();
        }

        // GET: Administrativo/Configuracion
        public ActionResult Configuracion()
        {
            ViewBag.Title = "Configuración del Sistema";
            return View();
        }

        #region Gestión de Roles

        // GET: Listar usuarios con sus roles
        public async Task<ActionResult> ListarUsuariosConRoles()
        {
            var usuarios = UserManager.Users.ToList();
            var roles = RoleManager.Roles.ToList();
            
            var usuariosConRoles = new List<UsuarioRolViewModel>();
            int contador = 1;
            
            foreach (var usuario in usuarios)
            {
                var rolesUsuario = await UserManager.GetRolesAsync(usuario.Id);
                var rolActual = rolesUsuario.FirstOrDefault() ?? "Sin rol";
                
                usuariosConRoles.Add(new UsuarioRolViewModel
                {
                    Id = contador++,
                    UserId = usuario.Id,
                    Email = usuario.Email,
                    RolActual = rolActual,
                    RolSeleccionado = rolActual,
                    RolesDisponibles = roles.Select(r => r.Name).ToList()
                });
            }
            
            ViewBag.Titulo = "Gestión de Roles de Usuarios";
            return View(usuariosConRoles);
        }

        // POST: Cambiar rol de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarRolUsuario(string userId, string nuevoRol)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(nuevoRol))
                {
                    return Json(new { success = false, message = "Datos incompletos" });
                }

                var usuario = await UserManager.FindByIdAsync(userId);
                
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }
                
                // Obtener rol actual
                var rolesActuales = await UserManager.GetRolesAsync(userId);
                
                // Eliminar todos los roles actuales
                foreach (var rol in rolesActuales)
                {
                    await UserManager.RemoveFromRoleAsync(userId, rol);
                }
                
                // Asignar nuevo rol
                var resultado = await UserManager.AddToRoleAsync(userId, nuevoRol);
                
                if (resultado.Succeeded)
                {
                    return Json(new { success = true, message = $"Rol cambiado a {nuevoRol} exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = string.Join(", ", resultado.Errors) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion

        #region Gestión de Usuarios (CRUD)

        // GET: Listar todos los usuarios
        public ActionResult ListarUsuarios(string searchEmail = "", int page = 1, int pageSize = 12)
        {
            var usuarios = UserManager.Users.ToList();
            
            if (!string.IsNullOrEmpty(searchEmail))
            {
                usuarios = usuarios.Where(u => u.Email.Contains(searchEmail)).ToList();
            }
            
            // Calcular paginación
            var totalItems = usuarios.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;
            
            var usuariosViewModel = usuarios
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new UsuarioViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    EstaBloqueado = u.LockoutEnabled && u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc.Value > DateTimeOffset.UtcNow,
                    EmailConfirmed = u.EmailConfirmed,
                    FechaRegistro = null
                }).ToList();
            
            ViewBag.SearchEmail = searchEmail;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            
            return View(usuariosViewModel);
        }

        // AJAX: Buscar usuarios con filtros y paginación
        [HttpGet]
        public JsonResult SearchUsuarios(string searchEmail = "", int page = 1, int pageSize = 12)
        {
            var usuarios = UserManager.Users.ToList();
            
            if (!string.IsNullOrEmpty(searchEmail))
            {
                usuarios = usuarios.Where(u => u.Email.Contains(searchEmail)).ToList();
            }
            
            // Calcular paginación
            var totalItems = usuarios.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;
            
            var usuariosViewModel = usuarios
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new
                {
                    Id = u.Id,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber ?? "No registrado",
                    EstaBloqueado = u.LockoutEnabled && u.LockoutEndDateUtc.HasValue && u.LockoutEndDateUtc.Value > DateTimeOffset.UtcNow,
                    EmailConfirmed = u.EmailConfirmed,
                    FechaRegistro = u.LockoutEndDateUtc?.ToString("dd/MM/yyyy") ?? ""
                }).ToList();
            
            var result = new
            {
                items = usuariosViewModel,
                totalItems = totalItems,
                totalPages = totalPages,
                currentPage = page,
                pageSize = pageSize
            };
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Crear usuario
        public ActionResult CrearUsuario()
        {
            var roles = RoleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View(new CrearUsuarioViewModel());
        }

        // POST: Crear usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearUsuario(CrearUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                
                var resultado = await UserManager.CreateAsync(usuario, model.Password);
                
                if (resultado.Succeeded)
                {
                    // Habilitar bloqueo de cuenta
                    await UserManager.SetLockoutEnabledAsync(usuario.Id, true);
                    
                    // Asignar rol si se especificó
                    if (!string.IsNullOrEmpty(model.RolInicial) && RoleManager.RoleExists(model.RolInicial))
                    {
                        await UserManager.AddToRoleAsync(usuario.Id, model.RolInicial);
                    }
                    else
                    {
                        // Asignar rol Estudiante por defecto
                        await UserManager.AddToRoleAsync(usuario.Id, "Estudiante");
                    }
                    
                    TempData["SuccessMessage"] = "Usuario creado exitosamente";
                    return RedirectToAction("ListarUsuarios");
                }
                
                AddErrors(resultado);
            }
            
            var roles = RoleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View(model);
        }

        // GET: Detalles de usuario
        public async Task<ActionResult> DetallesUsuario(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var usuario = await UserManager.FindByIdAsync(id);
            
            if (usuario == null)
            {
                return HttpNotFound();
            }
            
            var roles = await UserManager.GetRolesAsync(usuario.Id);
            
            var viewModel = new UsuarioViewModel
            {
                Id = usuario.Id,
                Email = usuario.Email,
                PhoneNumber = usuario.PhoneNumber,
                EstaBloqueado = usuario.LockoutEnabled && usuario.LockoutEndDateUtc.HasValue && usuario.LockoutEndDateUtc.Value > DateTimeOffset.UtcNow,
                EmailConfirmed = usuario.EmailConfirmed,
                FechaRegistro = null
            };
            
            ViewBag.RolUsuario = roles.FirstOrDefault() ?? "Sin rol";
            return View(viewModel);
        }

        // GET: Editar usuario
        public async Task<ActionResult> EditarUsuario(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var usuario = await UserManager.FindByIdAsync(id);
            
            if (usuario == null)
            {
                return HttpNotFound();
            }
            
            // Obtener rol del usuario
            var rolesUsuario = await UserManager.GetRolesAsync(usuario.Id);
            var rolUsuario = rolesUsuario.FirstOrDefault() ?? "Sin rol";
            
            // Obtener todos los roles para el dropdown
            var todosLosRoles = RoleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(todosLosRoles, "Name", "Name", rolUsuario);
            
            var viewModel = new EditarUsuarioViewModel
            {
                Id = usuario.Id,
                Email = usuario.Email,
                PhoneNumber = usuario.PhoneNumber,
                RolInicial = rolUsuario,
                EmailConfirmed = usuario.EmailConfirmed,
                EsActivo = !(usuario.LockoutEnabled && usuario.LockoutEndDateUtc.HasValue && usuario.LockoutEndDateUtc.Value > DateTimeOffset.UtcNow)
            };
            
            return View(viewModel);
        }

        // POST: Editar usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarUsuario(EditarUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByIdAsync(model.Id);
                
                if (usuario == null)
                {
                    return HttpNotFound();
                }
                
                // Actualizar email (solo lectura en la vista, pero mantener)
                // usuario.Email = model.Email;
                // usuario.UserName = model.Email;
                
                // Actualizar teléfono
                usuario.PhoneNumber = model.PhoneNumber;
                
                // Actualizar confirmación de email
                usuario.EmailConfirmed = model.EmailConfirmed;
                
                // Actualizar rol del usuario
                if (!string.IsNullOrEmpty(model.RolInicial))
                {
                    // Obtener roles actuales
                    var rolesActuales = await UserManager.GetRolesAsync(usuario.Id);
                    
                    // Remover todos los roles
                    if (rolesActuales.Any())
                    {
                        await UserManager.RemoveFromRolesAsync(usuario.Id, rolesActuales.ToArray());
                    }
                    
                    // Agregar nuevo rol
                    await UserManager.AddToRoleAsync(usuario.Id, model.RolInicial);
                }
                
                // Actualizar estado (bloqueado/activo)
                if (model.EsActivo)
                {
                    // Desbloquear usuario
                    await UserManager.SetLockoutEnabledAsync(usuario.Id, true);
                    await UserManager.SetLockoutEndDateAsync(usuario.Id, DateTimeOffset.UtcNow.AddYears(-1));
                }
                else
                {
                    // Bloquear usuario
                    await UserManager.SetLockoutEnabledAsync(usuario.Id, true);
                    await UserManager.SetLockoutEndDateAsync(usuario.Id, DateTimeOffset.UtcNow.AddYears(100));
                }
                
                var resultado = await UserManager.UpdateAsync(usuario);
                
                if (resultado.Succeeded)
                {
                    TempData["SuccessMessage"] = "Usuario actualizado exitosamente";
                    return RedirectToAction("ListarUsuarios");
                }
                
                AddErrors(resultado);
            }
            
            // Si hay error, cargar los roles nuevamente
            var todosLosRoles = RoleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(todosLosRoles, "Name", "Name", model.RolInicial);
            
            return View(model);
        }

        // GET: Eliminar usuario
        public async Task<ActionResult> EliminarUsuario(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var usuario = await UserManager.FindByIdAsync(id);
            
            if (usuario == null)
            {
                return HttpNotFound();
            }
            
            // Obtener rol del usuario
            var roles = await UserManager.GetRolesAsync(usuario.Id);
            var rolUsuario = roles.FirstOrDefault() ?? "Sin rol";
            
            var viewModel = new UsuarioViewModel
            {
                Id = usuario.Id,
                Email = usuario.Email,
                PhoneNumber = usuario.PhoneNumber,
                EstaBloqueado = usuario.LockoutEnabled && usuario.LockoutEndDateUtc.HasValue && usuario.LockoutEndDateUtc.Value > DateTimeOffset.UtcNow,
                EmailConfirmed = usuario.EmailConfirmed
            };
            
            // Pasar información adicional a la vista
            ViewBag.RolUsuario = rolUsuario;
            
            return View(viewModel);
        }

        // POST: Confirmar eliminación de usuario
        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmarEliminarUsuario(string id)
        {
            var usuario = await UserManager.FindByIdAsync(id);
            
            if (usuario != null)
            {
                var resultado = await UserManager.DeleteAsync(usuario);
                
                if (resultado.Succeeded)
                {
                    TempData["SuccessMessage"] = "Usuario eliminado exitosamente";
                    return RedirectToAction("ListarUsuarios");
                }
                
                AddErrors(resultado);
            }
            
            return RedirectToAction("ListarUsuarios");
        }

        #endregion

        #region Bloqueo y Desbloqueo de Cuentas

        // POST: Bloquear usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BloquearUsuario(string userId, string razon)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Usuario no especificado" });
                }
                
                var usuario = await UserManager.FindByIdAsync(userId);
                
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }
                
                // Bloquear la cuenta permanentemente
                await UserManager.SetLockoutEnabledAsync(userId, true);
                await UserManager.SetLockoutEndDateAsync(userId, DateTimeOffset.UtcNow.AddYears(1));
                
                return Json(new { success = true, message = "Usuario bloqueado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Desbloquear usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DesbloquearUsuario(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Usuario no especificado" });
                }
                
                var usuario = await UserManager.FindByIdAsync(userId);
                
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }
                
                // Desbloquear la cuenta
                await UserManager.SetLockoutEndDateAsync(userId, DateTimeOffset.UtcNow);
                await UserManager.ResetAccessFailedCountAsync(userId);
                
                return Json(new { success = true, message = "Usuario desbloqueado exitosamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Cambiar contraseña de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarContrasena(CambiarPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByIdAsync(model.UserId);
                
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }
                
                var token = await UserManager.GeneratePasswordResetTokenAsync(model.UserId);
                var resultado = await UserManager.ResetPasswordAsync(model.UserId, token, model.NuevaPassword);
                
                if (resultado.Succeeded)
                {
                    return Json(new { success = true, message = "Contraseña cambiada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = string.Join(", ", resultado.Errors) });
                }
            }
            
            return Json(new { success = false, message = "Datos inválidos" });
        }

        #endregion

        #region Gestión de Mensajes del Sistema

        // GET: Listar mensajes del sistema
        public ActionResult ListarMensajesSistema()
        {
            using (var db = new ApplicationDbContext())
            {
                var mensajes = db.MensajeSistema.ToList();
                return View(mensajes);
            }
        }

        // GET: Crear mensaje del sistema
        public ActionResult CrearMensajeSistema()
        {
            var roles = RoleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View(new MensajeSistema());
        }

        // POST: Crear mensaje del sistema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearMensajeSistema(MensajeSistema mensaje)
        {
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    mensaje.IdUsuarioCreacion = User.Identity.GetUserId();
                    mensaje.FechaCreacion = DateTime.Now;
                    db.MensajeSistema.Add(mensaje);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Mensaje creado exitosamente";
                    return RedirectToAction("ListarMensajesSistema");
                }
            }
            
            var roles = RoleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View(mensaje);
        }

        // GET: Editar mensaje del sistema
        public ActionResult EditarMensajeSistema(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var mensaje = db.MensajeSistema.Find(id);
                
                if (mensaje == null)
                {
                    return HttpNotFound();
                }
                
                var roles = RoleManager.Roles.ToList();
                ViewBag.Roles = new SelectList(roles, "Name", "Name");
                return View(mensaje);
            }
        }

        // POST: Editar mensaje del sistema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMensajeSistema(MensajeSistema mensaje)
        {
            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    db.Entry(mensaje).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Mensaje actualizado exitosamente";
                    return RedirectToAction("ListarMensajesSistema");
                }
            }
            
            var roles = RoleManager.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "Name", "Name");
            return View(mensaje);
        }

        // GET: Detalles de mensaje del sistema
        public ActionResult DetallesMensajeSistema(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var mensaje = db.MensajeSistema.Find(id);
                
                if (mensaje == null)
                {
                    return HttpNotFound();
                }
                
                return View(mensaje);
            }
        }

        // GET: Eliminar mensaje del sistema
        public ActionResult EliminarMensajeSistema(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var mensaje = db.MensajeSistema.Find(id);
                
                if (mensaje == null)
                {
                    return HttpNotFound();
                }
                
                return View(mensaje);
            }
        }

        // POST: Confirmar eliminación de mensaje del sistema
        [HttpPost, ActionName("EliminarMensajeSistema")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarEliminarMensajeSistema(int id)
        {
            using (var db = new ApplicationDbContext())
            {
                var mensaje = db.MensajeSistema.Find(id);
                
                if (mensaje != null)
                {
                    db.MensajeSistema.Remove(mensaje);
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Mensaje eliminado exitosamente";
                }
            }
            
            return RedirectToAction("ListarMensajesSistema");
        }

        // POST: Activar/Desactivar mensaje del sistema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstadoMensajeSistema(int id, bool activo)
        {
            using (var db = new ApplicationDbContext())
            {
                var mensaje = db.MensajeSistema.Find(id);
                
                if (mensaje != null)
                {
                    mensaje.Activo = activo;
                    db.SaveChanges();
                    
                    return Json(new { success = true, message = mensaje.Activo ? "Mensaje activado" : "Mensaje desactivado" });
                }
                
                return Json(new { success = false, message = "Mensaje no encontrado" });
            }
        }

        #endregion

        #region Métodos auxiliares

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        #endregion
    }
}
