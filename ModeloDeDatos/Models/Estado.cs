using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Estado
    {
        public Estado()
        {
            Agencia = new HashSet<Agencia>();
        }

        public short EstadoId { get; set; }
        public string NombreEstado { get; set; }

        public virtual ICollection<Agencia> Agencia { get; set; }
    }
}
