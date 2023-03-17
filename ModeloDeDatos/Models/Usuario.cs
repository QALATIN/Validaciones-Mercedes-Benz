using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    [Serializable]
    [JsonObject(IsReference = true)]

    public partial class Usuario
    {
        public Usuario()
        {
            Avisosprivacidads = new HashSet<Avisosprivacidad>();
            Capturasidentificacions = new HashSet<Capturasidentificacion>();
            Documentos = new HashSet<Documento>();
            Fotos = new HashSet<Foto>();
            Huellas = new HashSet<Huella>();
            Listanegras = new HashSet<Listanegra>();
            Resolucions = new HashSet<Resolucion>();
            Solicitantes = new HashSet<Solicitante>();
            Validaciones = new HashSet<Validacion>();
        }

        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public int PerfilId { get; set; }
        public int AgenciaId { get; set; }
        public string Password { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaBaja { get; set; }
        public string Token { get; set; }
        public DateTime? TokenVigencia { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string CorreoElectronico { get; set; }
        public bool? ActivarCuenta { get; set; }

        public virtual Agencia Agencia { get; set; }
        public virtual Perfile Perfil { get; set; }
        public virtual ICollection<Avisosprivacidad> Avisosprivacidads { get; set; }
        public virtual ICollection<Capturasidentificacion> Capturasidentificacions { get; set; }
        public virtual ICollection<Documento> Documentos { get; set; }
        public virtual ICollection<Foto> Fotos { get; set; }
        public virtual ICollection<Huella> Huellas { get; set; }
        public virtual ICollection<Listanegra> Listanegras { get; set; }
        public virtual ICollection<Resolucion> Resolucions { get; set; }
        public virtual ICollection<Solicitante> Solicitantes { get; set; }
        public virtual ICollection<Validacion> Validaciones { get; set; }
    }
}
