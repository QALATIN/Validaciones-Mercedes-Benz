using System;
using System.Collections.Generic;

#nullable disable

namespace ModeloDeDatos.Models
{
    public partial class Tmpreportesemaforo
    {
        public string Agencia { get; set; }
        public string Folio { get; set; }
        public string Contrato { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime Fechaenvio1 { get; set; }
        public TimeSpan Horaenvio { get; set; }
        public DateTime FechaCaptura { get; set; }
        public DateTime Fechacaptura1 { get; set; }
        public TimeSpan Horacaptura { get; set; }
        public string Huellas { get; set; }
        public string Foto { get; set; }
        public string Semaforoassure { get; set; }
        public string Semaforolatin { get; set; }
        public string Semaforogeneral { get; set; }
        public string Semaforodelistanegra { get; set; }
        public string Semaforodedocumento { get; set; }
        public string Semaforodehuellas { get; set; }
        public string Semaforodefolioynombre { get; set; }
        public string Semaforodeconsultaweb { get; set; }
        public string Semaforodecomparacionfacial { get; set; }
        public string Tipodeidentificacion { get; set; }
    }
}
