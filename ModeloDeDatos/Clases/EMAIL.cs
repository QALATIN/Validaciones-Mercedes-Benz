using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModeloDeDatos.Clases
{


    public class EMAIL
    {
        private string _hex_rgb = "#919191"; //gris
        private string _leyenda_score = string.Empty;

        private int _score = -1;

        [JsonIgnore]
        public int Score { get { return _score; } }

        [JsonProperty("eaScore")]
        public string LeyendaScore {
            get { return _leyenda_score; }
            set
            {
                if (!int.TryParse(value, out _score))
                {
                    _leyenda_score = "Correo no procesado.";
                    _hex_rgb = "#919191";
                }
                else
                {
                    if (_score >= 0 && _score <= 100)
                    {//verde
                        _leyenda_score = "Riesgo de fraude muy bajo.";
                        _hex_rgb = "#2D572C"; //verde bandera
                    }
                    else if (_score >= 101 && _score <= 300)
                    {//verde
                        _leyenda_score = "Riesgo de fraude bajo.";
                        _hex_rgb = "#9acc81";//verde tenue
                    }
                    else if (_score >= 301 && _score <= 600)
                    {//amarillo
                        _leyenda_score = "Riesgo de fraude moderado.";
                        _hex_rgb = "#efe096";//amarillo tenue
                    }
                    else if (_score >= 601 && _score <= 799)
                    {//amarillo
                        _leyenda_score = "Riesgo de fraude moderado."; 
                        _hex_rgb = "#E5BE01";//amarillo intenso
                    }
                    else if (_score >= 800 && _score <= 899)
                    {//rojo
                        _leyenda_score = "Riesgo de fraude alto.";
                        _hex_rgb = "#e65050";//rojo tenue
                    }
                    else if (_score >= 900 && _score <= 1000)
                    {//rojo
                        _leyenda_score = "Riesgo de fraude muy alto.";
                        _hex_rgb = "#ab0000";//rojo intenso
                    }
                    else
                    {
                        _leyenda_score = "Correo no procesado."; 
                        _hex_rgb = "#919191";//gris
                    }
                }

            }
        }

        [JsonIgnore]
        public string HexRgbScore
        {
            get { return _hex_rgb; }
        }

        [JsonProperty("eaAdvice")] //riesgo moderado de fraude | riesgo de fraude bajo
        public string Mensaje { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("eaReason")]//puede encapsular el tiempo de creación del correo o una leyenda de fraude
        public string DatoCurioso { get; set; }
  
      
        [JsonProperty("source_industry")] //industria que hizo la validación
        public string UbicacionFraude { get; set; }
  
        [JsonProperty("fraud_type")] //tipo de fraude en ingles 
        public string TipoDeFraudeEn { get; set; }
    
        //información referente al dominio

        [JsonProperty("domainname")] //@...
        public string Dominio { get; set; }

        [JsonProperty("domaincountryname")] //@...
        public string DominioUbicacion { get; set; }

        [JsonProperty("domaincompany")] //@...
        public string DominioEmpresa { get; set; }



    }
}
