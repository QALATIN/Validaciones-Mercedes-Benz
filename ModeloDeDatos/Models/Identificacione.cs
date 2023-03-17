using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Identificacione
    {
        public int IdentificacionId { get; set; }
        public int SolicitanteId { get; set; }
        public string Serie { get; set; }
        public string NumeroEmision { get; set; }
        public string Cic { get; set; }
        public string Ocr { get; set; }
        public string ClaveElector { get; set; }
        public string IdentificadorCiudadano { get; set; }
        public string Vigencia { get; set; }
        public string AnioRegistro { get; set; }
        public string Emision { get; set; }
        public string Mrz { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }

        public string TipoIne { get; set; }

        public virtual Solicitante Solicitante { get; set; }
    }
}
