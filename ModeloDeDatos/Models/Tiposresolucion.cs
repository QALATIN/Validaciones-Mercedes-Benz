using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Tiposresolucion
    {
        public Tiposresolucion()
        {
            Resolucions = new HashSet<Resolucion>();
        }

        public int TipoResolucionId { get; set; }
        public string TipoResolucionNombre { get; set; }

        public virtual ICollection<Resolucion> Resolucions { get; set; }
    }
}
