using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Utilidades.IDScan
{
    public class IDScanVerify
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public DocumentType Tipo { get; set; }

        /// <summary>
        /// Nombre de la persona del documento digitalizado
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Nombre { get; set; }

        /// <summary>
        /// Dirección postal del documento
        /// </summary>
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Direccion { get; set; }

        /// <summary>
        /// Calificación de las validaciones del documento
        /// </summary>
        [JsonProperty("qualification", NullValueHandling = NullValueHandling.Ignore)]
        public int Calificacion { get; set; }

        /// <summary>
        /// Referencia de la imagen que se puede utilizar dentro del servicio GetImage 
        /// </summary>
        [JsonProperty("imageReference", NullValueHandling = NullValueHandling.Ignore)]
        public string ImagenID { get; set; }

        /// <summary>
        /// Fecha de envío del documento
        /// </summary>
        [JsonProperty("dateIssue", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime FechaEnvio { get; set; }

        /// <summary>
        /// Lista de objetos con las validaciones del documento
        /// </summary>
        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
        public List<SectionDescription> Detalles { get; set; }

        /// <summary>
        /// Información extra de la hoja, contiene info del código de barras en recibo de telmex
        /// </summary>
        [JsonProperty("extraData", NullValueHandling = NullValueHandling.Ignore)]
        public string DatoExtra { get; set; }

        /// <summary>
        /// Lectura OCR de cada hoja del documento
        /// </summary>
        [JsonProperty("ocr", NullValueHandling = NullValueHandling.Ignore)]
        public string OCR { get; set; }
    }
    public class DocumentType
    {
        private TipoDocumento _tipoDocumento = TipoDocumento.NINGUNO;
        /// <summary>
        /// Establece el tipo de comprobante de domicilio
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string typeString
        {
            set
            {
                _tipoDocumento = Ti(value);
            }
        }

        /// <summary>
        /// Devuelve enumeración del tipo de comprobante
        /// </summary>
        public TipoDocumento TipoComprobante
        {
            get
            {
                return _tipoDocumento;
            }
        }

        /// <summary>
        /// Clasificacion del comprobante de domicilio
        /// </summary>
        [JsonProperty("classification", NullValueHandling = NullValueHandling.Ignore)]
        public string Clasificacion { get; set; }


        private TipoDocumento Ti(string valor)
        {
            switch (valor.ToUpper())
            {
                case "CFE": return TipoDocumento.CFE;
                case "TELMEX": return TipoDocumento.TELMEX;
                case "BANAMEX": return TipoDocumento.BANAMEX;
                case "HSBC": return TipoDocumento.HSBC;
                case "SCOTIABANK": return TipoDocumento.SCOTIABANK;
                case "SANTANDER": return TipoDocumento.SANTANDER;
                case "BANORTE": return TipoDocumento.BANORTE;
                case "BBVA": return TipoDocumento.BBVA;
                case "BBVA QR": return TipoDocumento.BBVA_QR;
                case "BBVA IPAB": return TipoDocumento.BBVA_IPAB;
                case "DESCONOCIDO": return TipoDocumento.Desconocido;
                case "ERROR": return TipoDocumento.Error;
                case "ACTA DE NACIMIENTO": return TipoDocumento.ACTA_DE_NACIMIENTO;
                case "LINEA DE CAPTURA OAXACA": return TipoDocumento.LINEA_DE_CAPTURA_OAXACA;
                case "COMPROBANTE EXAMEN OAXACA": return TipoDocumento.COMPROBANTE_EXAMEN_OAXACA;
                default: return TipoDocumento.NINGUNO;
            }
        }
    }

    public class SectionDescription
    {
        /// <summary>
        /// Tipo de detalle de la validación realizada
        /// </summary>
        [JsonProperty("detailType", NullValueHandling = NullValueHandling.Ignore)]
        public string TipoDetalle { get; set; }

        //Calificación de la validación
        [JsonProperty("qualification", NullValueHandling = NullValueHandling.Ignore)]
        public int Calificacion { get; set; }

        /// <summary>
        /// Detalle de la validación realizada
        /// </summary>
        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detalle { get; set; }

        /// <summary>
        /// Sección 1 a comparar para la validación
        /// </summary>
        [JsonProperty("section1", NullValueHandling = NullValueHandling.Ignore)]
        public string Seccion1 { get; set; }

        /// <summary>
        /// Sección 2 de la validación a comparar por lo general es el número del código de barras.
        /// </summary>
        [JsonProperty("section2", NullValueHandling = NullValueHandling.Ignore)]
        public string Seccion2 { get; set; }
    }

    public enum TipoDocumento
    {

        CFE = 1,
        TELMEX = 2,
        BANAMEX = 3,
        HSBC = 4,
        SCOTIABANK = 5,
        SANTANDER = 6,
        BANORTE = 7,
        BBVA = 8,
        BBVA_QR = 9,
        BBVA_IPAB = 10,
        Desconocido = 11,
        Error = 12,
        ACTA_DE_NACIMIENTO = 13,
        LINEA_DE_CAPTURA_OAXACA = 1001,
        COMPROBANTE_EXAMEN_OAXACA = 1002,
        NINGUNO = -1
    }
}
