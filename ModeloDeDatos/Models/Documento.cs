using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Documento
    {
        public int DocumentoId { get; set; }
        public int SolicitanteId { get; set; }
        public string TipoDocumento { get; set; }
        public byte[] Imagen { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }
        public string NombreDocumento { get; set; }

        public virtual Solicitante Solicitante { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
