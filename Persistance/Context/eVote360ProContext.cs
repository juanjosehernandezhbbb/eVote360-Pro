using Microsoft.EntityFrameworkCore;
using Persistance.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Persistance.Context
{
    public class eVote360ProContext : DbContext
    {
        public eVote360ProContext(DbContextOptions<eVote360ProContext> options) : base(options) { }
        public DbSet<Ciudadano> Ciudadanos { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<CandidatoPuesto> CandidatoPuestos { get; set; }
        public DbSet<CodigoVerificacion> CodigoVerificacion { get; set; }
        public DbSet<DetalleVoto> DetallesVotos { get; set; }
        public DbSet<DirigentePolitico> DirigentesPoliticos { get; set; }
        public DbSet<Eleccion> Elecciones { get; set; }
        public DbSet<PuestoElectivo> PuestosElectivos { get; set; }
        public DbSet<PartidoPolitico> PartidosPoliticos { get; set; }
        public DbSet<SolicitudAlianza> solicitudAlianzas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Voto> Votos { get; set; }
        public DbSet<VotacionCiudadano> VotacionesCiudadanos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        }
    }
}
