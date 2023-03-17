using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Tmpreportedetalleenvio
    {
        public string Agencianombre { get; set; }
        public string Clientenombre { get; set; }
        public string Contrato { get; set; }
        public DateTime? Fechacaptura { get; set; }
        public DateTime? Fechaenvio { get; set; }
        public string Tipodocumento { get; set; }
        public string Tipoalerta { get; set; }
        public string Inerespuesta { get; set; }
        public string Revisionanalista { get; set; }
    }
}
