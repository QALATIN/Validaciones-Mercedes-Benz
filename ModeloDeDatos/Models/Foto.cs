using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Foto
    {
        public int FotoId { get; set; }
        public int SolicitanteId { get; set; }
        public byte[] Imagen { get; set; }
        public int FotoOrigenId { get; set; }
        public Guid? Guid { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaBaja { get; set; }
        public bool Activo { get; set; }

        public virtual Fotosorigen FotoOrigen { get; set; }
        public virtual Solicitante Solicitante { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
