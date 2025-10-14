using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class RolInitialize
    {
        public static void Inicializar()
        {
            var rolManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            //Roles 
            List<String> roles = new List<String>();
            roles.Add("Administrador");
            roles.Add("Docente");
            roles.Add("Estudiante");

            foreach (var role in roles)
            {
                if (!rolManager.RoleExists(role))
                {
                    rolManager.Create(new IdentityRole(role));
                }
            }

            //usuario por defecto
            var adminUser = new ApplicationUser { Email = "Admin.EscuelaFelixArcadio@gmail.com" };
            String contra = "EscuelaFelix01$";

            if (userManager.FindByEmail(adminUser.Email) == null)
            {
                var creacion = userManager.Create(adminUser, contra);
                if (creacion.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Administrador");
                }
            }
        }

    }
}