using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModeloDeDatos.Clases;
using ModeloDeDatos.Context;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModeloDeDatos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidarReferenciaController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarDocumentosController> _log;


        public ValidarReferenciaController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarDocumentosController> log)
        {
            _context = context;
            _app = cfg;
            _log = log;
        }

        private enum TIPO_COMPROBANTE
        {
            Ingresos = 1,
            Domicilio = 2,
            Bancario = 3,
            Ninguno = -1
        }

        [HttpPost]
        public async Task<ActionResult<JObject>> PostValidacion(JObject ValoresEntrada)
        {
            /*
             
               {
            "revalidar": false
                "resultado_comprobante_referencia": null,
                "validacion_id": 8,
                "tipo_comprobante": "1"
                }
            //11102022143818421
             */


            /*paso 1: se deberán verificar solo 2 datos de entrada:

            validacion_id: debemos traer el ID de la validación
            resultadoCurp: dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
                0: mantener la validación como esta.
                1: solicitar una nueva validación 2(o cualquier otro dato): Forza a realiz
               ar una validación

           Entradas principales 
              "validacionId": int,
              "semaforoCurp": string,
              "resultadoCurp": string,
              "fechaCurp": DateTime,
            */


            IDBiometricsServicesAPI rest = null;
            bool hError = false;
            string envio = string.Empty;
            string result = string.Empty;
            string msg = string.Empty;
            //PASO 1: generamos las referencias del documento y las almacenamos en el respectivo campo

            //generamos el token
            rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.DigiAPI,
                _app.Value.DigiAPIKey,
                _app.Value.IDBiometricsApiUrl,
                 IDBiometricsServicesAPI.Operacion.IDScan);

            JObject SendAPI = new JObject();
            string reference = ValoresEntrada["ref"].ToString();
            bool isValidated = false;

            if (!string.IsNullOrEmpty(reference))
            {
                SendAPI = new JObject();
                SendAPI["reference"] = reference;
                SendAPI["message"] = "Documentos enviados correctamente";
                result = SendAPI.ToString(Formatting.None);
            }

            Stopwatch cronoResult = new Stopwatch();
            string tResult = result;
            JObject referencia = null;
            JObject jResult = null;

            List<ModeloDeDatos.Clases.IDSCAN> IDScanDocumento = null;// 
            do
            {
                referencia = JsonConvert.DeserializeObject<JObject>(result);
                rest.NewToken();//generamos un nuevo token para obtener los datos de la referencia generada
                if (rest.Call("GetProofAddress", true, referencia.ToString(Formatting.None), out result))
                {
                    if (!cronoResult.IsRunning)
                        cronoResult.Start();
                    //  result = "{\"message\":\"Verifique su número de referencia o espere a que sus documentos se procesen\"}";
                    try
                    {

                        if (!result.Contains("\"message\""))
                        {
                            IDScanDocumento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);//deserializamos el objeto
                            if (IDScanDocumento.Count <= 0)
                            {
                                return BadRequest("El documento no pudo ser verificado.");
                            }
                            else
                            {
                                isValidated = true;
                                break;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogCritical(ex, "Excepción al deserializar resultado a List<ModeloDeDatos.Clases.IDSCAN>");
                        isValidated = false;
                        result = "Excepción grave al procesar la verificación del documento.";
                        break;
                    }
                }

                if (cronoResult.ElapsedMilliseconds <= 15000)
                {
                    result = tResult;//reincorporamos la referencia
                }
                else
                {
                    isValidated = false;
                    jResult = JsonConvert.DeserializeObject<JObject>(result);
                    result = string.Format("{0}, referencia: {1}", jResult["message"].ToString(), referencia["reference"].ToString());
                    break;
                }

            } while (true);


            foreach (IDSCAN hoja in IDScanDocumento)
            {
                rest.NewToken();
                referencia = null;
                referencia = new JObject();
                referencia["ImageReference"] = hoja.ImagenID;


                if (rest.Call("GetImage", true, referencia.ToString(Formatting.None), out result))
                {
                    string FileName = @"C:\Users\earciniega\Documents\Img\IDScan\" + reference + "_" + hoja.ImagenID + ".jpg";
                    string b64 = JsonConvert.DeserializeObject<JObject>(result)["base64"].ToString();
                    System.IO.File.WriteAllBytes(FileName, Convert.FromBase64String(b64));

                }
            }

            JObject salida = new JObject();
            cronoResult.Stop();
            // if (!rest.Call("GetProofAddress", true, referencia.ToString(Formatting.None), out result))
            if (isValidated)
            {

                return Ok(IDScanDocumento);

                //salida = VerificaComprobanteDeDomicilio(IDScanDocumento);
            }

            return Ok(salida);

        }


        private JObject VerificaComprobanteDeDomicilio(List<ModeloDeDatos.Clases.IDSCAN> documento)
        {
            //List<ModeloDeDatos.Clases.IDSCAN> documento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);
            string codigo_postal = string.Empty;
            string sFechaFacturacion = string.Empty;
            int iFechaDia = 0;
            int iFechaMes = 0;
            int iFechaAnio = 0;
            string semaforo = "";
            string tDocumento = string.Empty;
            DateTime VigenciaDocumento = DateTime.MinValue;
            List<APIValidacion> ListaValidaciones = new List<APIValidacion>();
            string calle = string.Empty;
            JObject data = new JObject();
            JObject salida = new JObject();
            bool verificado = false;
            foreach (IDSCAN hoja in documento)
            {
                if (hoja.Tipo.TipoComprobante == TipoDocumento.CFE)
                {
                    #region Clasificación genérica de detalles
                    tDocumento = hoja.Tipo.Clasificacion + " - " + hoja.Tipo.TipoComprobante;
                    foreach (SectionDescription sec in hoja.Detalles)
                    {
                        if (sec.Calificacion == 100)
                        {//verde
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, sec.Detalle));
                        }
                        else
                        {//rojo
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, sec.Detalle));
                            semaforo = "rojo";
                        }
                    }
                    #endregion

                    #region CFE

                    SectionDescription CalculaFechaCorte = hoja.Detalles.Where(x => x.TipoDetalle.ToUpper().Contains("FECHA DE CORTE")).Select(x => x).FirstOrDefault();
                    if (CalculaFechaCorte != null)
                    {
                        string sCb = CalculaFechaCorte.Seccion2;
                        iFechaDia = Convert.ToInt32(sCb.Substring(18, 2));
                        iFechaMes = Convert.ToInt32(sCb.Substring(16, 2));
                        iFechaAnio = Convert.ToInt32(string.Format("20{0}", sCb.Substring(14, 2)));
                        if (iFechaDia == 0 || iFechaMes == 0 || iFechaAnio == 0)
                        {
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "Error en la extracción de la fecha de vigencia del comprobante."));
                            if (string.IsNullOrEmpty(semaforo))
                                semaforo = "amarillo";
                        }
                        else
                        {
                            VigenciaDocumento = new DateTime(iFechaAnio, iFechaMes, iFechaDia);
                            double dias = (DateTime.Now - VigenciaDocumento).TotalDays;
                            if (dias > 60)
                            {
                                ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, "El comprobante de domicilio presentado no se encuentra vigente, la vigencia debe ser menor a 2 meses."));
                                semaforo = "rojo";
                            }
                            else
                            {
                                ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, "Vigencia correcta del comprobante de domicilio."));
                                if (string.IsNullOrEmpty(semaforo))
                                    semaforo = "verde";
                            }
                        }
                    }
                    else
                    {
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "No se pudo extraer la fecha de vigencia del comprobante de domicilio."));
                        if (string.IsNullOrEmpty(semaforo))
                            semaforo = "amarillo";
                    }

                    // codigo_postal = RegexCFE_CodigoPostal(hoja.Direccion, out calle);
                    calle = hoja.Direccion.Replace("\r", "").Replace("\n", "");
                    verificado = true;
                    break;//ya analizado
                    #endregion
                }
                else if (hoja.Tipo.TipoComprobante == TipoDocumento.TELMEX)
                {
                    #region Clasificación genérica de detalles
                    tDocumento = hoja.Tipo.Clasificacion + " - " + hoja.Tipo.TipoComprobante;
                    foreach (SectionDescription sec in hoja.Detalles)
                    {
                        if (sec.Calificacion == 100)
                        {//verde
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, sec.Detalle));
                        }
                        else
                        {//rojo
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, sec.Detalle));
                            semaforo = "rojo";
                        }
                    }
                    #endregion

                    #region TELMEX

                    List<string> sBuscar = new List<string> { "PAGAR", "TOTAL", "ANTES", "SERVICIOS" };
                    bool contiene = sBuscar.Any(w => hoja.Direccion.ToUpper().Contains(w));
                    if (contiene)
                    {
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "La extracción del domicilio es errónea."));
                        semaforo = "amarillo";
                        calle = string.Empty;
                    }

                    else
                    {
                        calle = hoja.Direccion;
                    //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, hoja.Direccion));
                    }
                    string FechaVencimiento = string.Empty;

                    if (!string.IsNullOrWhiteSpace(hoja.OCR))
                    {

                        foreach (string fila in hoja.OCR.Split('\n'))
                        {
                            if (fila.ToUpper().Contains("Pagar antes de:".ToUpper()))
                            {
                                FechaVencimiento = fila.Split(":")[1];

                                try
                                {
                                    FechaVencimiento = FechaVencimiento.Replace("-", "").Trim();
                                    iFechaDia = Convert.ToInt32(FechaVencimiento.Substring(0, 2));
                                    iFechaMes = Month(FechaVencimiento.Substring(2, 3));


                                   string tmpAnio =  FechaVencimiento.Substring(5, 4);
                                    // tmpAnio = "2022";
                                    string fantasmaAnio = string.Empty;
                                    if(tmpAnio.Any(x => char.IsLetter(x)))
                                    {
                                        foreach(char n in tmpAnio)
                                        {
                                            if(!char.IsDigit(n))
                                            {
                                                if (n == 'Q' || n == 'O')
                                                    fantasmaAnio += "0";
                                                else if (n == 'Z')
                                                    fantasmaAnio += "2";
                                                else if (n == 'S')
                                                    fantasmaAnio += "5";
                                                else if (n == 'L' || n == 'I')
                                                    fantasmaAnio = "1";
                                                else if (n == 'E' || n == 'F')
                                                    fantasmaAnio = "3";
                                                else
                                                    fantasmaAnio = "";
                                            }
                                            else
                                            {
                                                fantasmaAnio += n.ToString();
                                            }
                                        }
                                    }
                                   

                                    iFechaAnio = Convert.ToInt32(fantasmaAnio);
                                }
                                catch
                                {
                                    iFechaAnio = 0;
                                    iFechaDia = 0;
                                    iFechaMes = 0;
                                }
                                break;
                            }

                        }
                    }

                    string bc = hoja.DatoExtra;
                    if (!string.IsNullOrEmpty(bc))
                    {
                        string[] sBc = bc.Split('|');

                        try
                        {
                            string ValorTMX = sBc[0];
                            FechaVencimiento = sBc[1];
                            if (!string.IsNullOrEmpty(ValorTMX))
                            {
                                codigo_postal = ValorTMX.Substring(3, 5);

                                if (iFechaDia == 0 || iFechaMes == 0 || iFechaAnio == 0)
                                {

                                    FechaVencimiento = FechaVencimiento.Replace("-", "").Trim();
                                    iFechaDia = Convert.ToInt32(FechaVencimiento.Substring(0, 2));
                                    iFechaMes = Month(FechaVencimiento.Substring(2, 3));

                                    string tmpAnio = FechaVencimiento.Substring(5, 4);
                                    // tmpAnio = "2022";
                                    string fantasmaAnio = string.Empty;
                                    if (tmpAnio.Any(x => char.IsLetter(x)))
                                    {
                                        foreach (char n in tmpAnio)
                                        {
                                            if (!char.IsDigit(n))
                                            {
                                                if (n == 'Q' || n == 'O')
                                                    fantasmaAnio += "0";
                                                else if (n == 'Z')
                                                    fantasmaAnio += "2";
                                                else if (n == 'S')
                                                    fantasmaAnio += "5";
                                                else if (n == 'L' || n == 'I')
                                                    fantasmaAnio = "1";
                                                else if (n == 'E' || n == 'F')
                                                    fantasmaAnio = "3";
                                                else
                                                    fantasmaAnio = "";
                                            }
                                            else
                                            {
                                                fantasmaAnio += n.ToString();
                                            }
                                        }
                                    }

                                    iFechaAnio = Convert.ToInt32(fantasmaAnio);
                                }
                            }
                            //else
                            //{
                                //codigo_postal = string.Empty;
                                ////string FechaVencimiento = sBc[1];
                                //FechaVencimiento = FechaVencimiento.Replace("-", "");
                                //iFechaDia = Convert.ToInt32(FechaVencimiento.Substring(0, 2));
                               //iFechaMes = Month(FechaVencimiento.Substring(2, 3));
                                //iFechaAnio = Convert.ToInt32(FechaVencimiento.Substring(5, 4));
                            //}
                        }
                        catch
                        {
                          //  iFechaDia = 0;
                          //  iFechaMes = 0;
                          //  iFechaAnio = 0;
                        }
                    }



                    if (iFechaDia == 0 || iFechaMes == 0 || iFechaAnio == 0)
                    {
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "Error en la extracción de la fecha de vigencia del comprobante."));
                        if (string.IsNullOrEmpty(semaforo))
                            semaforo = "amarillo";
                    }
                    else
                    {
                        VigenciaDocumento = new DateTime(iFechaAnio, iFechaMes, iFechaDia);

                        // VigenciaDocumento = VigenciaDocumento.AddMonths(-2);
                        double dias = (DateTime.Now - VigenciaDocumento).TotalDays;
                        if (dias > 60)
                        {
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, string.Format("El comprobante de domicilio presentado no se encuentra vigente, la vigencia debe ser menor a 2 meses ({0:dd/MM/yyyy}). ", VigenciaDocumento)));
                            semaforo = "rojo";
                        }
                        else
                        {
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, "Vigencia correcta del comprobante de domicilio."));
                            if (string.IsNullOrEmpty(semaforo))
                                semaforo = "verde";
                        }
                    }
                    verificado = true;
                    break;
                    #endregion
                }
                else if (hoja.Tipo.TipoComprobante == TipoDocumento.Desconocido)
                {
                    if (verificado) break;
                }
            }

            if (verificado)
            {
                //finalizamos el proceso, devolvemos el json generado
                data["clasificacion"] = tDocumento;
                data["domicilio"] = calle;
                data["codigo_postal"] = codigo_postal;
                data["vigencia"] = VigenciaDocumento.ToString("dd/MM/yyyy");
                salida["error"] = 0;
                salida["mensaje"] = "Verificación del comporbante de domicilio.";
                salida["semaforo"] = semaforo;
                salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                salida["datos_validacion"] = data;
            }
            else
            {
                semaforo = "rojo";
                data["clasificacion"] = "Documendo desconocido";
                ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, "Verificación errónea debido a que el documento es ilegible o está mal capturado."));
                salida["error"] = 0;
                salida["mensaje"] = "El comprobante de domicilio no pudo ser verificado.";
                salida["semaforo"] = semaforo;
                salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                salida["datos_validacion"] = data;
            }
            return salida;
        }
        private JObject VerificaComprobanteDeIngresos(List<ModeloDeDatos.Clases.IDSCAN> documento)
        {
            // List<ModeloDeDatos.Clases.IDSCAN> documento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);

            string tDocumento = string.Empty;
            string semaforo = string.Empty;
            string rfc = string.Empty;
            string clabe = string.Empty;
            bool verificado = false;

            List<APIValidacion> ListaValidaciones = new List<APIValidacion>();

            foreach (IDSCAN hoja in documento)
            {
                if (hoja.Tipo.TipoComprobante != TipoDocumento.Desconocido)
                {
                    tDocumento = hoja.Tipo.Clasificacion + " - " + hoja.Tipo.TipoComprobante;
                    foreach (SectionDescription sec in hoja.Detalles)
                    {
                        if (sec.Calificacion == 100)
                        {//verde
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, sec.Detalle));
                        }
                        else
                        {//rojo
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, sec.Detalle));
                            semaforo = "rojo";
                        }
                    }
                    if (string.IsNullOrEmpty(semaforo))
                        semaforo = "verde";

                    if (!string.IsNullOrEmpty(hoja.DatoExtra))
                    {
                        rfc = hoja.DatoExtra.Split('@')[0];
                        clabe = hoja.DatoExtra.Split('@')[1];
                    }
                    else
                    {
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "La verificación del documento no pudo extraer el RFC y CLABE interbancaria."));
                    }
                    verificado = true;
                    break; //finalizamos la comprobación del documento 
                }
            }
            JObject data = new JObject();
            JObject salida = new JObject();
            if (verificado)
            {
                //finalizamos el proceso, devolvemos el json generado
                data["clasificacion"] = tDocumento;
                data["rfc"] = rfc;
                data["clabe"] = clabe;
                salida["error"] = 0;
                salida["mensaje"] = "Verificación del comporbante de ingresos.";
                salida["semaforo"] = semaforo;
                salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                salida["datos_validacion"] = data;
            }
            else
            {
                semaforo = "rojo";
                data["clasificacion"] = "Documendo desconocido";
                ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, "Verificación errónea debido a que el documento es ilegible o está mal capturado."));
                salida["error"] = 0;
                salida["mensaje"] = "El comprobante de ingresos no pudo ser verificado.";
                salida["semaforo"] = semaforo;
                salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                salida["datos_validacion"] = data;
            }
            return salida;

        }
        private JObject VerificaComprobanteBancario(List<ModeloDeDatos.Clases.IDSCAN> documento)
        {
            //    List<ModeloDeDatos.Clases.IDSCAN> documento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);

            string tDocumento = string.Empty;
            string semaforo = string.Empty;
            string rfc = string.Empty;
            string clabe = string.Empty;
            bool verificado = false;
            List<APIValidacion> ListaValidaciones = new List<APIValidacion>();

            foreach (IDSCAN hoja in documento)
            {
                if (hoja.Tipo.TipoComprobante != TipoDocumento.Desconocido)
                {
                    tDocumento = hoja.Tipo.Clasificacion + " - " + hoja.Tipo.TipoComprobante;
                    foreach (SectionDescription sec in hoja.Detalles)
                    {
                        if (sec.Calificacion == 100)
                        {//verde
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, sec.Detalle));
                        }
                        else
                        {//rojo
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, sec.Detalle));
                            semaforo = "rojo";
                        }
                    }
                    if (string.IsNullOrEmpty(semaforo))
                        semaforo = "verde";

                    if (!string.IsNullOrEmpty(hoja.DatoExtra))
                    {
                        rfc = hoja.DatoExtra.Split('@')[0];
                        clabe = hoja.DatoExtra.Split('@')[1];
                    }
                    else
                    {
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "La verificación del documento no pudo extraer el RFC y CLABE interbancaria."));
                    }
                    verificado = true;
                    break; //finalizamos la comprobación del documento 
                }
            }

            JObject data = new JObject();
            JObject salida = new JObject();
            if (verificado)
            {
                //finalizamos el proceso, devolvemos el json generado
                data["clasificacion"] = tDocumento;
                data["rfc"] = rfc;
                data["clabe"] = clabe;
                salida["error"] = 0;
                salida["mensaje"] = "Verificación del comprobante bancario.";
                salida["semaforo"] = semaforo;
                salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                salida["datos_validacion"] = data;
            }
            else
            {
                semaforo = "rojo";
                data["clasificacion"] = "Documendo desconocido";
                ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, "Verificación errónea debido a que el documento es ilegible o está mal capturado."));
                salida["error"] = 0;
                salida["mensaje"] = "El comprobante bancario no pudo ser verificado.";
                salida["semaforo"] = semaforo;
                salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                salida["datos_validacion"] = data;
            }
            return salida;

        }



        private int Month(string mes)
        {
            switch (mes.ToUpper())
            {
                case "ENE": return 1;
                case "FEB": return 2;
                case "MAR": return 3;
                case "ABR": return 4;
                case "MAY": return 5;
                case "JUN": return 6;
                case "JUL": return 7;
                case "AGO": return 8;
                case "SEP": return 9;
                case "OCT": return 10;
                case "NOV": return 11;
                case "DIC": return 12;
                default: return 0;
            }
        }

        private string RegexCFE_CodigoPostal(string direccion, out string calle)
        {
            string[] filas = direccion.Split('\r');
            string tmp = string.Empty;

            calle = filas[0];
            if (calle.Contains("$"))
                calle = filas[1];
            else
            {
                if (calle.Length <= 11) //puede ser una cantidad o el monto del recibo
                {
                    calle = filas[1];
                }
            }

            foreach (string fila in filas)
            {
                if (Regex.IsMatch(fila, "^.*?[0-9]{5}.*?"))//buscamos el código postal
                {//aquí se encuentra el código postal
                    int index = 0;

                    tmp = fila.Replace(" ", "");
                    string salida = string.Empty;
                    for (int x = 0; x < tmp.Length; x++)
                    {
                        if (char.IsLetterOrDigit(tmp[x]))
                            salida += tmp[x];
                    }

                    if (salida.ToUpper().Contains("CP"))
                        index = salida.LastIndexOf("CP");

                    if (index != -1)
                    {
                        tmp = salida.Remove(0, index + 2);

                        tmp = tmp.Substring(0, 5);//aquí viene el CP

                    }
                    else
                        tmp = string.Empty;
                    break;
                }
            }
            return tmp;
        }
    }
}
