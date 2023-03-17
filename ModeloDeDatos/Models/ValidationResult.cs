using ModeloDeDatos.Clases;
using System;

namespace ModeloDeDatos.Models
{
    public class ValidationResult
    {
        public int ValidacionId { get; set; }
        public string Semaforo { get; set; }
        public string Resultado { get; set; }
        public DateTime Fecha { get; set; }
        public TipoValidacion ValidacionTipo { get; set; }
    }
}
