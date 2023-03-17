using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ModeloDeDatos.Clases
{

    public class INE
    {
        [JsonProperty("code")]

        public int CodigoError { get; set; }

        [JsonProperty("success")]
        public bool EsCorrecto { get; set; }

        [JsonProperty("message")]
        public string Mensaje { get; set; }

        [JsonProperty("responseData")]
        public DataIne ResponseData { get; set; }

        public class DataIne
        {
            private string _msg = string.Empty;
            [JsonProperty("mensaje")]
            public string MsgMensaje
            {
                set { _msg = value; }
            }

            [JsonProperty("codigoValidacion")]
            public string CodigoDeValidacion { get; set; }


            private string _mensajeFinal = string.Empty;

            [JsonIgnore]
            public string MensajeFinal
            {
                get
                {
                    if (string.IsNullOrEmpty(_msg))
                        return _infoAdicional;
                    else if (string.IsNullOrEmpty(_infoAdicional))
                        return _msg;
                    else
                        return string.Format("{0}, {1}", _msg, _infoAdicional);
                }
            }

            private string _infoAdicional = string.Empty;
            [JsonProperty("informacionAdicional")]
            public string MsgAdicional 
            { 
                set { _infoAdicional = value; } 
            }

            [JsonProperty("claveMensaje")]
            public string Clave { get; set; }



            [JsonProperty("claveElector")]
            public string ClaveDeElector { get; set; }


            [JsonProperty("estatus")]
            public string EstatusRespuesta { get; set; }



            [JsonProperty("numeroEmision")]
            public string NumeroDeEmision { get; set; }

            [JsonProperty("anioRegistro")]
            public string Registro { get; set; }

            [JsonProperty("anioEmision")]
            public string Emision { get; set; }

            [JsonProperty("vigencia")]
            public string FechaVigencia { get; set; }

            [JsonProperty("ocr")]
            public string Ocr{ get; set; }


        }
    }
}
