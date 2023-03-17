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
    public class ValidarCurpController : ControllerBase
    {
        private readonly IDBiometricsMercedezBenzContext _context;
        private readonly IOptions<AppSettings> _app;
        private readonly ILogger<ValidarCurpController> _log;

        public ValidarCurpController(IDBiometricsMercedezBenzContext context, IOptions<AppSettings> cfg, ILogger<ValidarCurpController> log)
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
                "resultadoCurp": "string"
            
            validacionId: Se debe proporcionar el ID de la validación
            resultadoCurp: dado que es un dato string, introducimos un dato entero para volver a validar, para ello:
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

            if (valEntry.ResultadoCurp == "1")
                revalidar = true;

            try
            {
                valEntry = _context.Validaciones.Where(x => x.ValidacionId == valEntry.ValidacionId).Include(x => x.Solicitante).FirstOrDefault();
                if (valEntry == null)
                    throw new InvalidOperationException("Error al obtener la información de la solicitud, el id no existe.");
                ItemSolicitante = valEntry.Solicitante;

                if (!revalidar)
                {
                    DateTime fechaValidacion = valEntry.FechaCurp.GetValueOrDefault();
                    if (fechaValidacion != DateTime.MinValue)
                    {
                        fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaValidacion, TipoValidacion.CURP);
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
                            fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.CURP);
                            if (System.IO.File.Exists(fileName))
                            {
                                salida = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(fileName));
                                cronometro.Stop();
                                salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                                valEntry.FechaCurp = fechaTransaccion;
                                valEntry.ResultadoCurp = salida["resultado"].ToString();
                                valEntry.SemaforoCurp = salida["semaforo"].ToString();

                                validationResult = new ValidationResult()
                                {
                                    ValidacionId = valEntry.ValidacionId,
                                    Semaforo = valEntry.SemaforoCurp,
                                    Resultado = valEntry.ResultadoCurp,
                                    Fecha = fechaTransaccion,
                                    ValidacionTipo = TipoValidacion.CURP
                                };

                                (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                                row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                                _log.LogInformation(string.Format("{0} registros afectados validación Curp", row_afected));

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
                string curp = ItemSolicitante.Curp;
                if (string.IsNullOrEmpty(curp))
                    throw new InvalidOperationException("La CURP no puede ir vacía.");
                envio["Estacion"] = "MercedesBensValidation";
                envio["Curp"] = curp.ToUpper();

                fechaTransaccion = StaticData.FechaServer(_context);
                if (fechaTransaccion == DateTime.MinValue)
                    throw new InvalidOperationException("Error al obtener la fecha del servidor.");

                IDBiometricsServicesAPI rest = null;
                string result = string.Empty;

                rest = new IDBiometricsServicesAPI(IDBiometricsServicesAPI.Tipo.CheckAPI,
                    _app.Value.CheckAPIKey,
                    _app.Value.IDBiometricsApiUrl,
                     IDBiometricsServicesAPI.Operacion.CURP);

                if (!rest.Call("CheckByCURP", true, envio.ToString(), out result))
                    throw new InvalidOperationException("Error en la obtención de información del servicio de Registro Nacional de Población.");
                else
                {
                    //procesamos la información obtenida de la API de RENAPO
                    string semaforo = string.Empty;
                    bool existeAlerta = false;
                    CURP curpi = JsonConvert.DeserializeObject<CURP>(result);

                    var ListaValidaciones = new List<APIValidacion>();
                    JObject data = new();

                    if (!curpi.EsCorrecto)
                    {
                        semaforo = "rojo";
                        ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, curpi.Mensaje));
                     
                        cronometro.Stop();
                        salida["error"] = 0;
                        salida["mensaje"] = curpi.Mensaje;
                        salida["semaforo"] = semaforo;
                        salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                        salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                        data["curp"] = ItemSolicitante.Curp;
                        salida["datos_validacion"] = data;

                        salida["resultado"] = false.ToString();
                        valEntry.ResultadoCurp = false.ToString();
                    }
                    else
                    {
                        if (curpi.ResponseData.AlertaPorEstatus)
                        {
                            semaforo = "rojo";
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.ALERTA, curpi.ResponseData.estatusCurp));
                        }
                        else
                            ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, curpi.ResponseData.estatusCurp));

                        //if (ItemSolicitante.Nombre == curpi.ResponseData.Nombres)
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, "El nombre del solicitante se valido correctamente."));
                        //else
                        //{
                        //    existeAlerta = true;
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, $"El nombre del solicitante no coincide: nombre capturado: {ItemSolicitante.Nombre}, nombre verificado {curpi.ResponseData.Nombres}."));
                        //}
                        //if (ItemSolicitante.ApellidoPaterno == curpi.ResponseData.ApellidoPaterno)
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, "El apellido paterno del solicitante se valido correctamente."));
                        //else
                        //{
                        //    existeAlerta = true;
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, $"El apellido paterno del solicitante no coincide: apellido capturado: {ItemSolicitante.ApellidoPaterno}, apellido verificado {curpi.ResponseData.ApellidoPaterno}."));
                        //}
                        //if (ItemSolicitante.ApellidoMaterno == curpi.ResponseData.ApellidoMaterno)
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, "El apellido materno del solicitante se valido correctamente."));
                        //else
                        //{
                        //    existeAlerta = true;
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, $"El apellido materno del solicitante no coincide: apellido capturado: {ItemSolicitante.ApellidoMaterno}, apellido verificado {curpi.ResponseData.ApellidoMaterno}."));
                        //}
                        //if (ItemSolicitante.FechaNacimiento == curpi.ResponseData.FechaNacimiento)
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.OK, "La fecha de nacimiento del solicitante se valido correctamente."));
                        //else
                        //{
                        //    existeAlerta = true;
                        //    ListaValidaciones.Add(new APIValidacion(TipoAlerta.AVISO, $"La fecha de nacimiento no coincide: fecha capturada: {ItemSolicitante.FechaNacimiento?.ToString("dd/MM/yyyy")}, fecha verificada {curpi.ResponseData.FechaNacimiento.ToString("dd/MM/yyyy")}."));
                        //}

                        data["nombre"] = curpi.ResponseData.Nombres;
                        data["paterno"] = curpi.ResponseData.ApellidoPaterno;
                        data["materno"] = curpi.ResponseData.ApellidoMaterno;
                        data["fecha_nacimiento"] = curpi.ResponseData.FechaNacimiento.ToString("dd/MM/yyyy");
                        data["curp"] = curpi.ResponseData.CURP;

                        data["sexo"] = curpi.ResponseData.Sexo;
                        data["estado_nacimiento"] = curpi.ResponseData.EstadoNacimiento;
                        data["pais_nacimiento"] = curpi.ResponseData.PaisNacimiento;
                        data["clave_pais_nacimiento"] = curpi.ResponseData.ClavePaisNacimiento;

                        data["entidad_registro"] = curpi.ResponseData.NumeroEntidad;

                        if (curpi.ResponseData.ConDocumentoProbatorio >= 1)
                        {
                            data["municipio_registro"] = curpi.ResponseData.DocumentoProbatorio.MunicipioRegistro;

                            data["anio_registro"] = curpi.ResponseData.DocumentoProbatorio.AnioRegistro;
                            data["numero_acta"] = curpi.ResponseData.DocumentoProbatorio.NumeroDeActa;
                        }

                        if (existeAlerta)
                        {//posible amarillo
                            if (string.IsNullOrEmpty(semaforo))
                                semaforo = "amarillo";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(semaforo))
                                semaforo = "verde";
                        }

                        cronometro.Stop();
                        salida["error"] = 0;
                        salida["mensaje"] = curpi.Mensaje;
                        salida["semaforo"] = semaforo;
                        salida["tiempo_espera"] = cronometro.ElapsedMilliseconds;
                        salida["validaciones"] = JArray.FromObject(ListaValidaciones.ToArray());
                        salida["datos_validacion"] = data;

                        salida["resultado"] = true.ToString();
                        valEntry.ResultadoCurp = true.ToString();
                    }

                    valEntry.FechaCurp = fechaTransaccion;
                    valEntry.SemaforoCurp = semaforo;

                    // Se identifico que se realizo la consulta, se guardo el archivo en el disco, pero no actualizo los datos en la base
                    validationResult = new ValidationResult()
                    {
                        ValidacionId = valEntry.ValidacionId,
                        Semaforo = valEntry.SemaforoCurp,
                        Resultado = valEntry.ResultadoCurp,
                        Fecha = fechaTransaccion,
                        ValidacionTipo = TipoValidacion.CURP
                    };

                    fileName = StaticData.GetFileName(_app.Value.RepositoryPath, valEntry.ValidacionId, fechaTransaccion, TipoValidacion.CURP);
                    System.IO.File.WriteAllText(fileName, JsonConvert.SerializeObject(salida, Formatting.Indented));
                    _log.LogInformation("Genera Json de validación Curp en " + fileName);

                    //_context.Entry(valEntry).State = EntityState.Modified;
                    //row_afected = await _context.SaveChangesAsync();

                    (string qry, NpgsqlParameter[] parameterToArray) = StaticData.QryUpdateValidacion(validationResult);
                    row_afected = await _context.Database.ExecuteSqlRawAsync(qry, parameterToArray);
                    _log.LogInformation(string.Format("{0} registros afectados validación Curp", row_afected));

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
