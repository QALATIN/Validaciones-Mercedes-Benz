using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModeloDeDatos.Clases;
using ModeloDeDatos.Context;
using ModeloDeDatos.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace ModeloDeDatos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidarDocumentosController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarDocumentosController> _log;

        public ValidarDocumentosController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarDocumentosController> log)
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
        public async Task<ActionResult<JObject>> PostValidacion(DocumentoRequest documento)
        {
            /*paso 1: se deberán verificar solo 3 datos de entrada:

            Entradas principales 
                "ValidacionId": int
                "Revalidar": "string"
                "Tipo_Comprobante": "string"

            ValidacionId. Se debe proporcionar el ID de la validación
            Revalidar. dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
                1: Forza a realizar una validación
                cualquier otro dato: Si existe validación, devuelve el resultado, de lo contrario realiza la validación
            Tipo_Comprobante. Se debe proporcionar el ID del tipo de comprobante
            */

            TIPO_COMPROBANTE tipo_comprobante = TIPO_COMPROBANTE.Ninguno;
            var salida = new JObject();
            Solicitante itemSolicitante = null;
            string reference = "";
            JObject referencia = null;

            bool revalidar = false;
            string fileName = "";
            ValidationResult validationResult = null;
            string serializeValidationResult = "";
            DateTime fechaTransaccion = DateTime.Now;
            int row_afected = 0;
            bool errorUpdateDb = false;

            if (documento.Revalidar == "1")
                revalidar = true;

            var cronometro = new Stopwatch();
            cronometro.Start();

            try
            {
                tipo_comprobante = (TIPO_COMPROBANTE)Convert.ToInt32(documento.Tipo_Comprobante);

                Validacion valEntry = _context.Validaciones.Where(x => x.ValidacionId == documento.Validacion_Id).Include(x => x.Solicitante).FirstOrDefault();
                if (valEntry == null)
                    throw new InvalidOperationException("Error al obtener la información de la solicitud, el id no existe.");

                itemSolicitante = valEntry.Solicitante;

                fechaTransaccion = StaticData.FechaServer(_context);

                if (fechaTransaccion == DateTime.MinValue)
                    throw new InvalidOperationException("Error al obtener la fecha del servidor.");

                string nombre_doc = string.Empty;
                DateTime fechaValidacion = DateTime.MinValue;
                var tipoValidacion = TipoValidacion.COMPROBANTE_INGRESOS;

                switch (tipo_comprobante)
                {
                    case TIPO_COMPROBANTE.Ingresos: //comprobante de ingresos
                        nombre_doc = "comprobante de ingreso";
                        reference = valEntry.ResultadoComprobanteIngresos;
                        fechaValidacion = valEntry.FechaComprobanteIngresos.GetValueOrDefault();
                        tipoValidacion = TipoValidacion.COMPROBANTE_INGRESOS;
                        break;
                    case TIPO_COMPROBANTE.Domicilio: //comprobante de domicilio
                        nombre_doc = "comprobante de domicilio";
                        reference = valEntry.ResultadoComprobanteDomicilio;
                        fechaValidacion = valEntry.FechaComprobanteDomicilio.GetValueOrDefault();
                        tipoValidacion = TipoValidacion.COMPROBANTE_DOMICILIO;
                        break;
                    case TIPO_COMPROBANTE.Bancario://comprobante bancario
                        nombre_doc = "comprobante bancario";
                        reference = valEntry.ResultadoComprobanteBancario;
                        fechaValidacion = valEntry.FechaComprobanteBancario.GetValueOrDefault();
                        tipoValidacion = TipoValidacion.COMPROBANTE_BANCARIO;
                        break;
                }

                if (string.IsNullOrEmpty(reference))
                {
                    revalidar = true;
                    fechaValidacion = fechaTransaccion;
                }

                if (!revalidar)
                {
                    fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, tipoValidacion);
                    if (System.IO.File.Exists(fileName))
                    {
                        salida = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(fileName));
                        cronometro.Stop();
                        salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                        return Ok(salida);
                    }
                }
                else
                {
                    reference = "";
                    fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, tipoValidacion);
                }

                var i = new { SolicitanteID = 0, TipoDocumento = "", Imagen = "", TipoDocumentoId = 0 };
                var docs = new[] { i }.ToList();
                docs.Clear();

                docs.Clear();
                docs = _context.Documentos.Where(docu => docu.SolicitanteId == itemSolicitante.SolicitanteId && docu.NombreDocumento == ((int)tipo_comprobante).ToString() && docu.Activo == true)
                                          .OrderByDescending(docu => docu.FechaEnvio)
                                          .Select(item => new
                                          {
                                              SolicitanteID = item.SolicitanteId,
                                              TipoDocumento = item.TipoDocumento,
                                              Imagen = Convert.ToBase64String(item.Imagen),
                                              TipoDocumentoId = Convert.ToInt32(item.NombreDocumento)
                                          }).ToList();
                if (docs.Count <= 0)
                    throw new InvalidOperationException("El " + nombre_doc + " no ha sido digitalizado.");

                IDBiometricsServicesAPI rest = null;
                string envio = string.Empty;
                string result = string.Empty;
                string msg = string.Empty;
                //PASO 1: generamos las referencias del documento y las almacenamos en el respectivo campo
                var item = docs[0];

                //generamos el token
                rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.DigiAPI,
                    _app.Value.DigiAPIKey,
                    _app.Value.IDBiometricsApiUrl,
                     IDBiometricsServicesAPI.Operacion.IDScan);

                //creamos el objeto que será enviado al SendDocuments
                var SendAPI = new JObject
                {
                    ["base64"] = item.Imagen,
                    ["extension"] = string.Format(".{0}", item.TipoDocumento),//.xxx
                    ["typeId"] = 1 //cualquier tipo
                };

                long tiempo_espera = 210000;
                if ((Convert.FromBase64String(item.Imagen).Length / 1048576) >= 4.1)
                    tiempo_espera = 28000;
                else
                    tiempo_espera = 15000;

                var envio_docs = new JArray
                {
                    SendAPI
                };

                envio = new JObject(new JProperty("documents", envio_docs)).ToString();

                if (!string.IsNullOrEmpty(reference))
                {
                    SendAPI = new JObject
                    {
                        ["reference"] = reference,
                        ["message"] = "Documentos enviados correctamente"
                    };
                    result = SendAPI.ToString(Formatting.None);
                }
                else
                {
                    //       result = "{\"reference\":\"09112022172039411\",\"message\":\"Documentos enviados correctamente\"}";
                    if (!rest.Call("SendDocuments", true, envio.ToString(), out result))
                        throw new InvalidOperationException("Error al enviar el " + nombre_doc + " a proceso de validación.");
                    else
                    {//aquí guardamos la referencia
                        referencia = JsonConvert.DeserializeObject<JObject>(result);

                        validationResult = new ValidationResult()
                        {
                            ValidacionId = valEntry.ValidacionId,
                            Semaforo = "gris",
                            Resultado = referencia["reference"].ToString(),
                            Fecha = fechaValidacion,
                            ValidacionTipo = tipoValidacion
                        };
                        
                        switch (tipo_comprobante)
                        {
                            case TIPO_COMPROBANTE.Ingresos: //comprobante de ingresos
                                valEntry.SemaforoComprobanteIngresos = "gris";
                                valEntry.FechaComprobanteIngresos = fechaValidacion;
                                valEntry.ResultadoComprobanteIngresos = referencia["reference"].ToString();
                                validationResult.Resultado = referencia["reference"].ToString();
                                break;
                            case TIPO_COMPROBANTE.Domicilio: //comprobante de domicilio
                                valEntry.SemaforoComprobanteDomicilio = "gris";
                                valEntry.FechaComprobanteDomicilio = fechaValidacion;
                                valEntry.ResultadoComprobanteDomicilio = referencia["reference"].ToString();
                                break;
                            case TIPO_COMPROBANTE.Bancario://comprobante bancario
                                valEntry.SemaforoComprobanteBancario = "gris";
                                valEntry.FechaComprobanteBancario = fechaValidacion;
                                valEntry.ResultadoComprobanteBancario = referencia["reference"].ToString();
                                break;
                        }

                        //_context.Entry(valEntry).State = EntityState.Modified;
                        //row_afected = await _context.SaveChangesAsync();

                        (string qryTemp, NpgsqlParameter[] parameterToArrayTemp) = StaticData.QryUpdateValidacion(validationResult);
                        row_afected = await _context.Database.ExecuteSqlRawAsync(qryTemp, parameterToArrayTemp);
                        _log.LogInformation(string.Format("{0} registros afectados validación {1}.", row_afected, nombre_doc));

                        if (row_afected == 0)
                        {
                            errorUpdateDb = true;
                            throw new InvalidOperationException("Error al actualizar la base de datos con el resultado de la validación.");
                        }

                    }
                }

                var cronoResult = new Stopwatch();
                string tResult = result;

                JObject jResult = null;

                List<ModeloDeDatos.Clases.IDSCAN> IDScanDocumento = null;// 

                bool isValidated = false;
                do
                {
                    referencia = JsonConvert.DeserializeObject<JObject>(result);
                    rest.NewToken();//generamos un nuevo token para obtener los datos de la referencia generada
                    if (rest.Call("GetProofAddress", true, referencia.ToString(Formatting.None), out result))
                    {
                        if (!cronoResult.IsRunning)
                            cronoResult.Start();
                        //  result = "{\"message\":\"Verifique su número de referencia o espere a que sus documentos se procesen\"}";

                        if (!result.Contains("\"message\""))
                        {
                            IDScanDocumento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);//deserializamos el objeto
                            if (IDScanDocumento.Count <= 0)
                                throw new InvalidOperationException("El documento no pudo ser verificado.");
                            else
                            {
                                isValidated = true;
                                break;
                            }
                        }
                    }

                    if (cronoResult.ElapsedMilliseconds <= tiempo_espera)
                    {
                        //borrar
                        //  tResult = "{\"reference\":\"10102022130110412\",\"message\":\"Documentos enviados correctamente\"}"; //este es el bueno
                        //-------
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

                cronoResult.Stop();
                // if (!rest.Call("GetProofAddress", true, referencia.ToString(Formatting.None), out result))
                if (!isValidated)
                {
                    var data = new JObject
                    {
                        ["clasificacion"] = "Documendo desconocido",
                        ["message"] = result,
                        ["referencia"] = referencia["reference"].ToString()
                    };

                    salida["error"] = 1;
                    salida["mensaje"] = $"El {nombre_doc} no pudo ser verificado, se registra la referencia de validación para su posterior verificación.";
                    salida["semaforo"] = "gris";
                    //  salida["documento"] = item.Imagen;
                    salida["documento_tipo"] = item.TipoDocumento;
                    salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                    salida["validaciones"] = null;
                    salida["datos_validacion"] = data;
                    salida["resultado"] = referencia["reference"].ToString();

                    switch (tipo_comprobante)
                    {
                        case TIPO_COMPROBANTE.Ingresos: //comprobante de ingresos
                            valEntry.FechaComprobanteIngresos = fechaValidacion;
                            valEntry.ResultadoComprobanteIngresos = referencia["reference"].ToString();
                            valEntry.SemaforoComprobanteIngresos = salida["semaforo"].ToString();
                            break;
                        case TIPO_COMPROBANTE.Domicilio: //comprobante de domicilio
                            valEntry.FechaComprobanteDomicilio = fechaValidacion;
                            valEntry.ResultadoComprobanteDomicilio = referencia["reference"].ToString();
                            valEntry.SemaforoComprobanteDomicilio = salida["semaforo"].ToString();
                            break;
                        case TIPO_COMPROBANTE.Bancario://comprobante bancario
                            valEntry.FechaComprobanteBancario = fechaValidacion;
                            valEntry.ResultadoComprobanteBancario = referencia["reference"].ToString();
                            valEntry.SemaforoComprobanteBancario = salida["semaforo"].ToString();
                            break;
                    }
                }
                else
                {//mostramos las verificaciones

                    switch (tipo_comprobante)
                    {
                        case TIPO_COMPROBANTE.Ingresos: //comprobante de ingresos

                            salida = VerificaComprobanteDeIngresos(IDScanDocumento);
                            //   salida["documento"] = item.Imagen;
                            salida["documento_tipo"] = item.TipoDocumento;
                            salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;

                            valEntry.FechaComprobanteIngresos = fechaValidacion;
                            valEntry.ResultadoComprobanteIngresos = referencia["reference"].ToString();
                            valEntry.SemaforoComprobanteIngresos = salida["semaforo"].ToString();
                            break;

                        case TIPO_COMPROBANTE.Domicilio: //comprobante de domicilio

                            salida = VerificaComprobanteDeDomicilio(IDScanDocumento, itemSolicitante.Nombre, itemSolicitante.ApellidoPaterno, itemSolicitante.ApellidoMaterno);
                            // salida["documento"] = item.Imagen;
                            salida["documento_tipo"] = item.TipoDocumento;
                            salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;

                            valEntry.FechaComprobanteDomicilio = fechaValidacion;
                            valEntry.ResultadoComprobanteDomicilio = referencia["reference"].ToString();
                            valEntry.SemaforoComprobanteDomicilio = salida["semaforo"].ToString();
                            break;

                        case TIPO_COMPROBANTE.Bancario://comprobante bancario

                            salida = VerificaComprobanteBancario(IDScanDocumento);
                            salida["documento_tipo"] = item.TipoDocumento;
                            salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;

                            valEntry.FechaComprobanteBancario = fechaValidacion;
                            valEntry.ResultadoComprobanteBancario = referencia["reference"].ToString();
                            valEntry.SemaforoComprobanteBancario = salida["semaforo"].ToString();
                            break;
                    }
                    salida["resultado"] = referencia["reference"].ToString();

                    System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(salida, Formatting.Indented));
                    _log.LogInformation("Genera Json de validación " + nombre_doc + " en " + fileName);
                }

                cronometro.Stop();

                validationResult = new ValidationResult()
                {
                    ValidacionId = valEntry.ValidacionId,
                    Semaforo = salida["semaforo"].ToString(),
                    Resultado = referencia["reference"].ToString(),
                    Fecha = fechaValidacion,
                    ValidacionTipo = tipoValidacion
                };

                //_context.Entry(valEntry).State = EntityState.Modified;
                //row_afected = await _context.SaveChangesAsync();

                (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                _log.LogInformation(string.Format("{0} registros afectados validación {1}.", row_afected, nombre_doc));

                if (row_afected == 0)
                {
                    errorUpdateDb = true;
                    throw new InvalidOperationException("Error al actualizar la base de datos con el resultado de la validación.");
                }

                return Ok(salida);

            }
            catch (Exception ex)
            {
                if (cronometro.IsRunning)
                    cronometro.Stop();

                string message = ex.Message;
                if (errorUpdateDb)
                {
                    serializeValidationResult = JsonConvert.SerializeObject(validationResult);
                    salida = new();
                    message = serializeValidationResult;
                }
                salida["error"] = 1;
                salida["mensaje"] = message;
                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                //salida["exception"] = JsonConvert.SerializeObject(new Error(ex));
                return BadRequest(JsonConvert.SerializeObject(salida));
            }

        }

        private static JObject VerificaComprobanteDeDomicilio(List<ModeloDeDatos.Clases.IDSCAN> documento, string nombre, string paterno, string materno)
        {
            //List<ModeloDeDatos.Clases.IDSCAN> documento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);
            string codigo_postal = string.Empty;
            string sFechaFacturacion = string.Empty;
            string semaforo = "";
            string tDocumento = string.Empty;
            DateTime VigenciaDocumento = DateTime.MinValue;
            var ListaValidaciones = new List<APIValidacion>();
            string calle = string.Empty;
            var data = new JObject();
            var salida = new JObject();
            bool verificado = false;
            foreach (IDSCAN hoja in documento)
            {
                if (hoja.Tipo.TipoComprobante == TipoDocumento.CFE || hoja.Tipo.TipoComprobante == TipoDocumento.TELMEX ||
                    hoja.Tipo.TipoComprobante == TipoDocumento.IZZI || hoja.Tipo.TipoComprobante == TipoDocumento.TOTAL_PLAY)
                {
                    tDocumento = hoja.Tipo.Clasificacion + " - " + hoja.Tipo.TipoComprobante;
                    if(hoja.Calificacion >= 75)
                    {
                        semaforo = "verde";
                    }
                    else if(hoja.Calificacion>= 50 && hoja.Calificacion < 75)
                    {
                        semaforo = "amarillo";
                    }
                    else
                    {
                        semaforo = "rojo";
                    }

                    foreach (SectionDescription sec in hoja.Detalles)
                    {
                        if (sec.Calificacion >= 75)
                        {
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, sec.Detalle));
                        }
                        else
                        {
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, sec.Detalle));
                        }
                    }
                    SectionDescription CalculaFechaCorte = hoja.Detalles.Where(x => x.Detalle.ToUpper().Contains("FECHA DE CORTE")).Select(x => x).FirstOrDefault();
                    if (CalculaFechaCorte != null)
                    {
                        string sCb = CalculaFechaCorte.Seccion1;
                        string Fecha = ConstruirFecha(CalculaFechaCorte.Seccion1);
                        DateTime fechasalida = new DateTime();
                        if(DateTime.TryParse(Fecha, out fechasalida))
                        {
                            VigenciaDocumento = fechasalida;
                        }
                    }
                    Regex regex = new Regex(@"\d{5}", RegexOptions.IgnoreCase);
                    MatchCollection joinMatches = regex.Matches(calle);
                    if (joinMatches.Count >= 1)
                    {
                        codigo_postal = joinMatches[0].Value;
                    }
                    

                    // codigo_postal = RegexCFE_CodigoPostal(hoja.Direccion, out calle);
                    calle = hoja.Direccion.Replace("\r", "").Replace("\n", "");
                    verificado = true;
                    break;//ya analizado
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
                salida["mensaje"] = "Verificación del comprobante de domicilio.";
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

        private static JObject VerificaComprobanteDeIngresos(List<ModeloDeDatos.Clases.IDSCAN> documento)
        {
            // List<ModeloDeDatos.Clases.IDSCAN> documento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);

            string tDocumento = string.Empty;
            string semaforo = string.Empty;
            string rfc = string.Empty;
            string clabe = string.Empty;
            bool verificado = false;

            var ListaValidaciones = new List<APIValidacion>();

            foreach (IDSCAN hoja in documento)
            {
                if (hoja.Tipo.TipoComprobante != TipoDocumento.Desconocido)
                {
                    #region Clasificación genérica de detalles

                    tDocumento = hoja.Tipo.Clasificacion + " - " + hoja.Tipo.TipoComprobante;
                    foreach (SectionDescription sec in hoja.Detalles)
                    {
                        if (sec.Calificacion == 100)
                        {//verde
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, sec.Detalle));
                            semaforo = "verde";
                        }
                        else
                        {//rojo
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, sec.Detalle));
                            semaforo = "rojo";
                        }
                    }
                    #endregion

                    if (hoja.Tipo.Clasificacion.ToUpper() == "ESTADO DE CUENTA")
                    {
                        //
                        //     splihoja.OCR

                        Regex regex = new Regex(@"(CLABE)([\sa-zA-Z\W]*)\d{18}", RegexOptions.IgnoreCase);
                        MatchCollection joinMatches = regex.Matches(hoja.OCR);

                        clabe = string.Empty;
                        if (joinMatches.Count >= 1)
                        {
                            clabe = joinMatches[0].Value;
                        }

                        if (!string.IsNullOrEmpty(clabe))
                        {
                            regex = new Regex(@"\d{18}", RegexOptions.IgnoreCase);
                            joinMatches = regex.Matches(clabe);

                         //   clabe = string.Empty;
                            if (joinMatches.Count >= 1)
                            {
                                clabe = joinMatches[0].Value;
                                ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "La verificación del documento no pudo extraer el RFC del comprobante de ingresos."));
                            }
                        }
                        verificado = true;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(semaforo))
                            semaforo = "verde";

                        bool extraccionRFC = false;
                        if (!string.IsNullOrEmpty(hoja.DatoExtra))
                        {
                            if (hoja.DatoExtra.Contains("@"))
                            {
                                rfc = hoja.DatoExtra.Split('@')[0];
                                clabe = hoja.DatoExtra.Split('@')[1];
                                extraccionRFC = true;
                            }
                        }
                        
                        if(!extraccionRFC)
                        {
                            semaforo = "amarillo";
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, "La verificación del documento no pudo extraer el RFC y CLABE interbancaria."));
                        }

                        verificado = true;
                        break; //finalizamos la comprobación del documento 
                    }
                }
            }
            var data = new JObject();
            var salida = new JObject();
            if (verificado)
            {
                //finalizamos el proceso, devolvemos el json generado
                data["clasificacion"] = tDocumento;
                data["rfc"] = rfc;
                data["clabe"] = clabe;
                salida["error"] = 0;
                salida["mensaje"] = "Verificación del comprobante de ingresos.";
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

        private static JObject VerificaComprobanteBancario(List<ModeloDeDatos.Clases.IDSCAN> documento)
        {
            //    List<ModeloDeDatos.Clases.IDSCAN> documento = JsonConvert.DeserializeObject<List<ModeloDeDatos.Clases.IDSCAN>>(result);

            string tDocumento = string.Empty;
            string semaforo = string.Empty;
            string rfc = string.Empty;
            string clabe = string.Empty;
            bool verificado = false;
            var ListaValidaciones = new List<APIValidacion>();

            foreach (IDSCAN hoja in documento)
            {
                if (hoja.Tipo.TipoComprobante != TipoDocumento.Desconocido)
                {
                    tDocumento = hoja.Tipo.Clasificacion + " - " + hoja.Tipo.TipoComprobante;
                    if (hoja.Calificacion >= 75)
                    {
                        semaforo = "verde";
                    }
                    else if (hoja.Calificacion >= 50 && hoja.Calificacion < 75)
                    {
                        semaforo = "amarillo";
                    }
                    else
                    {
                        semaforo = "rojo";
                    }

                    foreach (SectionDescription sec in hoja.Detalles)
                    {
                        if (sec.Calificacion >= 75)
                        {
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, sec.Detalle));
                        }
                        else
                        {
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, sec.Detalle));
                        }
                    }

                    if (!string.IsNullOrEmpty(hoja.DatoExtra))
                    {
                        rfc = hoja.DatoExtra.Split('@')[0];
                        clabe = hoja.DatoExtra.Split('@')[1];
                    }
                    else
                    verificado = true;
                    break; //finalizamos la comprobación del documento 
                }
            }

            var data = new JObject();
            var salida = new JObject();
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

        private static int Month(string mes)
        {
            return mes.ToUpper() switch
            {
                "ENE" => 1,
                "FEB" => 2,
                "MAR" => 3,
                "ABR" => 4,
                "MAY" => 5,
                "JUN" => 6,
                "JUL" => 7,
                "AGO" => 8,
                "SEP" => 9,
                "OCT" => 10,
                "NOV" => 11,
                "DIC" => 12,
                _ => 0,
            };
        }

        private static string RegexCFE_CodigoPostal(string direccion, out string calle)
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

        private static string DomicilioFromTelmex(string ocr, string nombres, string paterno, string materno, out string codigo_postal)
        {
            codigo_postal = string.Empty;
            //Regex regex = new Regex("^C\\.P\\. [0-9]+-[a-zA-Z]+", RegexOptions.IgnoreCase);

            var regex = new Regex("^[A-Z]{1}[AEIOU]{1}[A-Z]{2}" +
                        "[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1])" +
                        "[HM]{1}" +
                        "(AS|BC|BS|CC|CS|CH|CL|CM|DF|DG|GT|GR|HG|JC|MC|MN|MS|NT|NL|OC|PL|QT|QR|SP|SL|SR|TC|TS|TL|VZ|YN|ZS|NE)" +
                        "[B-DF-HJ-NP-TV-Z]{3}" +
                        "[0-9A]{1}" +
                        "[0-9]{1}$", RegexOptions.IgnoreCase);

            string cadenota_domicilio = "";
            //^C\.P\. [0-9]+-[a-zA-Z]+$
            string[] filas = ocr.Split('\n');
            int get_fila_cp = 0;
            for (int x = 0; x < filas.Length; x++)
            {
                if (regex.IsMatch(filas[x]))
                {
                    get_fila_cp = x;
                    break;
                }

            }
            int first_pos = 4;
            for(int x = 0; x <= get_fila_cp; x++)
            {
                if (filas[x].ToLower().Contains("a cobro"))
                {
                    filas[x] = "";
                    first_pos++;
                }
                if (filas[x].ToLower().Contains("banco"))
                {
                    first_pos++;
                    filas[x] = "";
                }
            }

            bool valido_nombre = false;
            if (get_fila_cp > first_pos)
            {
                for (int y = get_fila_cp - first_pos; y <= get_fila_cp; y++)
            {
                if (!string.IsNullOrEmpty(filas[y]))
                {
                    if (y == get_fila_cp)
                    {//en este punto se obtiene el código postal
                        MatchCollection joinMatches = regex.Matches(filas[y]);
                        string tmp = string.Empty;

                        if (joinMatches.Count <= 1)
                        {
                            tmp = joinMatches[0].ToString();
                        }

                        tmp = tmp.Substring(0, tmp.LastIndexOf('-'));
                        cadenota_domicilio += tmp;
                        tmp = tmp.Replace("C", "").Replace("P", "").Replace(".", "").Trim();
                        codigo_postal = tmp;
                    }
                    else
                    {
                        if (!valido_nombre)
                        {
                            if (!filas[y].Contains("RFC")) //por si detecta la fila del RFC de telmex
                            {
                                if (string.IsNullOrEmpty(nombres) && string.IsNullOrEmpty(paterno) && string.IsNullOrEmpty(materno))
                                {
                                    cadenota_domicilio += string.Format("{0}, ", filas[y].Trim());
                                    valido_nombre = true;
                                }
                                else
                                {//hacemos validaciones del nombre en la fila Y 

                                    if (!string.IsNullOrEmpty(nombres) && filas[y].Contains(nombres))
                                    {
                                        valido_nombre = true;
                                        continue;
                                    }
                                    else if (!string.IsNullOrEmpty(paterno) && filas[y].Contains(paterno))
                                    {
                                        valido_nombre = true;
                                        continue;
                                    }
                                    else if (!string.IsNullOrEmpty(materno) && filas[y].Contains(materno))
                                    {
                                        valido_nombre = true;
                                        continue;
                                    }
                                    else
                                    {
                                        cadenota_domicilio += string.Format("{0}, ", filas[y].Trim());
                                        valido_nombre = true;
                                    }
                                }
                            }
                            else
                            {
                                valido_nombre = true;
                                continue;
                            }
                        }
                        else
                        {
                            cadenota_domicilio += string.Format("{0}, ", filas[y].Trim());
                        }
                    }
                }
            }
            }

            return cadenota_domicilio.Trim();
        }


        private static string ConstruirFecha(string fecha)
        {
            try
            {
                string dater = "";
                if (fecha != null)
                {
                    fecha = fecha.Trim();
                }
                string[] arr = fecha.Split('-');
                if (arr.Length == 1)
                {
                    arr = fecha.Split('/');
                }
                if (arr.Length == 1)
                {
                    arr = fecha.Split(' ');

                }
                if (arr.Length == 1)
                {
                    return "";

                }
                else
                {
                    if (arr.Length == 3)
                    {
                        string mes = devolvermes(arr[1]);
                        if (mes == "00")
                        {
                            mes = arr[1];
                        }
                        dater = arr[0] + "/" + mes + "/" + arr[2];
                        return dater;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private static string devolvermes(string mes)
        {
            mes = mes.Replace("Q", "O");
            mes = mes.Replace("0", "O");
            mes = mes.Replace("1", "I");
            string mesdevuelto = "00";
            if (mes.ToUpper().Contains("ENE"))
            {
                mesdevuelto = "01";
            }
            if (mes.ToUpper().Contains("FEB"))
            {
                mesdevuelto = "02";
            }
            if (mes.ToUpper().Contains("MAR"))
            {
                mesdevuelto = "03";
            }
            if (mes.ToUpper().Contains("ABR"))
            {
                mesdevuelto = "04";
            }
            if (mes.ToUpper().Contains("MAY"))
            {
                mesdevuelto = "05";
            }
            if (mes.ToUpper().Contains("JUN"))
            {
                mesdevuelto = "06";
            }
            if (mes.ToUpper().Contains("JUL"))
            {
                mesdevuelto = "07";
            }
            if (mes.ToUpper().Contains("AGO"))
            {
                mesdevuelto = "08";
            }
            if (mes.ToUpper().Contains("SEP"))
            {
                mesdevuelto = "09";
            }
            if (mes.ToUpper().Contains("OCT"))
            {
                mesdevuelto = "10";
            }
            if (mes.ToUpper().Contains("NOV"))
            {
                mesdevuelto = "11";
            }
            if (mes.ToUpper().Contains("DIC"))
            {
                mesdevuelto = "12";
            }
            return mesdevuelto;

        }

    }
}
