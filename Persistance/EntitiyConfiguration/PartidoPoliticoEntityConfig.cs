using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistance.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class PartidoPoliticoEntityConfig : IEntityTypeConfiguration<PartidoPolitico>
    {
        public void Configure(EntityTypeBuilder<PartidoPolitico> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("PartidoPolitico");
            #endregion

            #region Property configuration
            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Descripcion)
                .IsRequired()
                .HasMaxLength(250);
            builder.Property(x => x.Siglas)
            .HasColumnType("char(6)")
            .IsFixedLength();

            builder.Property(x => x.LogoUrl)
                .IsRequired();
           
            builder.Property(x => x.Estado)
                .IsRequired();
            #endregion

            #region Relationships configuration

            builder.HasMany(x => x.CandidatoPuestos)
                    .WithOne(cp => cp.PartidoPolitico)
                    .HasForeignKey(cp => cp.PartidoPoliticoId)
                    .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Candidatos)
                .WithOne(c => c.PartidoPolitico)
                .HasForeignKey(c => c.PartidoPoliticoId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion
        }

    }
}