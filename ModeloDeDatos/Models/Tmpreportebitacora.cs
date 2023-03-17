using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Tmpreportebitacora
    {
        public byte[] Clienteimagen { get; set; }
        public string Clientenombre { get; set; }
        public string Folio { get; set; }
        public string Contrato { get; set; }
        public string Eliminacionmotivo { get; set; }
        public string Tieneafis { get; set; }
        public DateTime? Eliminacionfecha { get; set; }
        public string Eliminacionusuario { get; set; }
        public string Agencianombre { get; set; }
    }
}
