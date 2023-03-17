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
    public class ValidarListaInteresController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarListaInteresController> _log;

        public ValidarListaInteresController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarListaInteresController> log)
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
                "resultadoListaAml": "string"
            
            validacionId: Se debe proporcionar el ID de la validación
            resultadoListaAml: dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
                1: Forza a realizar una validación
                cualquier otro dato: Si existe validación, devuelve el resultado, de lo contrario realiza la validación

            */

            var ser = new JObject
            {
                ["ValidacionId"] = valEntry.ValidacionId,
                ["ResultadoListaAml"] = valEntry.ResultadoListaAml
            };

            _log.LogInformation(ser.ToString(Formatting.None));

            var cronometro = new Stopwatch();
            var envio = new JObject();
            var salida = new JObject();
            Solicitante ItemSolicitante = null;

            cronometro.Start();
            bool revalidar = false;
            string fileName = "";
            ValidationResult validationResult = null;
            string serializeValidationResult = "";
            DateTime fechaTransaccion = DateTime.Now;
            int row_afected = 0;
            bool errorUpdateDb = false;

            if (valEntry.ResultadoListaAml == "1")
                revalidar = true;

            try
            {
                valEntry = _context.Validaciones.Where(x => x.ValidacionId == valEntry.ValidacionId).Include(x => x.Solicitante).FirstOrDefault();
                if (valEntry == null)
                    throw new InvalidOperationException("Error al obtener la información de la solicitud, el id no existe.");
                ItemSolicitante = valEntry.Solicitante;

                if (!revalidar)
                {
                    DateTime fechaValidacion = valEntry.FechaListaAml.GetValueOrDefault();
                    if (fechaValidacion != DateTime.MinValue)
                    {
                        fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, TipoValidacion.LISTA_INTERES);
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
                            fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.LISTA_INTERES);
                            if (System.IO.File.Exists(fileName))
                            {
                                salida = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(fileName));
                                cronometro.Stop();
                                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                                valEntry.FechaListaAml = fechaTransaccion;
                                valEntry.ResultadoListaAml = salida["resultado"].ToString();
                                valEntry.SemaforoListaAml = salida["semaforo"].ToString();

                                validationResult = new ValidationResult()
                                {
                                    ValidacionId = valEntry.ValidacionId,
                                    Semaforo = valEntry.SemaforoListaAml,
                                    Resultado = valEntry.ResultadoListaAml,
                                    Fecha = fechaTransaccion,
                                    ValidacionTipo = TipoValidacion.LISTA_INTERES
                                };

                                (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                                row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                                _log.LogInformation(string.Format("{0} registros afectados validación Listas de interés", row_afected));

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
                envio["Apellidos"] = string.Format("{0} {1}", ItemSolicitante.ApellidoPaterno, ItemSolicitante.ApellidoMaterno);
                envio["Nombres"] = ItemSolicitante.Nombre;

                if (string.IsNullOrEmpty(ItemSolicitante.Curp))
                    throw new InvalidOperationException("La CURP no puede ir vacía.");

                JObject idDeEnvio = new();
                idDeEnvio["Curp"] = ItemSolicitante.Curp;
                //   idDeEnvio["Rfc"] = ItemSolicitante.;
                envio["Identificacion"] = idDeEnvio;

                fechaTransaccion = StaticData.FechaServer(_context);
                if (fechaTransaccion == DateTime.MinValue)
                    throw new InvalidOperationException("Error al obtener la fecha del servidor.");

                IDBiometricsServicesAPI rest = null;
                string result = string.Empty;

#if SERVICES
                rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.CheckAPI,
                    _app.Value.CheckAPIKey,
                    _app.Value.IDBiometricsApiUrl,
                     IDBiometricsServicesAPI.Operacion.ListaAml);

                _log.LogInformation("Inicia transacción a API CheckPerson con json de entrada " + envio.ToString(Formatting.None));

                if (!rest.Call("CheckPerson", true, envio.ToString(), out result))
                    throw new InvalidOperationException("Error en la obtención de información del servicio de verificación de listas de interes.");
                else
#else
                JObject xxx = new JObject();
                JObject yyy = new JObject();
                xxx["message"] = "OK";
                xxx["success"] = true;
               
                yyy["lista"] = "Listado de contribuyentes Artículo 69 del Código Fiscal de la Federación";
                yyy["pais_Lista"] = "MÉXICO";
                yyy["enlace"] = "https://www.prevenciondelavado.com/portal/enlace.aspx?c=L25encGeYnfNyzOryBrUjOprGQ+ah2mxEx6+jO6cbETIr7NvP36hnObacBZO4W+bVWY0VdRbBwU=";
                yyy["exactitud_Denominacion"] = "ALTO(5 sobre 5)";

                xxx["responseData"] = new JArray() { yyy };
               
                result = JsonConvert.SerializeObject(xxx);
                _log.LogInformation("DEBUG PARA NO USAR TRANSACCIONES \n\r" + result);
#endif
                {
                    //procesamos la información obtenida de la API
                    string semaforo = string.Empty;
                    //bool existeAlerta = false;
                    LIST Listas = JsonConvert.DeserializeObject<LIST>(result);

                    JObject data = new();
                    data["dato_usado_rfc"] = string.Empty;
                    data["dato_usado_curp"] = ItemSolicitante.Curp;
                    data["dato_usado_nombre"] =  ItemSolicitante.NombreCompletoSolicitante;

                    cronometro.Stop();
                    salida["error"] = 0;

                    if (Listas.Respuesta.Count > 0)
                    {
                        salida["mensaje"] = string.Format("Se han localizado {0} resultados con la búsqueda solicitada.", Listas.Respuesta.Count);
                        semaforo = "rojo";
                    }
                    else
                    {
                        salida["mensaje"] = string.Format("No se localizaron resultados con la búsqueda solicitada.", Listas.Respuesta.Count);
                        semaforo = "verde";
                    }

                    salida["semaforo"] = semaforo;
                    salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                    salida["validaciones"] = JArray.FromObject(Listas.Respuesta.ToArray());
                    salida["datos_validacion"] = data;

                    salida["resultado"] = true.ToString();
                    valEntry.ResultadoListaAml = true.ToString();

                    valEntry.FechaListaAml = fechaTransaccion;
                    valEntry.SemaforoListaAml = semaforo;

                    // Se identifico que se realizo la consulta, se guardo el archivo en el disco, pero no actualizo los datos en la base
                    validationResult = new ValidationResult()
                    {
                        ValidacionId = valEntry.ValidacionId,
                        Semaforo = valEntry.SemaforoListaAml,
                        Resultado = valEntry.ResultadoListaAml,
                        Fecha = fechaTransaccion,
                        ValidacionTipo = TipoValidacion.LISTA_INTERES
                    };

                    fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.LISTA_INTERES);
                    System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(salida, Formatting.Indented));
                    _log.LogInformation("Genera Json de validación Listas de interés en " + fileName);

                    //_context.Entry(valEntry).State = EntityState.Modified;
                    //row_afected = await _context.SaveChangesAsync();

                    (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                    row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                    _log.LogInformation(string.Format("{0} registros afectados validación Listas de interés", row_afected));

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
