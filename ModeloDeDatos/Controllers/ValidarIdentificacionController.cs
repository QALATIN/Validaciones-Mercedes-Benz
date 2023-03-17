using System;
using System.Diagnostics;
using System.Linq;
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
    public class ValidarIdentificacionController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarIdentificacionController> _log;

        public ValidarIdentificacionController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarIdentificacionController> log)
        {
            _context = context;
            _app = cfg;
            _log = log;
        }

        [HttpPost]
        public async Task<ActionResult<JObject>> PostValidacion(Validacion valEntry)
        {
            /*paso 1: se deberán verificar solo 2 datos de entrada:

            Entradas principales 
                "validacionId": int
                "ResultadoIne": "string"
            
            validacionId: Se debe proporcionar el ID de la validación
            ResultadoIne: dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
                1: Forza a realizar una validación
                cualquier otro dato: Si existe validación, devuelve el resultado, de lo contrario realiza la validación

            */

            var cronometro = new Stopwatch();
            var salida = new JObject();
            Solicitante ItemSolicitante = null;
            Identificacione IneSolicitante = null;

            cronometro.Start();
            bool revalidar = false;
            string fileName = "";
            ValidationResult validationResult = null;
            string serializeValidationResult = "";
            DateTime fechaTransaccion = DateTime.Now;
            int row_afected = 0;
            bool errorUpdateDb = false;

            if (valEntry.ResultadoIne == "1")
                revalidar = true;

            try
            {
                valEntry = _context.Validaciones.Where(x => x.ValidacionId == valEntry.ValidacionId).Include(x => x.Solicitante).FirstOrDefault();
                if (valEntry == null)
                    throw new InvalidOperationException("Error al obtener la información de la solicitud, el id no existe.");

                ItemSolicitante = valEntry.Solicitante;
                IneSolicitante = _context.Identificaciones.Where(x => x.SolicitanteId == ItemSolicitante.SolicitanteId).FirstOrDefault();
                if (IneSolicitante == null)
                    throw new InvalidOperationException("Error al obtener la identificación del solicitante, no existe la credencial del ine.");

                if (!revalidar)
                {
                    DateTime fechaValidacion = valEntry.FechaIne.GetValueOrDefault();
                    if (fechaValidacion != DateTime.MinValue)
                    {
                        fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, TipoValidacion.INE);
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
                        // Se identifico que se realizo la consulta, se guardo el archivo en el disco, pero no actualizo los datos en la base, por eso se buscara el archivo con la fecha actual
                        fechaTransaccion = StaticData.FechaServer(_context);
                        if (fechaTransaccion != DateTime.MinValue)
                        {
                            fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.INE);
                            if (System.IO.File.Exists(fileName))
                            {
                                salida = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(fileName));
                                cronometro.Stop();
                                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                                valEntry.FechaIne = fechaTransaccion;
                                valEntry.ResultadoIne = salida["resultado"].ToString();
                                valEntry.SemaforoIne = salida["semaforo"].ToString();

                                validationResult = new ValidationResult()
                                {
                                    ValidacionId = valEntry.ValidacionId,
                                    Semaforo = valEntry.SemaforoIne,
                                    Resultado = valEntry.ResultadoIne,
                                    Fecha = fechaTransaccion,
                                    ValidacionTipo = TipoValidacion.INE
                                };

                                (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                                row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                                _log.LogInformation(string.Format("{0} registros afectados validación INE", row_afected));

                                if (row_afected == 0)
                                {
                                    errorUpdateDb = true;
                                    throw new InvalidOperationException("Error al actualizar la base de datos con el resultado de la validación.");
                                }

                                return Ok(salida);
                            }
                        }
                    }
                }

                //en caso de no existir alguna validación (que haya sido falsa o bien aún no se ha validado), se procede a hacer el proceso de validación
                fechaTransaccion = StaticData.FechaServer(_context);
                if (fechaTransaccion == DateTime.MinValue)
                    throw new InvalidOperationException("Error al obtener la fecha del servidor.");

                IDBiometricsServicesAPI rest = null;
                string result = string.Empty;
                string envio = string.Empty;
                string msg = string.Empty;
                //PASO 1: generamos las referencias del documento y las almacenamos en el respectivo campo
                //    var item = docs[0];

                //generamos el token
                rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.CheckAPI,
                    _app.Value.CheckAPIKey,
                    _app.Value.IDBiometricsApiUrl,
                     IDBiometricsServicesAPI.Operacion.INE);

                //creamos el objeto que será enviado al SendDocuments
                var SendAPI = new JObject
                {
                    ["cic"] = IneSolicitante.Cic,
                    ["ocr"] = IneSolicitante.Ocr,
                    ["claveElector"] = IneSolicitante.ClaveElector,
                    ["numeroEmision"] = IneSolicitante.NumeroEmision,
                    ["identificadorCiudadano"] = IneSolicitante.IdentificadorCiudadano
                };

                switch (IneSolicitante.TipoIne)
                {
                    case "C":
                        SendAPI["tipoIdentificacion"] = 1;
                        break;
                    case "D":
                        SendAPI["tipoIdentificacion"] = 2;
                        break;
                    case "E":
                    case "F":
                    case "G":
                    case "H":
                        SendAPI["tipoIdentificacion"] = 3;
                        break;
                    default:
                        throw new InvalidOperationException("El valor de tipo de identificación no es válido.");
                }

                if (!rest.Call("Validate", true, SendAPI.ToString(), out result))
                    throw new InvalidOperationException("Error al procesar la validación de la identificación");
                else
                {
                    INE infoResult = JsonConvert.DeserializeObject<INE>(result);
                    string semaforo = string.Empty;
                    if (infoResult.CodigoError == 1)
                    {//devolvemos true

                        salida["error"] = 0;
                        salida["mensaje"] = infoResult.Mensaje;
                        cronometro.Stop();
                        salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                        if (infoResult.ResponseData.EstatusRespuesta == "OK")
                            semaforo = "verde";
                        else
                            semaforo = "rojo";
                        salida["semaforo"] = semaforo;
                        var data = new JObject
                        {
                            ["Modelo"] = IneSolicitante.TipoIne.ToUpper(),
                            ["IneClaveDeElector"] = infoResult.ResponseData.ClaveDeElector,
                            ["IneNumeroDeEmision"] = infoResult.ResponseData.NumeroDeEmision,
                            ["IneOcr"] = infoResult.ResponseData.Ocr,
                            ["IneCic"] = IneSolicitante.Cic,
                            ["IneIdentificadorCiudadano"] = IneSolicitante.IdentificadorCiudadano,
                            ["AnioDeRegistro"] = infoResult.ResponseData.Registro,
                            ["AnioDeEmision"] = infoResult.ResponseData.Emision,
                            ["FechaDeConsulta"] = fechaTransaccion,
                            ["FechaDeVigencia"] = infoResult.ResponseData.FechaVigencia,
                            ["Respuesta"] = infoResult.ResponseData.MensajeFinal,
                            ["CodigoValidacion"] = infoResult.ResponseData.CodigoDeValidacion
                        };

                        salida["datos_validacion"] = data;
                        salida["resultado"] = true.ToString();
                        valEntry.ResultadoIne = true.ToString();

                        valEntry.FechaIne = fechaTransaccion;
                        valEntry.SemaforoIne = semaforo;

                        // Se identifico que se realizo la consulta, se guardo el archivo en el disco, pero no actualizo los datos en la base
                        validationResult = new ValidationResult()
                        {
                            ValidacionId = valEntry.ValidacionId,
                            Semaforo = valEntry.SemaforoIne,
                            Resultado = valEntry.ResultadoIne,
                            Fecha = fechaTransaccion,
                            ValidacionTipo = TipoValidacion.INE
                        };

                        fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.INE);
                        System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(salida, Formatting.Indented));
                        _log.LogInformation("Genera Json de validación INE en " + fileName);

                        //_context.Entry(valEntry).State = EntityState.Modified;
                        //row_afected = await _context.SaveChangesAsync();

                        (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                        row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                        _log.LogInformation(string.Format("{0} registros afectados validación INE", row_afected));

                        if (row_afected == 0)
                        {
                            errorUpdateDb = true;
                            throw new InvalidOperationException("Error al actualizar la base de datos con el resultado de la validación.");
                        }

                        return Ok(salida);
                    }
                    else
                        throw new InvalidOperationException(infoResult.Mensaje);
                }
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
    }
}
