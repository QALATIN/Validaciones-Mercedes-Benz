
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;



namespace ProcesamientoDeValidaciones.Models
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
        public virtual ICollection<Validacion> Validaciones { get; set; }
        public int SolicitanteId { get; set; }


        public int UsuarioId { get; set; }

        public int AgenciaId { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; }
        public string CorreoElectronico { get; set; }

        public string Estatus { get; set; }

        public string Sexo { get; set; }

        public string LugarNacimiento { get; set; }

        public string Nacionalidad { get; set; }

        public DateTime FechaEnvio { get; set; }
        public DateTime FechaRegistro { get; set; }

        public string NombreCompletoSolicitante { get; set; }


    }
}
