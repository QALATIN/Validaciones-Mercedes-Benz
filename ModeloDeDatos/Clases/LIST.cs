using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ModeloDeDatos.Clases
{
    [Serializable]
    public class LIST
    {

        public LIST()
        {
            _listaInteres = new List<DataList>();
        }

        [JsonProperty("message")]
        public string Mensaje { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        private List<DataList> _listaInteres = null;
        [JsonProperty("responseData")]
        public List<DataList> Respuesta 
        {
            get
            {
                return _listaInteres;
            } 
            set
            {
                _listaInteres = value;
            }
        }


        public class DataList
        {
            [JsonProperty("lista")]
            public string Lista { get; set; }

            [JsonProperty("pais_Lista")]
            public string PaisLista { get; set; }

            [JsonProperty("enlace")]
            public string Url { get; set; }

            [JsonProperty("exactitud_Denominacion")]
            public string Denominacion { get; set; }



        }
    }
}
