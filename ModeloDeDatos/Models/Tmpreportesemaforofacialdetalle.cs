using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Tmpreportesemaforofacialdetalle
    {
        public string Tipoalerta { get; set; }
        public float Scorefacial { get; set; }
        public string Clientenombre { get; set; }
        public string Folio { get; set; }
        public string Contrato { get; set; }
        public DateTime? Fechacaptura { get; set; }
        public DateTime? Fechaenvio { get; set; }
        public string Usuarioenrolador { get; set; }
        public string Agencianombre { get; set; }
    }
}
