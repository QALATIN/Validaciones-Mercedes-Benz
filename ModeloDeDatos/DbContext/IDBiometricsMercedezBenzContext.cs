using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModeloDeDatos.Models;

namespace ModeloDeDatos.Context
{
    public partial class IDBiometricsMercedezBenzContext : DbContext
    {
        public IDBiometricsMercedezBenzContext()
        {
        }

        public IDBiometricsMercedezBenzContext(DbContextOptions<IDBiometricsMercedezBenzContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Agencia> Agencias { get; set; }
        public virtual DbSet<Agenciastipo> Agenciastipos { get; set; }
        public virtual DbSet<Avisosprivacidad> Avisosprivacidads { get; set; }
        public virtual DbSet<Capturasidentificacion> Capturasidentificacions { get; set; }
        public virtual DbSet<Capturasnombre> Capturasnombres { get; set; }
        public virtual DbSet<Dedosestatus> Dedosestatuses { get; set; }
        public virtual DbSet<Dedosindie> Dedosindice { get; set; }
        public virtual DbSet<Documento> Documentos { get; set; }
        public virtual DbSet<Estado> Estados { get; set; }
        public virtual DbSet<Foto> Fotos { get; set; }
        public virtual DbSet<Fotosorigen> Fotosorigens { get; set; }
        public virtual DbSet<Huella> Huellas { get; set; }
        public virtual DbSet<Identificacione> Identificaciones { get; set; }
        public virtual DbSet<Listanegra> Listanegras { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Perfile> Perfiles { get; set; }
        public virtual DbSet<Resolucion> Resolucions { get; set; }
        public virtual DbSet<Semaforo> Semaforos { get; set; }
        public virtual DbSet<Solicitante> Solicitantes { get; set; }
        public virtual DbSet<Tiposresolucion> Tiposresolucions { get; set; }
        public virtual DbSet<Tmpbusqueda> Tmpbusquedas { get; set; }
        public virtual DbSet<Tmpreportebitacora> Tmpreportebitacoras { get; set; }
        public virtual DbSet<Tmpreportedetalleenvio> Tmpreportedetalleenvios { get; set; }
        public virtual DbSet<Tmpreportelistanegra> Tmpreportelistanegras { get; set; }
        public virtual DbSet<Tmpreportesemaforo> Tmpreportesemaforos { get; set; }
        public virtual DbSet<Tmpreportesemaforofacialdetalle> Tmpreportesemaforofacialdetalles { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<VSolicitante> VSolicitantes { get; set; }
        public virtual DbSet<Validacion> Validaciones { get; set; }

        public virtual DbSet<ATime> HoraServidor { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();
                var connectionString = configuration.GetConnectionString("ConexionModelo");
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Spanish_Mexico.1252");

            modelBuilder.Entity<ATime>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.FechaServer).HasColumnName("fecha_server");

            });

            modelBuilder.Entity<Agencia>(entity =>
            {
                entity.ToTable("agencias");

                entity.Property(e => e.AgenciaId).HasColumnName("agencia_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.ClaveAgencia)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("clave_agencia");

                entity.Property(e => e.Direccion)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnName("direccion");

                entity.Property(e => e.EstadoId).HasColumnName("estado_id");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.NombreAgencia)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("nombre_agencia");

                entity.Property(e => e.Telefono)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("telefono");

                entity.Property(e => e.TipoAgenciaId).HasColumnName("tipo_agencia_id");

                entity.HasOne(d => d.Estado)
                    .WithMany(p => p.Agencia)
                    .HasForeignKey(d => d.EstadoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("agencias_estado_id_fkey");
            });

            modelBuilder.Entity<Agenciastipo>(entity =>
            {
                entity.HasKey(e => e.TipoAgenciaId)
                    .HasName("agenciastipo_pkey");

                entity.ToTable("agenciastipo");

                entity.Property(e => e.TipoAgenciaId)
                    .ValueGeneratedNever()
                    .HasColumnName("tipo_agencia_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.TipoAgenciaNombre)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("tipo_agencia_nombre");
            });

            modelBuilder.Entity<Avisosprivacidad>(entity =>
            {
                entity.HasKey(e => e.AvisoPrivacidadId)
                    .HasName("avisosprivacidad_pkey");

                entity.ToTable("avisosprivacidad");

                entity.Property(e => e.AvisoPrivacidadId).HasColumnName("aviso_privacidad_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.Imagen)
                    .IsRequired()
                    .HasColumnName("imagen");

                entity.Property(e => e.Referencia)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnName("referencia");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Avisosprivacidads)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("avisosprivacidad_solicitante_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Avisosprivacidads)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("avisosprivacidad_usuario_id_fkey");
            });

            modelBuilder.Entity<Capturasidentificacion>(entity =>
            {
                entity.HasKey(e => e.CapturaIdentificacionId)
                    .HasName("capturasidentificacion_pkey");

                entity.ToTable("capturasidentificacion");

                entity.Property(e => e.CapturaIdentificacionId).HasColumnName("captura_identificacion_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.CapturaNombreId).HasColumnName("captura_nombre_id");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.Imagen)
                    .IsRequired()
                    .HasColumnName("imagen");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.CapturaNombre)
                    .WithMany(p => p.Capturasidentificacions)
                    .HasForeignKey(d => d.CapturaNombreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("capturasidentificacion_captura_nombre_id_fkey");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Capturasidentificacions)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("capturasidentificacion_solicitante_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Capturasidentificacions)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("capturasidentificacion_usuario_id_fkey");
            });

            modelBuilder.Entity<Capturasnombre>(entity =>
            {
                entity.HasKey(e => e.CapturaNombreId)
                    .HasName("capturasnombre_pkey");

                entity.ToTable("capturasnombre");

                entity.Property(e => e.CapturaNombreId)
                    .ValueGeneratedNever()
                    .HasColumnName("captura_nombre_id");

                entity.Property(e => e.CapturaNombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("captura_nombre");
            });

            modelBuilder.Entity<Dedosestatus>(entity =>
            {
                entity.HasKey(e => e.DedoEstatusId)
                    .HasName("dedosestatus_pkey");

                entity.ToTable("dedosestatus");

                entity.Property(e => e.DedoEstatusId)
                    .ValueGeneratedNever()
                    .HasColumnName("dedo_estatus_id");

                entity.Property(e => e.DedoEstatusClave)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("dedo_estatus_clave");

                entity.Property(e => e.DedoEstatusNombre)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("dedo_estatus_nombre");
            });

            modelBuilder.Entity<Dedosindie>(entity =>
            {
                entity.HasKey(e => e.DedoIndiceId)
                    .HasName("dedosindice_pkey");

                entity.ToTable("dedosindice");

                entity.Property(e => e.DedoIndiceId)
                    .ValueGeneratedNever()
                    .HasColumnName("dedo_indice_id");

                entity.Property(e => e.DedoIndiceNombre)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("dedo_indice_nombre");
            });

            modelBuilder.Entity<Documento>(entity =>
            {
                entity.ToTable("documentos");

                entity.Property(e => e.DocumentoId).HasColumnName("documento_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.Imagen)
                    .IsRequired()
                    .HasColumnName("imagen");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.TipoDocumento)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("tipo_documento");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Documentos)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("documentos_solicitante_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Documentos)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("documentos_usuario_id_fkey");

                entity.Property(e => e.NombreDocumento).HasColumnName("nombre_documento");
            });

            modelBuilder.Entity<Estado>(entity =>
            {
                entity.ToTable("estados");

                entity.Property(e => e.EstadoId)
                    .ValueGeneratedNever()
                    .HasColumnName("estado_id");

                entity.Property(e => e.NombreEstado)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("nombre_estado");
            });

            modelBuilder.Entity<Foto>(entity =>
            {
                entity.ToTable("fotos");

                entity.Property(e => e.FotoId).HasColumnName("foto_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.FotoOrigenId).HasColumnName("foto_origen_id");

                entity.Property(e => e.Guid).HasColumnName("guid");

                entity.Property(e => e.Imagen)
                    .IsRequired()
                    .HasColumnName("imagen");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.FotoOrigen)
                    .WithMany(p => p.Fotos)
                    .HasForeignKey(d => d.FotoOrigenId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fotos_foto_origen_id_fkey");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Fotos)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fotos_solicitante_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Fotos)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fotos_usuario_id_fkey");
            });

            modelBuilder.Entity<Fotosorigen>(entity =>
            {
                entity.HasKey(e => e.FotoOrigenId)
                    .HasName("fotosorigen_pkey");

                entity.ToTable("fotosorigen");

                entity.Property(e => e.FotoOrigenId)
                    .ValueGeneratedNever()
                    .HasColumnName("foto_origen_id");

                entity.Property(e => e.FotoOrigenNombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("foto_origen_nombre");
            });

            modelBuilder.Entity<Huella>(entity =>
            {
                entity.ToTable("huellas");

                entity.Property(e => e.HuellaId).HasColumnName("huella_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.DedoEstatusId).HasColumnName("dedo_estatus_id");

                entity.Property(e => e.DedoIndiceId).HasColumnName("dedo_indice_id");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.Imagen).HasColumnName("imagen");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.DedoEstatus)
                    .WithMany(p => p.Huellas)
                    .HasForeignKey(d => d.DedoEstatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("huellas_dedo_estatus_id_fkey");

                entity.HasOne(d => d.DedoIndice)
                    .WithMany(p => p.Huellas)
                    .HasForeignKey(d => d.DedoIndiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("huellas_dedo_indice_id_fkey");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Huellas)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("huellas_solicitante_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Huellas)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("huellas_usuario_id_fkey");
            });

            modelBuilder.Entity<Identificacione>(entity =>
            {
                entity.HasKey(e => e.IdentificacionId)
                    .HasName("identificaciones_pkey");

                entity.ToTable("identificaciones");

                entity.Property(e => e.IdentificacionId).HasColumnName("identificacion_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.AnioRegistro)
                    .HasMaxLength(50)
                    .HasColumnName("anio_registro");

                entity.Property(e => e.Cic)
                    .HasMaxLength(50)
                    .HasColumnName("cic");

                entity.Property(e => e.ClaveElector)
                    .HasMaxLength(50)
                    .HasColumnName("clave_elector");

                entity.Property(e => e.Emision)
                    .HasMaxLength(50)
                    .HasColumnName("emision");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.IdentificadorCiudadano)
                    .HasMaxLength(50)
                    .HasColumnName("identificador_ciudadano");

                entity.Property(e => e.Mrz)
                    .HasMaxLength(500)
                    .HasColumnName("mrz");

                entity.Property(e => e.NumeroEmision)
                    .HasMaxLength(50)
                    .HasColumnName("numero_emision");

                entity.Property(e => e.Ocr)
                    .HasMaxLength(50)
                    .HasColumnName("ocr");

                entity.Property(e => e.Serie)
                    .HasMaxLength(50)
                    .HasColumnName("serie");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.Vigencia)
                    .HasMaxLength(50)
                    .HasColumnName("vigencia");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Identificaciones)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("identificaciones_solicitante_id_fkey");

                entity.Property(e => e.TipoIne).HasColumnName("tipo_ine");
            });

            modelBuilder.Entity<Listanegra>(entity =>
            {
                entity.ToTable("listanegra");

                entity.Property(e => e.ListaNegraId).HasColumnName("lista_negra_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.Motivo)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnName("motivo");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.TipoMovimientoId).HasColumnName("tipo_movimiento_id");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Listanegras)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("listanegra_solicitante_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Listanegras)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("listanegra_usuario_id_fkey");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("logs");

                entity.Property(e => e.Exception).HasColumnName("exception");

                entity.Property(e => e.Level).HasColumnName("level");

                entity.Property(e => e.LogEvent)
                    .HasColumnType("jsonb")
                    .HasColumnName("log_event");

                entity.Property(e => e.Message).HasColumnName("message");

                entity.Property(e => e.MessageTemplate).HasColumnName("message_template");

                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            });

            modelBuilder.Entity<Perfile>(entity =>
            {
                entity.HasKey(e => e.PerfilId)
                    .HasName("perfiles_pkey");

                entity.ToTable("perfiles");

                entity.Property(e => e.PerfilId)
                    .ValueGeneratedNever()
                    .HasColumnName("perfil_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.NombrePerfil)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("nombre_perfil");
            });

            modelBuilder.Entity<Resolucion>(entity =>
            {
                entity.ToTable("resolucion");

                entity.Property(e => e.ResolucionId).HasColumnName("resolucion_id");

                entity.Property(e => e.Comentario)
                    .HasMaxLength(500)
                    .HasColumnName("comentario");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.TipoResolucionId).HasColumnName("tipo_resolucion_id");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Resolucions)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("resolucion_solicitante_id_fkey");

                entity.HasOne(d => d.TipoResolucion)
                    .WithMany(p => p.Resolucions)
                    .HasForeignKey(d => d.TipoResolucionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("resolucion_tipo_resolucion_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Resolucions)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("resolucion_usuario_id_fkey");
            });

            modelBuilder.Entity<Semaforo>(entity =>
            {
                entity.ToTable("semaforos");

                entity.Property(e => e.SemaforoId).HasColumnName("semaforo_id");

                entity.Property(e => e.FechaComprobanteDomicilio).HasColumnName("fecha_comprobante_domicilio");

                entity.Property(e => e.FechaComprobanteIngresos).HasColumnName("fecha_comprobante_ingresos");

                entity.Property(e => e.FechaConsulta).HasColumnName("fecha_consulta");

                entity.Property(e => e.FechaCorreo).HasColumnName("fecha_correo");

                entity.Property(e => e.FechaCurp).HasColumnName("fecha_curp");

                entity.Property(e => e.FechaFacial).HasColumnName("fecha_facial");

                entity.Property(e => e.FechaIbms).HasColumnName("fecha_ibms");

                entity.Property(e => e.FechaIdentificacion).HasColumnName("fecha_identificacion");

                entity.Property(e => e.FechaIne).HasColumnName("fecha_ine");

                entity.Property(e => e.FechaListaAml).HasColumnName("fecha_lista_aml");

                entity.Property(e => e.FechaListaNegra).HasColumnName("fecha_lista_negra");

                entity.Property(e => e.FechaTelefono).HasColumnName("fecha_telefono");

                entity.Property(e => e.ResultadoComprobanteDomicilio)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_comprobante_domicilio");

                entity.Property(e => e.ResultadoComprobanteIngresos)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_comprobante_ingresos");

                entity.Property(e => e.ResultadoCorreo)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_correo");

                entity.Property(e => e.ResultadoCurp)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_curp");

                entity.Property(e => e.ResultadoFacial)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_facial");

                entity.Property(e => e.ResultadoIbms)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_ibms");

                entity.Property(e => e.ResultadoIdentificacion)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_identificacion");

                entity.Property(e => e.ResultadoIne)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_ine");

                entity.Property(e => e.ResultadoListaAml)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_lista_aml");

                entity.Property(e => e.ResultadoListaNegra)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_lista_negra");

                entity.Property(e => e.ResultadoTelefono)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_telefono");

                entity.Property(e => e.SemaforoComprobanteDomicilio)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_comprobante_domicilio");

                entity.Property(e => e.SemaforoComprobanteIngresos)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_comprobante_ingresos");

                entity.Property(e => e.SemaforoCorreo)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_correo");

                entity.Property(e => e.SemaforoCurp)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_curp");

                entity.Property(e => e.SemaforoFacial)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_facial");

                entity.Property(e => e.SemaforoIbms)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_ibms");

                entity.Property(e => e.SemaforoIdentificacion)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_identificacion");

                entity.Property(e => e.SemaforoIne)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_ine");

                entity.Property(e => e.SemaforoListaAml)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_lista_aml");

                entity.Property(e => e.SemaforoListaNegra)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_lista_negra");

                entity.Property(e => e.SemaforoTelefono)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_telefono");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.UsuarioIdConsulta).HasColumnName("usuario_id_consulta");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Semaforos)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("semaforos_solicitante_id_fkey");
            });

            modelBuilder.Entity<Solicitante>(entity =>
            {
                entity.ToTable("solicitantes");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.AgenciaId).HasColumnName("agencia_id");

                entity.Property(e => e.ApellidoMaterno)
                    .HasMaxLength(50)
                    .HasColumnName("apellido_materno");

                entity.Property(e => e.ApellidoPaterno)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("apellido_paterno");

                entity.Property(e => e.CalleNumero)
                    .HasMaxLength(200)
                    .HasColumnName("calle_numero");

                entity.Property(e => e.CodigoPostal)
                    .HasMaxLength(50)
                    .HasColumnName("codigo_postal");

                entity.Property(e => e.Colonia)
                    .HasMaxLength(100)
                    .HasColumnName("colonia");

                entity.Property(e => e.CoordenadasGps)
                    .HasMaxLength(50)
                    .HasColumnName("coordenadas_gps");

                entity.Property(e => e.CorreoElectronico)
                    .HasMaxLength(250)
                    .HasColumnName("correo_electronico");

                entity.Property(e => e.Curp)
                    .HasMaxLength(25)
                    .HasColumnName("curp");

                entity.Property(e => e.DireccionCompleta)
                    .HasMaxLength(500)
                    .HasColumnName("direccion_completa");

                entity.Property(e => e.Edad)
                    .HasMaxLength(10)
                    .HasColumnName("edad");

                entity.Property(e => e.Estado)
                    .HasMaxLength(100)
                    .HasColumnName("estado");

                entity.Property(e => e.Estatus)
                    .HasMaxLength(30)
                    .HasColumnName("estatus");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.FechaNacimiento)
                    .HasColumnType("date")
                    .HasColumnName("fecha_nacimiento");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.Folio)
                    .HasMaxLength(30)
                    .HasColumnName("folio");

                entity.Property(e => e.Guid).HasColumnName("guid");

                entity.Property(e => e.LugarNacimiento)
                    .HasMaxLength(150)
                    .HasColumnName("lugar_nacimiento");

                entity.Property(e => e.Municipio)
                    .HasMaxLength(100)
                    .HasColumnName("municipio");

                entity.Property(e => e.Nacionalidad)
                    .HasMaxLength(50)
                    .HasColumnName("nacionalidad");

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("nombre");

                entity.Property(e => e.NombreCompletoSolicitante)
                    .HasMaxLength(200)
                    .HasColumnName("nombre_completo_solicitante");

                entity.Property(e => e.NumeroDocumento)
                    .HasMaxLength(50)
                    .HasColumnName("numero_documento");

                entity.Property(e => e.PruebaVida)
                    .HasMaxLength(50)
                    .HasColumnName("prueba_vida");

                entity.Property(e => e.ResultadoComparacionFacial)
                    .HasMaxLength(50)
                    .HasColumnName("resultado_comparacion_facial");

                entity.Property(e => e.ResultadoGeneral)
                    .HasMaxLength(50)
                    .HasColumnName("resultado_general");

                entity.Property(e => e.ScoreComparacionFacial)
                    .HasMaxLength(50)
                    .HasColumnName("score_comparacion_facial");

                entity.Property(e => e.Sexo)
                    .HasMaxLength(50)
                    .HasColumnName("sexo");

                entity.Property(e => e.SolicitanteIdOtro).HasColumnName("solicitante_id_origen");

                entity.Property(e => e.Telefono)
                    .HasMaxLength(15)
                    .HasColumnName("telefono");

                entity.Property(e => e.TipoCliente)
                    .HasMaxLength(50)
                    .HasColumnName("tipo_cliente");

                entity.Property(e => e.TipoDocumento)
                    .HasMaxLength(50)
                    .HasColumnName("tipo_documento");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.HasOne(d => d.Agencia)
                    .WithMany(p => p.Solicitantes)
                    .HasForeignKey(d => d.AgenciaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("solicitantes_agencia_id_fkey");

                entity.HasOne(d => d.Usuario)
                    .WithMany(p => p.Solicitantes)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("solicitantes_usuario_id_fkey");
            });

            modelBuilder.Entity<Tiposresolucion>(entity =>
            {
                entity.HasKey(e => e.TipoResolucionId)
                    .HasName("tiposresolucion_pkey");

                entity.ToTable("tiposresolucion");

                entity.Property(e => e.TipoResolucionId)
                    .ValueGeneratedNever()
                    .HasColumnName("tipo_resolucion_id");

                entity.Property(e => e.TipoResolucionNombre)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("tipo_resolucion_nombre");
            });

            modelBuilder.Entity<Tmpbusqueda>(entity =>
            {
                entity.HasKey(e => e.Paqueteid)
                    .HasName("tmpbusquedas_pkey");

                entity.ToTable("tmpbusquedas");

                entity.Property(e => e.Paqueteid)
                    .ValueGeneratedNever()
                    .HasColumnName("paqueteid");

                entity.Property(e => e.Apellidomaterno)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("apellidomaterno");

                entity.Property(e => e.Apellidopaterno)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("apellidopaterno");

                entity.Property(e => e.Claveagencia)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("claveagencia");

                entity.Property(e => e.Contrato)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("contrato");

                entity.Property(e => e.Curp)
                    .IsRequired()
                    .HasMaxLength(25)
                    .HasColumnName("curp");

                entity.Property(e => e.Fechaenvio).HasColumnName("fechaenvio");

                entity.Property(e => e.Folio)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("folio");

                entity.Property(e => e.Listanegra)
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnName("listanegra");

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("nombre");

                entity.Property(e => e.Nombreagencia)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("nombreagencia");
            });

            modelBuilder.Entity<Tmpreportebitacora>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tmpreportebitacora");

                entity.Property(e => e.Agencianombre)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("agencianombre");

                entity.Property(e => e.Clienteimagen).HasColumnName("clienteimagen");

                entity.Property(e => e.Clientenombre)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("clientenombre");

                entity.Property(e => e.Contrato)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("contrato");

                entity.Property(e => e.Eliminacionfecha).HasColumnName("eliminacionfecha");

                entity.Property(e => e.Eliminacionmotivo)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("eliminacionmotivo");

                entity.Property(e => e.Eliminacionusuario)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("eliminacionusuario");

                entity.Property(e => e.Folio)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("folio");

                entity.Property(e => e.Tieneafis)
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnName("tieneafis");
            });

            modelBuilder.Entity<Tmpreportedetalleenvio>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tmpreportedetalleenvio");

                entity.Property(e => e.Agencianombre)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("agencianombre");

                entity.Property(e => e.Clientenombre)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("clientenombre");

                entity.Property(e => e.Contrato)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("contrato");

                entity.Property(e => e.Fechacaptura).HasColumnName("fechacaptura");

                entity.Property(e => e.Fechaenvio).HasColumnName("fechaenvio");

                entity.Property(e => e.Inerespuesta)
                    .HasMaxLength(500)
                    .HasColumnName("inerespuesta");

                entity.Property(e => e.Revisionanalista)
                    .HasMaxLength(50)
                    .HasColumnName("revisionanalista");

                entity.Property(e => e.Tipoalerta)
                    .HasMaxLength(20)
                    .HasColumnName("tipoalerta");

                entity.Property(e => e.Tipodocumento)
                    .HasMaxLength(50)
                    .HasColumnName("tipodocumento");
            });

            modelBuilder.Entity<Tmpreportelistanegra>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tmpreportelistanegra");

                entity.Property(e => e.Agencianombre)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("agencianombre");

                entity.Property(e => e.Clienteimagen).HasColumnName("clienteimagen");

                entity.Property(e => e.Clientenombre)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("clientenombre");

                entity.Property(e => e.Contrato)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("contrato");

                entity.Property(e => e.Fechaingreso).HasColumnName("fechaingreso");

                entity.Property(e => e.Folio)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("folio");

                entity.Property(e => e.Motivo)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("motivo");
            });

            modelBuilder.Entity<Tmpreportesemaforo>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tmpreportesemaforos");

                entity.Property(e => e.Agencia)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("agencia");

                entity.Property(e => e.Contrato)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("contrato");

                entity.Property(e => e.FechaCaptura).HasColumnName("fecha_captura");

                entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");

                entity.Property(e => e.Fechacaptura1)
                    .HasColumnType("date")
                    .HasColumnName("fechacaptura");

                entity.Property(e => e.Fechaenvio1)
                    .HasColumnType("date")
                    .HasColumnName("fechaenvio");

                entity.Property(e => e.Folio)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("folio");

                entity.Property(e => e.Foto)
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnName("foto");

                entity.Property(e => e.Horacaptura)
                    .HasColumnType("time without time zone")
                    .HasColumnName("horacaptura");

                entity.Property(e => e.Horaenvio)
                    .HasColumnType("time without time zone")
                    .HasColumnName("horaenvio");

                entity.Property(e => e.Huellas)
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnName("huellas");

                entity.Property(e => e.Semaforoassure)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforoassure");

                entity.Property(e => e.Semaforodecomparacionfacial)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforodecomparacionfacial");

                entity.Property(e => e.Semaforodeconsultaweb)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforodeconsultaweb");

                entity.Property(e => e.Semaforodedocumento)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforodedocumento");

                entity.Property(e => e.Semaforodefolioynombre)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforodefolioynombre");

                entity.Property(e => e.Semaforodehuellas)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforodehuellas");

                entity.Property(e => e.Semaforodelistanegra)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforodelistanegra");

                entity.Property(e => e.Semaforogeneral)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforogeneral");

                entity.Property(e => e.Semaforolatin)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("semaforolatin");

                entity.Property(e => e.Tipodeidentificacion)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("tipodeidentificacion");
            });

            modelBuilder.Entity<Tmpreportesemaforofacialdetalle>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tmpreportesemaforofacialdetalle");

                entity.Property(e => e.Agencianombre)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("agencianombre");

                entity.Property(e => e.Clientenombre)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("clientenombre");

                entity.Property(e => e.Contrato)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("contrato");

                entity.Property(e => e.Fechacaptura).HasColumnName("fechacaptura");

                entity.Property(e => e.Fechaenvio).HasColumnName("fechaenvio");

                entity.Property(e => e.Folio)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("folio");

                entity.Property(e => e.Scorefacial).HasColumnName("scorefacial");

                entity.Property(e => e.Tipoalerta)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("tipoalerta");

                entity.Property(e => e.Usuarioenrolador)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("usuarioenrolador");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");

                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

                entity.Property(e => e.ActivarCuenta).HasColumnName("activar_cuenta");

                entity.Property(e => e.Activo).HasColumnName("activo");

                entity.Property(e => e.AgenciaId).HasColumnName("agencia_id");

                entity.Property(e => e.ApellidoMaterno)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("apellido_materno");

                entity.Property(e => e.ApellidoPaterno)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("apellido_paterno");

                entity.Property(e => e.CorreoElectronico)
                    .HasMaxLength(50)
                    .HasColumnName("correo_electronico");

                entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");

                entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");

                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro");

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("nombre");

                entity.Property(e => e.NombreUsuario)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("nombre_usuario");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("password");

                entity.Property(e => e.PerfilId).HasColumnName("perfil_id");

                entity.Property(e => e.Token)
                    .HasMaxLength(500)
                    .HasColumnName("token");

                entity.Property(e => e.TokenVigencia).HasColumnName("token_vigencia");

                entity.HasOne(d => d.Agencia)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.AgenciaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("usuarios_agencia_id_fkey");

                entity.HasOne(d => d.Perfil)
                    .WithMany(p => p.Usuarios)
                    .HasForeignKey(d => d.PerfilId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("usuarios_perfil_id_fkey");
            });

            modelBuilder.Entity<VSolicitante>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("v_solicitantes");

                entity.Property(e => e.Apellidomaterno)
                    .HasMaxLength(50)
                    .HasColumnName("apellidomaterno");

                entity.Property(e => e.Apellidopaterno)
                    .HasMaxLength(50)
                    .HasColumnName("apellidopaterno");

                entity.Property(e => e.Correoelectronico)
                    .HasMaxLength(250)
                    .HasColumnName("correoelectronico");

                entity.Property(e => e.Estadosolicitud)
                    .HasMaxLength(30)
                    .HasColumnName("estadosolicitud");

                entity.Property(e => e.Fechaenvio).HasColumnName("fechaenvio");

                entity.Property(e => e.Folio)
                    .HasMaxLength(30)
                    .HasColumnName("folio");

                entity.Property(e => e.Listanegra).HasColumnName("listanegra");

                entity.Property(e => e.Nombre)
                    .HasMaxLength(50)
                    .HasColumnName("nombre");

                entity.Property(e => e.Nombrecompleto).HasColumnName("nombrecompleto");

                entity.Property(e => e.Solicitanteid).HasColumnName("solicitanteid");

                entity.Property(e => e.Tipocliente)
                    .HasMaxLength(50)
                    .HasColumnName("tipocliente");
            });



            modelBuilder.Entity<Validacion>(entity =>
            {
                entity.HasKey(e => e.ValidacionId)
                    .HasName("validaciones_pkey");

                entity.ToTable("validaciones");

                entity.Property(e => e.ValidacionId).HasColumnName("validacion_id");

                entity.Property(e => e.FechaComprobanteDomicilio).HasColumnName("fecha_comprobante_domicilio");

                entity.Property(e => e.FechaComprobanteIngresos).HasColumnName("fecha_comprobante_ingresos");

                entity.Property(e => e.FechaComprobanteBancario).HasColumnName("fecha_comprobante_bancario");

                entity.Property(e => e.FechaConsulta).HasColumnName("fecha_consulta");

                entity.Property(e => e.FechaCorreo).HasColumnName("fecha_correo");

                entity.Property(e => e.FechaCurp).HasColumnName("fecha_curp");

                entity.Property(e => e.FechaFacial).HasColumnName("fecha_facial");

                entity.Property(e => e.FechaIbms).HasColumnName("fecha_ibms");

                entity.Property(e => e.FechaIdentificacion).HasColumnName("fecha_identificacion");

                entity.Property(e => e.FechaIne).HasColumnName("fecha_ine");

                entity.Property(e => e.FechaListaAml).HasColumnName("fecha_lista_aml");

                entity.Property(e => e.FechaListaNegra).HasColumnName("fecha_lista_negra");

                entity.Property(e => e.FechaTelefono).HasColumnName("fecha_telefono");

                entity.Property(e => e.FechaAfis).HasColumnName("fecha_afis");

                entity.Property(e => e.FechaGeoreferencia).HasColumnName("fecha_georeferencia");

                entity.Property(e => e.ResultadoComprobanteDomicilio)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_comprobante_domicilio");

                entity.Property(e => e.ResultadoComprobanteIngresos)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_comprobante_ingresos");

                entity.Property(e => e.ResultadoComprobanteBancario)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_comprobante_bancario");

                entity.Property(e => e.ResultadoCorreo)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_correo");

                entity.Property(e => e.ResultadoCurp)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_curp");

                entity.Property(e => e.ResultadoFacial)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_facial");

                entity.Property(e => e.ResultadoIbms)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_ibms");

                entity.Property(e => e.ResultadoIdentificacion)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_identificacion");

                entity.Property(e => e.ResultadoIne)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_ine");

                entity.Property(e => e.ResultadoListaAml)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_lista_aml");

                entity.Property(e => e.ResultadoListaNegra)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_lista_negra");

                entity.Property(e => e.ResultadoTelefono)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_telefono");

                entity.Property(e => e.ResultadoAfis)
                    .HasMaxLength(200)
                    .HasColumnName("resultado_afis");

                entity.Property(e => e.ResultadoGeoreferencia)
                    .HasMaxLength(30)
                    .HasColumnName("resultado_georeferencia");

                entity.Property(e => e.SemaforoComprobanteDomicilio)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_comprobante_domicilio");

                entity.Property(e => e.SemaforoComprobanteIngresos)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_comprobante_ingresos");

                entity.Property(e => e.SemaforoComprobanteBancario)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_comprobante_bancario");

                entity.Property(e => e.SemaforoCorreo)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_correo");

                entity.Property(e => e.SemaforoCurp)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_curp");

                entity.Property(e => e.SemaforoFacial)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_facial");

                entity.Property(e => e.SemaforoIbms)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_ibms");

                entity.Property(e => e.SemaforoIdentificacion)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_identificacion");

                entity.Property(e => e.SemaforoIne)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_ine");

                entity.Property(e => e.SemaforoListaAml)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_lista_aml");

                entity.Property(e => e.SemaforoListaNegra)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_lista_negra");

                entity.Property(e => e.SemaforoTelefono)
                    .HasMaxLength(30)
                    .HasColumnName("semaforo_telefono");
               
                entity.Property(e => e.SemaforoAfis)
                        .HasMaxLength(30)
                        .HasColumnName("semaforo_afis");

                entity.Property(e => e.SolicitanteId).HasColumnName("solicitante_id");

                entity.Property(e => e.UsuarioConsultaId).HasColumnName("usuario_consulta_id");

                entity.HasOne(d => d.Solicitante)
                    .WithMany(p => p.Validaciones)
                    .HasForeignKey(d => d.SolicitanteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("validaciones_solicitante_id_fkey");

                entity.HasOne(d => d.UsuarioConsulta)
                    .WithMany(p => p.Validaciones)
                    .HasForeignKey(d => d.UsuarioConsultaId)
                    .HasConstraintName("validaciones_usuario_consulta_id_fkey");
            });

            modelBuilder.HasSequence("agencias_agencia_id_seq");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
