using System;
using System.Collections.Generic;



namespace ProcesamientoDeValidaciones.Models
{
    public partial class Validacion
    {
        public int ValidacionId { get; set; }
        public int SolicitanteId { get; set; }
        public int? UsuarioConsultaId { get; set; }
        public DateTime? FechaConsulta { get; set; }
        public string SemaforoListaNegra { get; set; }
        public string ResultadoListaNegra { get; set; }
        public DateTime? FechaListaNegra { get; set; }
        public string SemaforoIbms { get; set; }
        public string ResultadoIbms { get; set; }
        public DateTime? FechaIbms { get; set; }
        public string SemaforoIdentificacion { get; set; }
        public string ResultadoIdentificacion { get; set; }
        public DateTime? FechaIdentificacion { get; set; }
        public string SemaforoIne { get; set; }
        public string ResultadoIne { get; set; }
        public DateTime? FechaIne { get; set; }
        public string SemaforoFacial { get; set; }
        public string ResultadoFacial { get; set; }
        public DateTime? FechaFacial { get; set; }
        public string SemaforoCorreo { get; set; }
        public string ResultadoCorreo { get; set; }
        public DateTime? FechaCorreo { get; set; }
        public string SemaforoTelefono { get; set; }
        public string ResultadoTelefono { get; set; }
        public DateTime? FechaTelefono { get; set; }
        public string SemaforoCurp { get; set; }
        public string ResultadoCurp { get; set; }
        public DateTime? FechaCurp { get; set; }
        public string SemaforoComprobanteDomicilio { get; set; }
        public string ResultadoComprobanteDomicilio { get; set; }
        public DateTime? FechaComprobanteDomicilio { get; set; }
        public string SemaforoComprobanteIngresos { get; set; }
        public string ResultadoComprobanteIngresos { get; set; }
        public DateTime? FechaComprobanteIngresos { get; set; }
        public string SemaforoListaAml { get; set; }
        public string ResultadoListaAml { get; set; }
        public DateTime? FechaListaAml { get; set; }

    }
}
