using Microsoft.EntityFrameworkCore;
using ModeloDeDatos.Context;
using ModeloDeDatos.Models;
using Npgsql;
using System;
using System.IO;
using System.Linq;

namespace ModeloDeDatos.Clases
{
    public class AppSettings
    {
        public string IDBiometricsApiUrl { get; set; }
        public string IDBiometricsApiUrlTest { get; set; }
        public string DigiAPIKey { get; set; }
        public string BioAPI { get; set; }
        public string CheckAPIKey { get; set; }
        public string RepositoryPath { get; set; }
    }

    public static class StaticData
    {
        /// <summary>
        /// Obtiene el archivo donde se guardará el resultado y genera las carpetas con el path principal, las carpetas están basadas en el path de la fecha del resultado de la validación correspondiente
        /// de la tabla de validaciones de la base de datos
        /// </summary>
        /// <param name="RutaPrincipal">Path principal de la aplicación obtenido del evento estático RepositorioDeResultados </param>
        /// <param name="validacion_id">El identificador de la validación para generar el nombre del archivo</param>
        /// <param name="fecha_dinamica">La fecha en que se está haciendo la validación DateTime.Now (se sugiere cambiar por el de la base de datos)</param>
        /// <returns></returns>
        public static string GetFileName(string RutaPrincipal,int  validacion_id, DateTime fecha_dinamica, TipoValidacion validacion)
        {

            string carpeta_year = fecha_dinamica.Year.ToString().PadLeft(4, '0');
            string carpeta_month = fecha_dinamica.Month.ToString().PadLeft(4, '0');
            string carpeta_day = fecha_dinamica.Day.ToString().PadLeft(4, '0');

            if (!Directory.Exists(Path.Combine(RutaPrincipal, carpeta_year)))
            {
                if (!Directory.Exists(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month)))
                {
                    if (!Directory.Exists(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month, carpeta_day)))
                    {
                        Directory.CreateDirectory(Path.Combine(RutaPrincipal, carpeta_year));
                        Directory.CreateDirectory(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month));
                        Directory.CreateDirectory(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month, carpeta_day));
                    }
                }
            }
            else
            {
                if (!Directory.Exists(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month)))
                {
                    if (!Directory.Exists(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month, carpeta_day)))
                    {
                        Directory.CreateDirectory(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month));
                        Directory.CreateDirectory(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month, carpeta_day));
                    }
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month, carpeta_day)))
                    {
                        Directory.CreateDirectory(Path.Combine(RutaPrincipal, carpeta_year, carpeta_month, carpeta_day));
                    }
                }
            }
            string fileName = string.Format("{0}\\{2}{1}.json", Path.Combine(RutaPrincipal, carpeta_year, carpeta_month, carpeta_day), validacion_id.ToString().PadLeft(12, '0'), ((int)validacion).ToString().PadLeft(2, '0'));

            return fileName;
        }

        public static DateTime FechaServer(IDBiometricsMercedezBenzContext _context)
        {
            try
            {
                return _context.Set<ATime>().FromSqlRaw("SELECT NOW() as fecha_server").FirstOrDefault().FechaServer;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return DateTime.MinValue;
            }
        }

        public static (string qry, NpgsqlParameter[] parameterToArray) QryUpdateValidacion(ValidationResult validation)
        {
            string campoSemaforo = "";
            string campoResultado;
            string campoFecha;

            NpgsqlParameter[] parameterToArray = {
                new NpgsqlParameter("@ValidacionId", validation.ValidacionId),
                new NpgsqlParameter("@Semaforo", string.IsNullOrEmpty(validation.Semaforo) ? "NA" : validation.Semaforo),
                new NpgsqlParameter("@Resultado", validation.Resultado),
                new NpgsqlParameter("@Fecha", validation.Fecha)
            };

            switch (validation.ValidacionTipo)
            {
                case TipoValidacion.INE:  // OK
                    campoSemaforo = "Semaforo_Ine";
                    campoResultado = "Resultado_Ine";
                    campoFecha = "Fecha_Ine";
                    break;
                case TipoValidacion.CORREO:   // OK
                    campoSemaforo = "Semaforo_Correo";
                    campoResultado = "Resultado_Correo";
                    campoFecha = "Fecha_Correo";
                    break;
                case TipoValidacion.TELEFONO: // OK
                    campoSemaforo = "Semaforo_Telefono";
                    campoResultado = "Resultado_Telefono";
                    campoFecha = "Fecha_Telefono";
                    break;
                case TipoValidacion.CURP: // OK
                    campoSemaforo = "Semaforo_Curp";
                    campoResultado = "Resultado_Curp";
                    campoFecha = "Fecha_Curp";
                    break;
                case TipoValidacion.COMPROBANTE_DOMICILIO: // OK
                    campoSemaforo = "Semaforo_Comprobante_Domicilio";
                    campoResultado = "Resultado_Comprobante_Domicilio";
                    campoFecha = "Fecha_Comprobante_Domicilio";
                    break;
                case TipoValidacion.COMPROBANTE_INGRESOS:  // OK
                    campoSemaforo = "Semaforo_Comprobante_Ingresos";
                    campoResultado = "Resultado_Comprobante_Ingresos";
                    campoFecha = "Fecha_Comprobante_Ingresos";
                    break;
                case TipoValidacion.COMPROBANTE_BANCARIO:  // OK
                    campoSemaforo = "Semaforo_Comprobante_Bancario";
                    campoResultado = "Resultado_Comprobante_Bancario";
                    campoFecha = "Fecha_Comprobante_Bancario";
                    break;
                case TipoValidacion.LISTA_INTERES:    // OK
                    campoSemaforo = "Semaforo_Lista_Aml";
                    campoResultado = "Resultado_Lista_Aml";
                    campoFecha = "Fecha_Lista_Aml";
                    break;
                case TipoValidacion.GEOREFERENCIA:    // OK
                    campoResultado = "Resultado_Georeferencia";
                    campoFecha = "Fecha_Georeferencia";
                    break;
                default:
                    throw new InvalidOperationException("El Tipo de validación es incorrecto");
            }

            string qrySemaforo = string.IsNullOrEmpty(campoSemaforo) ? "" : $" {campoSemaforo} = @Semaforo, ";
            string qry = $"UPDATE Validaciones SET {qrySemaforo} {campoResultado} = @Resultado, {campoFecha} = @Fecha WHERE Validacion_Id = @ValidacionId;";

            return (qry, parameterToArray);
        }

    }

}
