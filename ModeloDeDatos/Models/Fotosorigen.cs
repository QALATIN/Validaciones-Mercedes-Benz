using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Fotosorigen
    {
        public Fotosorigen()
        {
            Fotos = new HashSet<Foto>();
        }

        public int FotoOrigenId { get; set; }
        public string FotoOrigenNombre { get; set; }

        public virtual ICollection<Foto> Fotos { get; set; }
    }
}
