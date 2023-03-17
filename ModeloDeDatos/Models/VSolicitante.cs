using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class VSolicitante
    {
        public int? Solicitanteid { get; set; }
        public string Folio { get; set; }
        public string Nombre { get; set; }
        public string Apellidopaterno { get; set; }
        public string Apellidomaterno { get; set; }
        public string Nombrecompleto { get; set; }
        public string Tipocliente { get; set; }
        public string Correoelectronico { get; set; }
        public string Listanegra { get; set; }
        public DateTime? Fechaenvio { get; set; }
        public string Estadosolicitud { get; set; }
    }
}
