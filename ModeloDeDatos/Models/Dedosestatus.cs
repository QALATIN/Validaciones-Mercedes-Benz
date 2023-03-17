using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Dedosestatus
    {
        public Dedosestatus()
        {
            Huellas = new HashSet<Huella>();
        }

        public int DedoEstatusId { get; set; }
        public string DedoEstatusNombre { get; set; }
        public string DedoEstatusClave { get; set; }

        public virtual ICollection<Huella> Huellas { get; set; }
    }
}
