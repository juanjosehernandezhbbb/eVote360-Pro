using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class CodigoVerificacionEntityConfig : IEntityTypeConfiguration<CodigoVerificacion>
    {
        public void Configure(EntityTypeBuilder<CodigoVerificacion> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("CodigoVerificacion");
            #endregion
            #region Property Configuration
            builder.Property(x=>x.CiudadanoId)
                .IsRequired();
            builder.Property(x => x.EleccionId)
                .IsRequired();
            builder.Property(x => x.Codigo)
                .IsRequired()
                .HasColumnType("char(6)");
            builder.Property(x => x.FechaGeneracion)
                .HasColumnType("datetime2")
                .IsRequired();
            builder.Property(x=>x.FechaExpiracion)
                .HasColumnType("datetime2")
                .IsRequired();
            #endregion

            #region Relationship Configuration
            builder.HasOne(x => x.Ciudadano)
                .WithMany(c => c.CodigosDeVerificacion)
                .HasForeignKey(x => x.CiudadanoId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Eleccion)
                .WithMany(e => e.CodigoVerificacion)
                .HasForeignKey(x => x.EleccionId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}