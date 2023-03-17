using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Tmpbusqueda
    {
        public int Paqueteid { get; set; }
        public string Folio { get; set; }
        public string Contrato { get; set; }
        public DateTime? Fechaenvio { get; set; }
        public string Nombre { get; set; }
        public string Apellidopaterno { get; set; }
        public string Apellidomaterno { get; set; }
        public string Curp { get; set; }
        public string Claveagencia { get; set; }
        public string Nombreagencia { get; set; }
        public string Listanegra { get; set; }
    }
}
