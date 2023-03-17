using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Capturasnombre
    {
        public Capturasnombre()
        {
            Capturasidentificacions = new HashSet<Capturasidentificacion>();
        }

        public int CapturaNombreId { get; set; }
        public string CapturaNombre { get; set; }

        public virtual ICollection<Capturasidentificacion> Capturasidentificacions { get; set; }
    }
}
