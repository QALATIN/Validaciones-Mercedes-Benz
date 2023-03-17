using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Capturasidentificacion
    {
        public int CapturaIdentificacionId { get; set; }
        public int SolicitanteId { get; set; }
        public byte[] Imagen { get; set; }
        public int CapturaNombreId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }

        public virtual Capturasnombre CapturaNombre { get; set; }
        public virtual Solicitante Solicitante { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
