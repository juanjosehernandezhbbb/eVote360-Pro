using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class VotoEntityConfig : IEntityTypeConfiguration<Voto>
    {
        public void Configure(EntityTypeBuilder<Voto> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Voto");
            #endregion
            #region Property Configuration
            builder.Property(x => x.CiudadanoId).IsRequired();
            builder.Property(x => x.EleccionId).IsRequired();
            builder.Property(x => x.FechaVoto).IsRequired();
            // CandidatoId is optional (nullable) so do not mark required
            #endregion
            #region relationship configuration
            builder.HasOne(x=>x.Ciudadano)
                .WithMany(c => c.VotosCollection)
                .HasForeignKey(v => v.CiudadanoId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Eleccion)
                .WithMany(e => e.Votos)
                .HasForeignKey(v => v.EleccionId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}