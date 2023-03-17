using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Resolucion
    {
        public int ResolucionId { get; set; }
        public int SolicitanteId { get; set; }
        public string Comentario { get; set; }
        public int TipoResolucionId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime? FechaRegistro { get; set; }

        public virtual Solicitante Solicitante { get; set; }
        public virtual Tiposresolucion TipoResolucion { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
