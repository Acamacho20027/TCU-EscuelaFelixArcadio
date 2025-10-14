using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class ApplicationRol: IdentityRole
    {
        public string Descripcion { get; set; }

        public ApplicationRol() : base() { }

    }
}