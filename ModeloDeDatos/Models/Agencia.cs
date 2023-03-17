using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Agencia
    {
        public Agencia()
        {
            Solicitantes = new HashSet<Solicitante>();
            Usuarios = new HashSet<Usuario>();
        }

        public int AgenciaId { get; set; }
        public string ClaveAgencia { get; set; }
        public string NombreAgencia { get; set; }
        public string Telefono { get; set; }
        public short EstadoId { get; set; }
        public string Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaBaja { get; set; }
        public int? TipoAgenciaId { get; set; }

        public virtual Estado Estado { get; set; }
        public virtual ICollection<Solicitante> Solicitantes { get; set; }
        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
