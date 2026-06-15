using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class CandidatoPuestoEntityConfig : IEntityTypeConfiguration<CandidatoPuesto>
    {
        public void Configure(EntityTypeBuilder<CandidatoPuesto> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("CandidatoPuesto");
            #endregion

            #region Property Configuration
            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.CandidatoId)
                .IsRequired();
            builder.Property(x => x.PuestoElectivoId)
                .IsRequired();
            builder.Property(x => x.PartidoPoliticoId)
                .IsRequired();
            builder.Property(x=>x.Estado)
                .IsRequired();
            #endregion

            #region Relationship Configuration
            builder.HasOne(x=>x.Candidato)
                .WithMany(c=>c.CandidatoPuestos)
                .HasForeignKey(x=>x.CandidatoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PuestoElectivo)
                .WithMany(p => p.CandidatoPuestos)
                .HasForeignKey(x => x.PuestoElectivoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PartidoPolitico)
                .WithMany(p=>p.CandidatoPuestos)
                .HasForeignKey(x => x.PartidoPoliticoId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
