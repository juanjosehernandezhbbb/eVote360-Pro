using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;

namespace Persistance.EntitiyConfiguration
{
    public class SolicitudAlianzaEntityConfig : IEntityTypeConfiguration<SolicitudAlianza>
    {
        public void Configure(EntityTypeBuilder<SolicitudAlianza> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("SolicitudAlianza");
            #endregion

            #region Property configuration
            builder.Property(x => x.PartidoSolicitanteId).IsRequired();
            builder.Property(x => x.PartidoReceptorId).IsRequired();
            builder.Property(x => x.Estado).IsRequired();
            builder.Property(x => x.FechaSolicitud)
                .HasColumnType("datetime2")
                .IsRequired();
            builder.Property(x => x.FechaRespuesta)
                .HasColumnType("datetime2");
            #endregion

            #region Relationship configuration
            builder.HasOne(x => x.PartidoSolicitante)
                .WithMany(p => p.SolicitudesEnviadas)
                .HasForeignKey(x => x.PartidoSolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PartidoReceptor)
                .WithMany(p => p.SolicitudesRecibidas)
                .HasForeignKey(x => x.PartidoReceptorId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
