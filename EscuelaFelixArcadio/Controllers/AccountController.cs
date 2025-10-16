using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using EscuelaFelixArcadio.Models;

namespace EscuelaFelixArcadio.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar el usuario por email
            var user = await UserManager.FindByEmailAsync(model.Email);
            
            // Si el usuario no existe, mostrar mensaje genérico
            if (user == null)
            {
                ModelState.AddModelError("", "Correo electrónico o contraseña incorrectos.");
                return View(model);
            }

            // Verificar si la cuenta está bloqueada
            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                var lockoutEnd = await UserManager.GetLockoutEndDateAsync(user.Id);
                var tiempoRestante = lockoutEnd.ToLocalTime().Subtract(DateTimeOffset.Now);
                if (tiempoRestante.TotalSeconds > 0)
                {
                    int minutosRestantes = (int)Math.Ceiling(tiempoRestante.TotalMinutes);
                    ModelState.AddModelError("", $"Su cuenta ha sido bloqueada debido a múltiples intentos fallidos de inicio de sesión. Por favor, intente nuevamente en {minutosRestantes} minuto(s).");
                    return View(model);
                }
            }

            // Verificar la contraseña
            var passwordValid = await UserManager.CheckPasswordAsync(user, model.Password);
            
            if (passwordValid)
            {
                // Contraseña correcta - resetear contador de intentos fallidos
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                
                // Iniciar sesión
                await SignInManager.SignInAsync(user, model.RememberMe, model.RememberMe);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // Contraseña incorrecta - incrementar contador de intentos fallidos
                await UserManager.AccessFailedAsync(user.Id);
                
                // Obtener el número actual de intentos fallidos
                int intentosFallidos = await UserManager.GetAccessFailedCountAsync(user.Id);
                int intentosRestantes = 5 - intentosFallidos;
                
                // Si alcanzó los 5 intentos, bloquear la cuenta
                if (intentosFallidos >= 5)
                {
                    // Bloquear la cuenta por 5 minutos
                    await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.UtcNow.AddMinutes(5));
                    ModelState.AddModelError("", "Su cuenta ha sido bloqueada debido a múltiples intentos fallidos de inicio de sesión. Por favor, intente nuevamente en 5 minuto(s).");
                }
                else if (intentosRestantes <= 2)
                {
                    // Mostrar advertencia cuando quedan 2 intentos o menos
                    ModelState.AddModelError("", $"Correo electrónico o contraseña incorrectos. Le quedan {intentosRestantes} intento(s) antes de que su cuenta sea bloqueada.");
                }
                else
                {
                    // Mensaje genérico para los primeros intentos
                    ModelState.AddModelError("", "Correo electrónico o contraseña incorrectos.");
                }
                
                return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Requerir que el usuario haya iniciado sesión con nombre de usuario y contraseña o inicio de sesión externo
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // El código siguiente protege de los ataques por fuerza bruta a los códigos de dos factores. 
            // Si un usuario introduce códigos incorrectos durante un intervalo especificado de tiempo, la cuenta del usuario 
            // se bloqueará durante un período de tiempo especificado. 
            // Puede configurar el bloqueo de la cuenta en IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Habilitar bloqueo de cuenta para este usuario
                    await UserManager.SetLockoutEnabledAsync(user.Id, true);
                    
                    // Asignar automáticamente el rol "Estudiante" a todos los usuarios nuevos
                    await UserManager.AddToRoleAsync(user.Id, "Estudiante");
                    
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                    // Enviar un correo electrónico con este vínculo
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirmar la cuenta", "Para confirmar su cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar un correo electrónico con este vínculo
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Solicitar redireccionamiento al proveedor de inicio de sesión externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generar el token y enviarlo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Si el usuario ya tiene un inicio de sesión, iniciar sesión del usuario con este proveedor de inicio de sesión externo
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Si el usuario no tiene ninguna cuenta, solicitar que cree una
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Obtener datos del usuario del proveedor de inicio de sesión externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // Habilitar bloqueo de cuenta para este usuario
                    await UserManager.SetLockoutEnabledAsync(user.Id, true);
                    
                    // Asignar automáticamente el rol "Estudiante" a usuarios registrados externamente
                    await UserManager.AddToRoleAsync(user.Id, "Estudiante");
                    
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        //
        // GET: /Account/RecuperarContrasena
        [AllowAnonymous]
        public ActionResult RecuperarContrasena()
        {
            return View();
        }

        //
        // POST: /Account/RecuperarContrasena
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecuperarContrasena(RecuperarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                ViewBag.Mensaje = "Si el correo existe en nuestro sistema, recibirás un email con una contraseña temporal.";
                return View("RecuperarContrasenaConfirmacion");
            }

            // Generar contraseña temporal
            string contrasenaTemporal = GenerarContrasenaTemporal();

            // Guardar registro de recuperación en la base de datos
            using (var db = new ApplicationDbContext())
            {
                var recuperacion = new RecuperacionContrasena
                {
                    Id = user.Id,
                    Token = contrasenaTemporal,
                    Expira = DateTime.Now.AddHours(24),
                    Usado = false,
                    FechaCreacion = DateTime.Now
                };
                db.RecuperacionContrasena.Add(recuperacion);
                await db.SaveChangesAsync();
            }

            // Enviar email con contraseña temporal
            string subject = "Recuperación de Contraseña - Escuela Félix Arcadio";
            string body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #1e3a8a; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>Escuela Félix Arcadio Montero Monge</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f8fafc;'>
                        <h2 style='color: #1e3a8a;'>Recuperación de Contraseña</h2>
                        <p>Hola,</p>
                        <p>Has solicitado recuperar tu contraseña. Tu contraseña temporal es:</p>
                        <div style='background-color: #fff; padding: 15px; border-left: 4px solid #1e3a8a; margin: 20px 0;'>
                            <h3 style='margin: 0; color: #1e3a8a; font-size: 24px; letter-spacing: 2px;'>{contrasenaTemporal}</h3>
                        </div>
                        <p><strong>Importante:</strong></p>
                        <ul>
                            <li>Esta contraseña temporal expira en 24 horas</li>
                            <li>Debes cambiarla por una nueva contraseña después de iniciar sesión</li>
                            <li>Si no solicitaste este cambio, ignora este correo</li>
                        </ul>
                        <p style='margin-top: 30px;'>
                            <a href='{Url.Action("CambiarContrasenaTemporal", "Account", null, Request.Url.Scheme)}' 
                               style='background-color: #1e3a8a; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Cambiar Contraseña
                            </a>
                        </p>
                    </div>
                    <div style='background-color: #e2e8f0; padding: 15px; text-align: center; color: #64748b;'>
                        <p style='margin: 0; font-size: 12px;'>Escuela Félix Arcadio Montero Monge - {DateTime.Now.Year}</p>
                    </div>
                </div>";

            await UserManager.SendEmailAsync(user.Id, subject, body);

            ViewBag.Mensaje = "Se ha enviado un correo con tu contraseña temporal. Por favor revisa tu bandeja de entrada.";
            return View("RecuperarContrasenaConfirmacion");
        }

        //
        // GET: /Account/CambiarContrasenaTemporal
        [AllowAnonymous]
        public ActionResult CambiarContrasenaTemporal()
        {
            return View();
        }

        //
        // POST: /Account/CambiarContrasenaTemporal
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarContrasenaTemporal(CambiarContrasenaTemporal model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View(model);
            }

            // Verificar la contraseña temporal en la base de datos
            using (var db = new ApplicationDbContext())
            {
                var recuperacion = db.RecuperacionContrasena
                    .Where(r => r.Id == user.Id && r.Token == model.ContrasenaTemporal && !r.Usado && r.Expira > DateTime.Now)
                    .OrderByDescending(r => r.FechaCreacion)
                    .FirstOrDefault();

                if (recuperacion == null)
                {
                    ModelState.AddModelError("", "La contraseña temporal es inválida o ha expirado.");
                    return View(model);
                }

                // Cambiar la contraseña
                var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var result = await UserManager.ResetPasswordAsync(user.Id, token, model.NuevaContrasena);

                if (result.Succeeded)
                {
                    // Marcar el token como usado
                    recuperacion.Usado = true;
                    await db.SaveChangesAsync();

                    ViewBag.Mensaje = "Tu contraseña ha sido cambiada exitosamente. Ahora puedes iniciar sesión con tu nueva contraseña.";
                    return View("CambiarContrasenaConfirmacion");
                }

                AddErrors(result);
            }

            return View(model);
        }

        // Método auxiliar para generar contraseña temporal
        private string GenerarContrasenaTemporal()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Aplicaciones auxiliares
        // Se usa para la protección XSRF al agregar inicios de sesión externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}