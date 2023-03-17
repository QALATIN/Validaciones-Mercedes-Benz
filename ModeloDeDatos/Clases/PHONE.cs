using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ModeloDeDatos.Clases
{



    [Serializable]
    public class PHONE
    {
        [JsonProperty("message")]
        public string Mensaje { get; set; }


        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("Code")]
        public int Codigo { get; set; }

        [JsonProperty("responseData")]
        public DataTelefono Respuesta {get;set;}

        public class DataTelefono
        {
            [JsonProperty("numeroIdentificadorRegion")]
            public string IdentificacionRegional { get; set; }

            [JsonProperty("tipoRed")]
            public string TipoDeRed { get; set; }

            [JsonProperty("ciudad")]
            public string Ciudad { get; set; }

            [JsonProperty("marcacionNacional")]
            public string NumeroTelefonico { get; set; }

            [JsonProperty("marcacionEU")]
            public string MarcacionEstadosUnidos { get; set; }

            [JsonProperty("marcacionInt")]
            public string MarcacionInternacional { get; set; }

            [JsonProperty("proveedor")]
            public string Proveedor { get; set; }

        }
    }
  



}
