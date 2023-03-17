using ProcesamientoDeValidaciones.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcesamientoDeValidaciones
{
    public enum TipoVerificacion
    {
        CURP, CORREO, TELEFONO, COMPROBANTE_DOMICILIO, COMPROBANTE_INGRESOS, LISTA_NEGRA, PEPS
    }
    public class Validaciones
    {
        private Thread thValidaciones = null;
        public event Action<string, bool> ProcesaMensaje;




        public bool IniciaProcesadoValidaciones()
        {
            try
            {
                this.thValidaciones = new Thread(new ThreadStart(this.ProcesaValidaciones));
                this.thValidaciones.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ProcesaValidaciones()
        {
            //aquí inicamos el proceso de adquisicion de tareas
            LatinID.Utilidades.WebServices.Caller caller = new LatinID.Utilidades.WebServices.Caller(ConfigurationManager.AppSettings["url"]);// "https://localhost:44389/");
            List<Solicitante> datos = caller.GetCatalog<Solicitante>("Solicitantes", ConfigurationManager.AppSettings["transacciones"]).ToList();
            ProcesaMensaje("Se procesarán las siguiente solicitudes: " + string.Join(", ", datos.Select(x => x.SolicitanteId).ToArray()), false);
            foreach (Solicitante s in datos)
            {
                ProcesaMensaje($"Procesando solicitud: {s.SolicitanteId}.", false);
                ThreadPool.QueueUserWorkItem(EjecutaTransaccion, s);
            }
        }


        private void EjecutaTransaccion(Object stateInfo)
        {
            Solicitante item = (Solicitante)stateInfo;
            Validacion v = null;
            if (item.Validaciones.Count >= 1)
                v = item.Validaciones.FirstOrDefault();
            else
            {
                //no existe el registro de validación
                return;
            }

            //iniciamos un nuevo hilo para realizar las validaciones
            SendToValidate envio = new SendToValidate(v, NotificarEnvio);

            ThreadStart threadDelegate = new ThreadStart(envio.DoValidate);
            Thread HiloValidacion = new Thread(threadDelegate);
            HiloValidacion.Start();//iniciamos con las validaciones

           






            //VALIDACIÓN DE CURP
            if (string.IsNullOrEmpty(v.ResultadoCurp))
            {//aún no se ha validado

            }
            else
            {//ya se valido el curp

            }

            //VALIDACIÓN DEL CORREO ELECTRÓNICO
            if (string.IsNullOrEmpty(v.ResultadoCorreo))
            {//aún no se ha validado

            }
            else
            {//ya se valido

            }


            //VALIDACIÓN DEL TELÉFONO
            if (string.IsNullOrEmpty(v.ResultadoTelefono))
            {//aún no se ha validado

            }
            else
            {//ya se valido

            }



            //VALIDACIÓN DEL COMPROBANTE DE DOMICILIO
            if (string.IsNullOrEmpty(v.ResultadoComprobanteDomicilio))
            {//aún no se ha validado

            }
            else
            {//ya se valido

            }



            //VALIDACIÓN DEL COMPROBANTE DE INGRESOS
            if (string.IsNullOrEmpty(v.ResultadoComprobanteIngresos))
            {//aún no se ha validado

            }
            else
            {//ya se valido

            }


           //TODO: aquí iniciamos con las validaciones de cada solicitante

            ProcesaMensaje($"Thread No. {Thread.CurrentThread.ManagedThreadId} ha comenzado la tarea del solicitante_id:  {item.NombreCompletoSolicitante}\r\n", false);
            Thread.Sleep(1500);
            ProcesaMensaje($"Thread No. {Thread.CurrentThread.ManagedThreadId} ha terminado la terea del solicitante_id: {item.NombreCompletoSolicitante}\r\n", false);

        }
        public ThreadState getEstatusHiloEnvio() =>
           (ThreadState)(this.thValidaciones?.ThreadState);

        //procesa los mensajes de salida del thread de validación
        private void NotificarEnvio(string mensaje) => 
            ProcesaMensaje($"Thread de notificación No. {Thread.CurrentThread.ManagedThreadId} " + mensaje + "\r\n", false);
        
    }
}
