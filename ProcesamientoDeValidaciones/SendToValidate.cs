using ProcesamientoDeValidaciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcesamientoDeValidaciones
{
    public class SendToValidate
    {
        private Validacion _aValidar = null;

        public SendToValidate(Validacion datoVal, Notify notificador)
        {
            _aValidar = datoVal;
            callbackNotify = notificador;
        }

        /// <summary>
        /// Notifica el final de la transacción realizada.
        /// </summary>
        /// <param name="msg"></param>
        public delegate void Notify(string msg);
        private Notify callbackNotify;


        public void DoValidate()
        {
            foreach (TipoVerificacion tipo in Enum.GetValues(typeof(TipoVerificacion)))
            {//realizamos un foreach para cada validación dentro de la tabla de validaciones

                switch(tipo)
                {
                    case TipoVerificacion.CURP:
                        ValidaCurp(_aValidar.ValidacionId);
                        break;
                }
            }

            callbackNotify?.Invoke("");
        }


        public bool ValidaCurp(int validacion_id)
        {//aquí metemos el llamado al controller de validación de curp




            return false;
        }



    }
}
