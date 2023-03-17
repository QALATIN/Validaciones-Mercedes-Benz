using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Tmpreportelistanegra
    {
        public byte[] Clienteimagen { get; set; }
        public string Clientenombre { get; set; }
        public string Folio { get; set; }
        public string Contrato { get; set; }
        public string Motivo { get; set; }
        public DateTime? Fechaingreso { get; set; }
        public string Agencianombre { get; set; }
    }
}
