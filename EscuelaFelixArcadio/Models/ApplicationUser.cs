using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Tenga en cuenta que authenticationType debe coincidir con el valor definido en CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Agregar reclamaciones de usuario personalizadas aquí
            return userIdentity;
        }

        public int IntentosFallidos { get; set; } = 0;

        public bool EstaBloqueado { get; set; } = false;

        public DateTime? FechaBloqueo { get; set; }

        // Relación con tablas 
        public ICollection<Documento> Documento { get; set; } = new List<Documento>();
        public ICollection<MantenimientoEspacio> MantenimientoEspacio { get; set; } = new List<MantenimientoEspacio>();
        public ICollection<MovimientoInventario> MovimientoInventario { get; set; } = new List<MovimientoInventario>();

        public ICollection<Prestamo> Prestamo { get; set; } = new List<Prestamo>();
        public ICollection<RecuperacionContrasena> RecuperacionContrasena { get; set; } = new List<RecuperacionContrasena>();
        public ICollection<Reserva> Reserva { get; set; } = new List<Reserva>();
        public ICollection<ReservaEspacio> ReservaEspacio { get; set; } = new List<ReservaEspacio>();
        public ICollection<Sancion> Sancion { get; set; } = new List<Sancion>();
    }

}