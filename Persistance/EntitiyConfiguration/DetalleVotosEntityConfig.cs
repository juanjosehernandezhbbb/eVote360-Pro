using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class DetalleVotoEntityConfig : IEntityTypeConfiguration<DetalleVoto>
    {
        public void Configure(EntityTypeBuilder<DetalleVoto> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("DetalleVoto");
            #endregion

            #region Property Configuration
            // Configure FK scalar properties rather than navigation properties
            builder.Property(x => x.CandidatoId).IsRequired();
            builder.Property(x => x.PuestoElectivoId).IsRequired();
            #endregion

            #region Relationship Configuration
            builder.HasOne(x=>x.Voto)
                .WithMany(v => v.Detalles)
                .HasForeignKey(x => x.VotoId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x=>x.PuestoElectivo)
                .WithMany(p => p.Detalles)
                .HasForeignKey(x => x.PuestoElectivoId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x=>x.Candidato)
                .WithMany(c => c.Detalles)
                .HasForeignKey(x => x.CandidatoId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
