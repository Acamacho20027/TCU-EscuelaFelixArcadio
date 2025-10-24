using System;

namespace EscuelaFelixArcadio.Models
{
    public class ArchivoExportacion
    {
        public string NombreArchivo { get; set; }
        public string TipoMIME { get; set; }
        public byte[] Contenido { get; set; }
        public int Tamaño { get; set; }
    }
}
