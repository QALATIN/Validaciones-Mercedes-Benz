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
    public class ValidarTelefonoController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarTelefonoController> _log;

        public ValidarTelefonoController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarTelefonoController> log)
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
                "resultadoTelefono": "string"

            validacionId: Se debe proporcionar el ID de la validación
            resultadoTelefono: dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
                1: Forza a realizar una validación
                cualquier otro dato: Si existe validación, devuelve el resultado, de lo contrario realiza la validación

            */

            Stopwatch cronometro = new();
            JObject envio = new();
            JObject salida = new();
            Solicitante ItemSolicitante = null;

            cronometro.Start();
            bool revalidar = false;
            string fileName = "";
            ValidationResult validationResult = null;
            string serializeValidationResult = "";
            DateTime fechaTransaccion = DateTime.Now;
            int row_afected = 0;
            bool errorUpdateDb = false;

            if (valEntry.ResultadoTelefono == "1")
                revalidar = true;

            try
            {
                valEntry = _context.Validaciones.Where(x => x.ValidacionId == valEntry.ValidacionId).Include(x => x.Solicitante).FirstOrDefault();
                if (valEntry == null)
                    throw new InvalidOperationException("Error al obtener la información de la solicitud, el id no existe.");
                ItemSolicitante = valEntry.Solicitante;

                if (!revalidar)
                {
                    DateTime fechaValidacion = valEntry.FechaTelefono.GetValueOrDefault();
                    if (fechaValidacion != DateTime.MinValue)
                    {
                        fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, TipoValidacion.TELEFONO);
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
                            fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.TELEFONO);
                            if (System.IO.File.Exists(fileName))
                            {
                                salida = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(fileName));
                                cronometro.Stop();
                                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                                valEntry.FechaTelefono = fechaTransaccion;
                                valEntry.ResultadoTelefono = salida["resultado"].ToString();
                                valEntry.SemaforoTelefono = salida["semaforo"].ToString();

                                validationResult = new ValidationResult()
                                {
                                    ValidacionId = valEntry.ValidacionId,
                                    Semaforo = valEntry.SemaforoTelefono,
                                    Resultado = valEntry.ResultadoTelefono,
                                    Fecha = fechaTransaccion,
                                    ValidacionTipo = TipoValidacion.TELEFONO
                                };

                                (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                                row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                                _log.LogInformation(string.Format("{0} registros afectados validación Teléfono", row_afected));

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
                string itemToValidate = ItemSolicitante.Telefono;
                if (string.IsNullOrEmpty(itemToValidate))
                    throw new InvalidOperationException("El número telefónico no puede ir vacio.");
                envio["Phone"] = itemToValidate.ToUpper();

                fechaTransaccion = StaticData.FechaServer(_context);
                if (fechaTransaccion == DateTime.MinValue)
                    throw new InvalidOperationException("Error al obtener la fecha del servidor.");

                IDBiometricsServicesAPI rest = null;
                string result = string.Empty;

#if SERVICES
                rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.CheckAPI,
                    _app.Value.CheckAPIKey,
                    _app.Value.IDBiometricsApiUrl,
                     IDBiometricsServicesAPI.Operacion.Phone);
                if (!rest.Call("Validate", true, envio.ToString(), out result))
                    throw new InvalidOperationException("Error en la obtención de información del servicio de verificación de número telefónico.");
                else
#else
                    JObject xxx = new();
                JObject yyy = new();
                xxx["message"] = "OK";
                xxx["success"] = true;
                yyy["numeroIdentificadorRegion"] = itemToValidate.ToUpper().Substring(0, 2);
                yyy["tipoRed"] = "Móvil-CPP";
                yyy["ciudad"] = "CUAUHTEMOC";
                yyy["marcacionNacional"] = itemToValidate.ToUpper();
                yyy["marcacionEU"] = "011 52 " + itemToValidate.ToUpper();
                yyy["marcacionInt"] = "+52 " + itemToValidate.ToUpper();
                yyy["proveedor"] = "UNKNOWN";
                xxx["responseData"] = yyy;
                xxx["code"] = 0;
                result = JsonConvert.SerializeObject(xxx);
#endif
                {
                    //procesamos la información obtenida de la API
                    string semaforo = string.Empty;
                    PHONE objResultado = JsonConvert.DeserializeObject<PHONE>(result);
                    JObject data = new();
                    cronometro.Stop();
                    if (objResultado.Success)
                    {
                        salida["error"] = 0;
                        if (!objResultado.Mensaje.ToUpper().Contains("OK"))
                        {
                            data["numero"] = itemToValidate;
                            salida["semaforo"] = "rojo";
                            salida["mensaje"] = objResultado.Mensaje;
                        }
                        else
                        {
                            data["numero"] = objResultado.Respuesta.NumeroTelefonico;
                            semaforo = "verde";
                            salida["semaforo"] = semaforo;
                            salida["mensaje"] = "Validación telefónica realizada con éxito.";

                            data["tipo_red"] = objResultado.Respuesta.TipoDeRed;
                            data["marcacion_internacional"] = objResultado.Respuesta.MarcacionInternacional;
                            data["proveedor"] = objResultado.Respuesta.Proveedor;
                        }
                        salida["resultado"] = true.ToString();
                        valEntry.ResultadoTelefono = true.ToString();
                    }
                    else
                    {
                        data["numero"] = itemToValidate;
                        salida["semaforo"] = "gris";
                        salida["mensaje"] = objResultado.Mensaje;
                        salida["resultado"] = false.ToString();
                        valEntry.ResultadoTelefono = false.ToString();
                    }
                    salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                    salida["datos_validacion"] = data;

                    valEntry.FechaTelefono = fechaTransaccion;
                    valEntry.SemaforoTelefono = semaforo;

                    // Se identifico que se realizo la consulta, se guardo el archivo en el disco, pero no actualizo los datos en la base
                    validationResult = new ValidationResult()
                    {
                        ValidacionId = valEntry.ValidacionId,
                        Semaforo = valEntry.SemaforoTelefono,
                        Resultado = valEntry.ResultadoTelefono,
                        Fecha = fechaTransaccion,
                        ValidacionTipo = TipoValidacion.TELEFONO
                    };

                    fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.TELEFONO);
                    System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(salida, Formatting.Indented));
                    _log.LogInformation("Genera Json de validación Teléfono en " + fileName);

                    //_context.Entry(valEntry).State = EntityState.Modified;
                    //row_afected = await _context.SaveChangesAsync();

                    (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                    row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                    _log.LogInformation(string.Format("{0} registros afectados validación Teléfono", row_afected));

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
                salida["error"] = 1;
                salida["mensaje"] = message;
                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                //salida["exception"] = JsonConvert.SerializeObject(new Error(ex));
                return BadRequest(JsonConvert.SerializeObject(salida));
            }
        }
    }
}
