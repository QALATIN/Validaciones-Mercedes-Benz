
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


#nullable disable

namespace ModeloDeDatos.Models
{
    [Serializable()]
    public partial class Solicitante
    {
        public Solicitante()
        {
            //Avisosprivacidads = new HashSet<Avisosprivacidad>();
            //Capturasidentificacions = new HashSet<Capturasidentificacion>();
            //Documentos = new HashSet<Documento>();
            //Fotos = new HashSet<Foto>();
            //Huellas = new HashSet<Huella>();
            //Identificaciones = new HashSet<Identificacione>();
            //Listanegras = new HashSet<Listanegra>();
            //Resolucions = new HashSet<Resolucion>();
            //Semaforos = new HashSet<Semaforo>();
            Validaciones = new HashSet<Validacion>();
        }

        public int SolicitanteId { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public int SolicitanteIdOtro { get; set; }

        public int UsuarioId { get; set; }

        public int AgenciaId { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; }
        public string CorreoElectronico { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string TipoCliente { get; set; }
        public string Estatus { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string Folio { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string TipoDocumento { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string NumeroDocumento { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public Guid? Guid { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string Telefono { get; set; }
        public string Sexo { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string ResultadoGeneral { get; set; }
        public string LugarNacimiento { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string CoordenadasGps { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string ScoreComparacionFacial { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string ResultadoComparacionFacial { get; set; }
        public string Nacionalidad { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string DireccionCompleta { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string CodigoPostal { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string CalleNumero { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string Colonia { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string Municipio { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string Estado { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string PruebaVida { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public string Edad { get; set; }

        public string NombreCompletoSolicitante { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime FechaRegistro { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public DateTime? FechaBaja { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public bool Activo { get; set; }
     
        [JsonIgnore]
        [IgnoreDataMember]
     
        public virtual Agencia Agencia { get; set; }
        
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual Usuario Usuario { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<Avisosprivacidad> Avisosprivacidads { get; set; }

        [JsonIgnore]
        [IgnoreDataMember] 
        public virtual ICollection<Capturasidentificacion> Capturasidentificacions { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<Documento> Documentos { get; set; }
    
        [JsonIgnore]
        [IgnoreDataMember] 
        public virtual ICollection<Foto> Fotos { get; set; }
   
        [JsonIgnore]
        [IgnoreDataMember] 
        public virtual ICollection<Huella> Huellas { get; set; }

        [JsonIgnore]
        [IgnoreDataMember] 
        public virtual ICollection<Identificacione> Identificaciones { get; set; }
    
        [JsonIgnore]
        [IgnoreDataMember] 
        public virtual ICollection<Listanegra> Listanegras { get; set; }
     
        [JsonIgnore]
        [IgnoreDataMember] 
        public virtual ICollection<Resolucion> Resolucions { get; set; }
    
        [JsonIgnore]
        [IgnoreDataMember] 
        public virtual ICollection<Semaforo> Semaforos { get; set; }
   
        [JsonIgnore]
        //[IgnoreDataMember] 
        public virtual ICollection<Validacion> Validaciones { get; set; }
    }
}
