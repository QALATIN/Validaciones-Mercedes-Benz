using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Dedosindie
    {
        public Dedosindie()
        {
            Huellas = new HashSet<Huella>();
        }

        public int DedoIndiceId { get; set; }
        public string DedoIndiceNombre { get; set; }

        public virtual ICollection<Huella> Huellas { get; set; }
    }
}
