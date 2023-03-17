using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Huella
    {
        public int HuellaId { get; set; }
        public int SolicitanteId { get; set; }
        public int DedoIndiceId { get; set; }
        public int DedoEstatusId { get; set; }
        public byte[] Imagen { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }

        public virtual Dedosestatus DedoEstatus { get; set; }
        public virtual Dedosindie DedoIndice { get; set; }
        public virtual Solicitante Solicitante { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
