using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistance.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class EleccionEntityConfig : IEntityTypeConfiguration<Eleccion>
    {
        public void Configure(EntityTypeBuilder<Eleccion> builder)
        {
            #region Basic Configuration
            builder.HasKey(x=>x.Id)
                .HasName("Eleccion");
            #endregion

            #region Property Configuration
            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Fecha)
                .IsRequired();
            #endregion

            #region Relationships Configuration
            // Collection navigations (Votos, CodigoVerificacion, VotacionesCiudadanos) are configured
            // from the dependent side (Voto, CodigoVerificacion, VotacionCiudadano entity configurations).
            #endregion

        }


    }
}
