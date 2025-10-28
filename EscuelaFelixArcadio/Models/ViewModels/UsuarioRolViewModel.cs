using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models.ViewModels
{
    public class UsuarioRolViewModel
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string RolActual { get; set; }
        public string RolSeleccionado { get; set; }
        public List<string> RolesDisponibles { get; set; }
    }
}

