using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Agenciastipo
    {
        public short TipoAgenciaId { get; set; }
        public string TipoAgenciaNombre { get; set; }
        public bool? Activo { get; set; }
    }
}
