using System;
using System.Collections.Generic;
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
    public class ValidarCorreoController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarCorreoController> _log;

        public ValidarCorreoController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarCorreoController> log)
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
                "resultadoCorreo": "string"
            
            validacionId: Se debe proporcionar el ID de la validación
            resultadoCorreo: dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
                1: Forza a realizar una validación
                cualquier otro dato: Si existe validación, devuelve el resultado, de lo contrario realiza la validación

            */

            var cronometro = new Stopwatch();
            var envio = new JObject();
            var salida  = new JObject();
            Solicitante ItemSolicitante = null;

            cronometro.Start();
            bool revalidar = false;
            string fileName = "";
            ValidationResult validationResult = null;
            string serializeValidationResult = "";
            DateTime fechaTransaccion = DateTime.Now;
            int row_afected = 0;
            bool errorUpdateDb = false;

            if (valEntry.ResultadoCorreo == "1")
                revalidar = true;

            try
            {
                valEntry = _context.Validaciones.Where(x => x.ValidacionId == valEntry.ValidacionId).Include(x => x.Solicitante).FirstOrDefault();
                if (valEntry == null)
                    throw new InvalidOperationException("Error al obtener la información de la solicitud, el id no existe.");
                ItemSolicitante = valEntry.Solicitante;

                if (!revalidar)
                {
                    DateTime fechaValidacion = valEntry.FechaCorreo.GetValueOrDefault();
                    if (fechaValidacion != DateTime.MinValue)
                    { 
                        fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, TipoValidacion.CORREO);
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
                            fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.CORREO);
                            if (System.IO.File.Exists(fileName))
                            {
                                salida = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(fileName));
                                cronometro.Stop();
                                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                                valEntry.FechaCorreo = fechaTransaccion;
                                valEntry.ResultadoCorreo = salida["resultado"].ToString();
                                valEntry.SemaforoCorreo = salida["semaforo"].ToString();

                                validationResult = new ValidationResult()
                                {
                                    ValidacionId = valEntry.ValidacionId,
                                    Semaforo = valEntry.SemaforoCorreo,
                                    Resultado = valEntry.ResultadoCorreo,
                                    Fecha = fechaTransaccion,
                                    ValidacionTipo = TipoValidacion.CORREO
                                };

                                (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                                row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                                _log.LogInformation(string.Format("{0} registros afectados validación Correo", row_afected));

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
                string valorEnvio = ItemSolicitante.CorreoElectronico;
                if (string.IsNullOrEmpty(valorEnvio))
                    throw new InvalidOperationException("El correo electrónico no puede estar vacío.");
                envio["email"] = valorEnvio.ToUpper();

                fechaTransaccion = StaticData.FechaServer(_context);
                if (fechaTransaccion == DateTime.MinValue)
                    throw new InvalidOperationException("Error al obtener la fecha del servidor.");

                IDBiometricsServicesAPI rest = null;
                string result = string.Empty;

                rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.CheckAPI,
                    _app.Value.CheckAPIKey,
                    _app.Value.IDBiometricsApiUrl,
                     IDBiometricsServicesAPI.Operacion.Email);

                if (!rest.Call("Validate", true, envio.ToString(), out result))
                    throw new InvalidOperationException("Error en la obtención de información del servicio de validación de correo electrónico.");
                else
                {
                    //procesamos la información obtenida de la API de RENAPO
                    string semaforo = string.Empty;
             
                    JObject obj = JsonConvert.DeserializeObject<JObject>(result);

                    EMAIL objValidations = obj["query"]["results"][0].ToObject<EMAIL>();

                    //  CURP curpi = JsonConvert.DeserializeObject<CURP>(result);
                    List<APIValidacion> ListaValidaciones = new List<APIValidacion>();
                    int _score = objValidations.Score;

                    if (_score >= 0 && _score <= 300)
                    {
                        semaforo = "verde";
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, objValidations.LeyendaScore));
                    }
                    else if (_score >= 301 && _score <= 799)
                    {
                        semaforo = "amarillo";
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, objValidations.LeyendaScore));
                    }
                    else if (_score >= 800 && _score <= 1000)
                    {
                        semaforo = "rojo";
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, objValidations.LeyendaScore));
                    }
                    else
                    {
                        semaforo = "gris";
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.SIN_ALERTA, objValidations.LeyendaScore));
                    }

                    JObject data = new();

                    //datos generales del correo
                    data["email"] = objValidations.Email;
                    data["score"] = objValidations.Score;
                    data["color_semaforo_interno"] = objValidations.HexRgbScore;
                    data["leyenda_score"] = objValidations.LeyendaScore;
                    data["tipo_fraude"] = objValidations.TipoDeFraudeEn;

                    //datos del dominio
                    data["dominio"] = objValidations.Dominio;
                    data["dominio_empresa"] = objValidations.DominioEmpresa;
                    data["dominio_pais"] = objValidations.DominioUbicacion;

                    cronometro.Stop();
                    salida["error"] = 0;
                    salida["mensaje"] = objValidations.Mensaje;
                    salida["semaforo"] = semaforo;
                    salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                    salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                    salida["datos_validacion"] = data;
                    salida["resultado"] = true.ToString();
                    valEntry.ResultadoCorreo = true.ToString();

                    valEntry.FechaCorreo = fechaTransaccion;
                    valEntry.SemaforoCorreo = semaforo;

                    // Se identifico que se realizo la consulta, se guardo el archivo en el disco, pero no actualizo los datos en la base
                    validationResult = new ValidationResult()
                    {
                        ValidacionId = valEntry.ValidacionId,
                        Semaforo = valEntry.SemaforoCorreo,
                        Resultado = valEntry.ResultadoCorreo,
                        Fecha = fechaTransaccion,
                        ValidacionTipo = TipoValidacion.CORREO
                    };

                    fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.CORREO);
                    System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(salida, Formatting.Indented));
                    _log.LogInformation("Genera Json de validación Correo en " + fileName);

                    //_context.Entry(valEntry).State = EntityState.Modified;
                    //row_afected = await _context.SaveChangesAsync();

                    (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                    row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                    _log.LogInformation(string.Format("{0} registros afectados validación Correo", row_afected));

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
