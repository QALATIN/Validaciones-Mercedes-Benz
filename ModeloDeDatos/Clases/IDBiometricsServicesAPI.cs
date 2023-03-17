using log4net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace ModeloDeDatos.Clases
{
    /// <summary>
    /// Enumeración para el tipo de validación: 1: (verde - OK) | 2: (amarillo - AVISO) | -1: (rojo - ALERTA) | 0: (SIN_ALERTA, no inicializado) 
    /// </summary>
    public enum TipoAlerta
    {
        OK = 0,
        AVISO = 1,
        ALERTA = 2,
        SIN_ALERTA = -1
    }

    public enum TipoValidacion
    {
        CURP = 1,
        CORREO = 2,
        TELEFONO = 3,
        LISTA_INTERES = 4,
        COMPROBANTE_DOMICILIO = 5,
        COMPROBANTE_BANCARIO = 6,
        COMPROBANTE_INGRESOS = 7,
        INE = 8,
        GEOREFERENCIA = 9
    }

    [Serializable]
    public class APIValidacion
    {
        [JsonProperty("alerta", NullValueHandling = NullValueHandling.Ignore)]
        public int? TipoAlerta
        {
            get { return (int)Alerta; }
            set { Alerta = (TipoAlerta)value; }
        }

        [JsonIgnore]
        public TipoAlerta Alerta { get; set; }

        [JsonProperty("mensaje", NullValueHandling = NullValueHandling.Ignore)]
        public string Mensaje { get; set; }

        public APIValidacion()
        {
            Alerta = Clases.TipoAlerta.SIN_ALERTA;
            Mensaje = string.Empty;

        }
        public APIValidacion(TipoAlerta tipo, string msg)
        {
            Alerta = tipo;
            Mensaje = msg;
        }
    }

    public class APIModel
    {
        public bool result { get; set; }
        public string message { get; set; }
        public string token { get; set; }
        public string tokenType { get; set; }
        public string apiKey { get; set; }
        public DateTime dateIssue { get; set; }

        public long TiempoEspera { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", tokenType, token);
        }
    }

    public class MensajeWebService
    {
        public bool HasError { get; set; }
        public int Error { get; set; }
        public string Data { get; set; }
        public string Mensaje { get; set; }
        public string Excepcion { get; set; }
    }

    public class IDBiometricsServicesAPI
    {
        public enum Tipo
        {
            CheckAPI,
            DigiAPI,
            BioAPI,
            Ninguna,
        }

        public enum Operacion
        {
            [Description("ScanningValidation")]
            IDScan,
            [Description("CURPValidation")]
            CURP,
            [Description("eSignOperation")]
            Sign,
            [Description("SeguriData")]
            SeguridataSign,
            [Description("EmailValidation")]
            Email,
            [Description("PhoneValidation")]
            Phone,
            [Description("DenyListValidation")]
            ListaAml,
            [Description("INEValidation")]
            INE,
            [Description("GeoreferenceOperation")]
            GEOREFERENCIA
        }

        public string SetUrl
        {
            set
            {
                url = value;
            }
        }

        private HttpWebRequest request;
        APIModel api = null;

        public override string ToString()
        {
            return api.ToString();
            //return base.ToString();
        }

        private Tipo tipoDeApi = Tipo.Ninguna;
        private string url = "";
        private string apikey_tmp = "";
        private string url_servicio_tmp = "";

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IDBiometricsServicesAPI(Tipo tipo_api, string apikey, string url_servicio, Operacion op)
        {
            log.Info("Crea Object IDBiometricsServicesAPI con operación " + op.DescriptionAttr<Operacion>() + " apikey " + apikey);
            string tipo = "";
            tipoDeApi = tipo_api;
            switch (tipo_api)
            {
                case Tipo.DigiAPI: tipo = "DigiApi"; break;
                case Tipo.CheckAPI: tipo = "CheckApi"; break;
                case Tipo.BioAPI: tipo = "BioApi"; break;
            }

            tipoDeApi = tipo_api;
            url = string.Format("{0}/{1}/{2}", url_servicio, tipo, op.DescriptionAttr<Operacion>());

            apikey_tmp = apikey;
            url_servicio_tmp = url_servicio;

            api = ToGetToken(apikey, url_servicio);
        }

        public void NewToken()
        {
            api = ToGetToken(apikey_tmp, url_servicio_tmp);
        }

        public IDBiometricsServicesAPI(string url_servicio)
        {
            url = url_servicio;
        }

        public IDBiometricsServicesAPI()
        {
            url = string.Empty;
        }


        private APIModel ToGetToken(string _key, string urlTokens)
        {
            var cronometro = new Stopwatch();
            string resp = string.Empty;

            //  ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            log.Info("IDBiometricsServicesAPI.ToGetToken");

            request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/GetToken", urlTokens));
            request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1");
            request.Method = "POST";
            request.ContentType = "application/json";

            ModeloDeDatos.Clases.APIModel resultado;
            JObject obj = new();
            obj["apikey"] = _key;

            request.ContentLength = obj.ToString().Length;

            cronometro.Start();
            {
                try
                {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(obj.ToString());
                    }
                    WebResponse respuesta = request.GetResponse();
                    Stream data = respuesta.GetResponseStream();
                    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                    var readStream = new StreamReader(data, encode);
                    Char[] read = new Char[256];
                    int count = readStream.Read(read, 0, 256);
                    while (count > 0)
                    {
                        resp += new String(read, 0, count);
                        count = readStream.Read(read, 0, 256);
                    }
                    readStream.Close();
                    respuesta.Close();
                    resultado = Newtonsoft.Json.JsonConvert.DeserializeObject<APIModel>(resp);
                    resultado.result = true;
                }
                catch (Exception ex)
                {
                    log.Fatal("ToGetToken() genero excepción a " + string.Format("{0}/GetToken", urlTokens), ex);

                    resultado = new APIModel
                    {
                        result = false,
                        token = string.Empty,
                        dateIssue = DateTime.Now,
                        tokenType = string.Empty,
                        message = "API Key no autorizada."
                    };
                }
            }
            cronometro.Stop();
            resultado.TiempoEspera = cronometro.ElapsedMilliseconds;
            return resultado;
        }


        public bool Call(string Evento, bool autorizacion, out string salida)
        {
            return Call(Evento, false, string.Empty, out salida);
        }

        public bool Call(string Evento, bool autorizacion, string jdata, out string salida)
        {
            if (!api.result)
            {
                salida = api.message;
                return false;
            }

            log.Info(string.Format("IDBiometricsServicesAPI.Call({0}) \r\n\r\n Longitud del mensaje: {1}", Evento, jdata.Length));

            salida = string.Empty;
            var cronometro = new Stopwatch();
            string resp = string.Empty;

            request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/{1}", url, Evento));
            request.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = jdata.Length;
            if (autorizacion)
                request.Headers.Add(HttpRequestHeader.Authorization, api.ToString());
            //NDQ2NDNfZG9taWNpbGlv
            cronometro.Start();
            {
                try
                {
                    if (jdata.Length > 0)
                    {
                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(jdata.ToString());
                        }
                    }
                    WebResponse respuesta = request.GetResponse();
                    Stream data = respuesta.GetResponseStream();
                    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                    StreamReader readStream = new StreamReader(data, encode);
                    resp = readStream.ReadToEnd();

                    readStream.Close();
                    respuesta.Close();
                }
                catch (WebException ex)
                {
                    log.Fatal("Error en Call(" + Evento + ") in WebException", ex);
                    JObject objSalidaError = new JObject();
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        objSalidaError["error"] = 0;
                        objSalidaError["esValido"] = false;
                        objSalidaError["mensaje"] = "El evento ha generado un BadRequest " + ex.Message;
                        objSalidaError["ExceptionCode"] = (int)HttpStatusCode.BadRequest;
                        salida = JsonConvert.SerializeObject(objSalidaError);
                        return true;
                    }
                    else
                    {
                        salida = ex.Message;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    log.Fatal("Error en Call(" + Evento + ") in Exception", ex);
                }
            }
            cronometro.Stop();
            salida = resp;
            return true;
        }

        public string Error { get; private set; }

        public string CallGet(string invoke, params string[] parametros)
        {
            var cronometro = new Stopwatch();
            string resp = string.Empty;
            request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/{1}/{2}", url, invoke, string.Join("/", parametros)));
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;
            cronometro.Start();
            try
            {
                WebResponse respuesta = request.GetResponse();
                Stream data = respuesta.GetResponseStream();
                long lData = respuesta.ContentLength;
                StreamReader readStream = new StreamReader(data);
                Char[] read = new Char[1128];
                int count = readStream.Read(read, 0, 1128);
                while (count > 0)
                {
                    resp += new String(read, 0, count);
                    count = readStream.Read(read, 0, 1128);
                }
                readStream.Close();
                respuesta.Close();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = string.Empty;
            }
            cronometro.Stop();
            return resp;
        }

        public MensajeWebService CallToGet(string evento, string idToGet)
        {

            var msg = new MensajeWebService();
            string resp = string.Empty;
            request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/{1}/{2}", url, evento, idToGet));

            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;
            try
            {
                WebResponse respuesta = request.GetResponse();
                Stream data = respuesta.GetResponseStream();
                StreamReader readStream = new StreamReader(data);
                Char[] read = new Char[256];
                int count = readStream.Read(read, 0, 256);
                while (count > 0)
                {
                    resp += new String(read, 0, count);
                    count = readStream.Read(read, 0, 256);
                }
                readStream.Close();
                respuesta.Close();
            }
            catch (Exception ex)
            {
                msg.Excepcion = ex.Message;
                msg.Mensaje = "Error al obtener la referencia de la verificación de de datos RENAPO.";
                msg.Data = string.Empty;
                msg.HasError = true;
            }
            finally
            {
                if (!string.IsNullOrEmpty(resp))
                    msg = JsonConvert.DeserializeObject<MensajeWebService>(resp);
                else
                {
                    msg.Excepcion = string.Empty;
                    msg.Mensaje = "No existe información con el identificador proporcionado.";
                    msg.Data = string.Empty;
                    msg.HasError = true;
                }
            }
            return msg;
        }

    }
}