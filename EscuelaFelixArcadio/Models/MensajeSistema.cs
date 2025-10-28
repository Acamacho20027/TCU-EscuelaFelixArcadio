using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EscuelaFelixArcadio.Models
{
    public class MensajeSistema
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Contenido { get; set; }
        public string Tipo { get; set; } // Info, Advertencia, Error, Exito
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Activo { get; set; }
        public string RolDestino { get; set; } // null = todos los roles
        public DateTime FechaCreacion { get; set; }
        public string IdUsuarioCreacion { get; set; }

        public MensajeSistema()
        {
            FechaCreacion = DateTime.Now;
            Activo = true;
        }
    }
}

