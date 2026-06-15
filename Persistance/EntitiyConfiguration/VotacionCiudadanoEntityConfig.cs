using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;

namespace Persistance.EntitiyConfiguration
{
    public class VotacionCiudadanoEntityConfig : IEntityTypeConfiguration<VotacionCiudadano>
    {
        public void Configure(EntityTypeBuilder<VotacionCiudadano> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("VotacionCiudadano");
            #endregion

            #region Property configuration
            builder.Property(x => x.CiudadanoId).IsRequired();
            builder.Property(x => x.EleccionId).IsRequired();
            builder.Property(x => x.VotoFinalizado).IsRequired();
            #endregion

            #region Indexes
            builder.HasIndex(x => new { x.CiudadanoId, x.EleccionId }).IsUnique();
            #endregion

            #region Relationship configuration
            builder.HasOne(x => x.Ciudadano)
                .WithMany(c => c.VotacionesCiudadanos)
                .HasForeignKey(x => x.CiudadanoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Eleccion)
                .WithMany(e => e.VotacionesCiudadanos)
                .HasForeignKey(x => x.EleccionId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
