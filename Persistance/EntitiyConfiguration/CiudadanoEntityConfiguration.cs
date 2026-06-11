using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistance.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class CiudadanoEntityConfiguration : IEntityTypeConfiguration<Ciudadano>
    {
        public void Configure(EntityTypeBuilder<Ciudadano> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Ciudadano");
            #endregion
            #region Property Configuration
            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Apellido)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Email).IsRequired();
            builder.Property(x => x.NumeroDocumento).IsRequired();
            builder.Property(x => x.Estado)
                .IsRequired();
            // Collection navigations are configured via relationships in related entity configurations.
            #endregion

            #region Relationship Configuration
            // Relationships are configured in the related entity configurations (Voto, CodigoVerificacion, VotacionCiudadano)
            #endregion

        }
    }
}