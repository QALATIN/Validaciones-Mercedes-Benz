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
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ModeloDeDatos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidarGeoreferenciaController : ControllerBase
    {

        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarGeoreferenciaController> _log;

        public ValidarGeoreferenciaController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarGeoreferenciaController> log)
        {
            _context = context;
            _app = cfg;
            _log = log;
        }

        [HttpPost]
        public async Task<IActionResult> PostValidacion(Validacion valEntry)
        {
            /*paso 1: se deberán verificar solo 2 datos de entrada:

            Entradas principales 
                "validacionId": int
                "resultadoGeoreferencia": "string"
            
            validacionId: Se debe proporcionar el ID de la validación
            resultadoGeoreferencia: dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
                1: Forza a realizar una validación
                cualquier otro dato: Si existe validación, devuelve el resultado, de lo contrario realiza la validación

            */

            var cronometro = new Stopwatch();
            var envio = new JObject();
            var salida = new JObject();
            Solicitante itemSolicitante = null;

            cronometro.Start();
            bool revalidar = false;
            string fileName = "";
            ValidationResult validationResult = null;
            string serializeValidationResult = "";
            DateTime fechaTransaccion = DateTime.Now;
            int row_afected = 0;
            bool errorUpdateDb = false;

            if (valEntry.ResultadoGeoreferencia == "1")
                revalidar = true;

            try
            {
                valEntry = _context.Validaciones.Where(x => x.ValidacionId == valEntry.ValidacionId).Include(x => x.Solicitante).FirstOrDefault();
                if (valEntry == null)
                    throw new InvalidOperationException("Error al obtener la información de la solicitud, el id no existe.");
                itemSolicitante = valEntry.Solicitante;

                if (!revalidar)
                {
                    DateTime fechaValidacion = valEntry.FechaGeoreferencia.GetValueOrDefault();
                    if (fechaValidacion != DateTime.MinValue)
                    {
                        fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, TipoValidacion.GEOREFERENCIA);
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
                            fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.GEOREFERENCIA);
                            if (System.IO.File.Exists(fileName))
                            {
                                salida = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(fileName));
                                cronometro.Stop();
                                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                                valEntry.FechaGeoreferencia = fechaTransaccion;
                                valEntry.ResultadoGeoreferencia = salida["Resultado"].ToString();

                                validationResult = new ValidationResult()
                                {
                                    ValidacionId = valEntry.ValidacionId,
                                    Resultado = valEntry.ResultadoGeoreferencia,
                                    Fecha = fechaTransaccion,
                                    ValidacionTipo = TipoValidacion.GEOREFERENCIA
                                };

                                (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                                row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                                _log.LogInformation(string.Format("{0} registros afectados validación Georeferencia", row_afected));

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

                string valorEnvio = itemSolicitante.DireccionCompleta;
                if (string.IsNullOrEmpty(valorEnvio))
                    throw new InvalidOperationException("La dirección no puede estar vacío.");
                envio["address"] = valorEnvio.ToUpper();

                fechaTransaccion = StaticData.FechaServer(_context);
                if (fechaTransaccion == DateTime.MinValue)
                    throw new InvalidOperationException("Error al obtener la fecha del servidor.");

                IDBiometricsServicesAPI rest = null;
                string result = string.Empty;

                rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.DigiAPI,
                    _app.Value.DigiAPIKey,
                    _app.Value.IDBiometricsApiUrl,
                     IDBiometricsServicesAPI.Operacion.GEOREFERENCIA);

                if (!rest.Call("GetGeoreferenceAddress", true, envio.ToString(), out result))
                    throw new InvalidOperationException("Error en la obtención de información del servicio de validación de georeferencia.");
                else
                {
                    Georeferencia georeferencia = JsonConvert.DeserializeObject<Georeferencia>(result);

                    JObject data = new();

                    cronometro.Stop();
                    salida["Error"] = 0;
                    salida["Mensaje"] = "Validación realizada de georeferencia por dirección.";
                    salida["Tiempo_Espera"] = cronometro.ElapsedMilliseconds;
                    salida["Map"] = georeferencia.Map;
                    salida["Resultado"] = true.ToString();
                    valEntry.ResultadoGeoreferencia = true.ToString();
                    valEntry.FechaGeoreferencia = fechaTransaccion;

                    validationResult = new ValidationResult()
                    {
                        ValidacionId = valEntry.ValidacionId,
                        Resultado = valEntry.ResultadoGeoreferencia,
                        Fecha = fechaTransaccion,
                        ValidacionTipo = TipoValidacion.GEOREFERENCIA
                    };

                    fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.GEOREFERENCIA);
                    System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(salida, Formatting.Indented));
                    _log.LogInformation("Genera Json de validación Georeferencia en " + fileName);

                    (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                    row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                    _log.LogInformation(string.Format("{0} registros afectados validación Georeferencia", row_afected));

                    if (row_afected == 0)
                    {
                        errorUpdateDb = true;
                        throw new InvalidOperationException("Error al actualizar la base de datos con el resultado de la validación.");
                    }

                    return Ok(salida);
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

                salida["Error"] = 1;
                salida["Mensaje"] = message;
                salida["Tiempo_Espera"] = cronometro.ElapsedMilliseconds;
                return BadRequest(JsonConvert.SerializeObject(salida));
            }
        }

    }
}
