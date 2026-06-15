using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class CandidatoEntityConfig : IEntityTypeConfiguration<Candidato>
    {
        public void Configure(EntityTypeBuilder<Candidato> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Candidato");
            #endregion

            #region Property Configuration
           
            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Apellido)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.FotoUrl)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.PartidoPoliticoId)
                .IsRequired();
            builder.Property(x => x.Estado)
                .IsRequired();
            #endregion

            #region Relationship Configuration
            builder.HasOne(x => x.PartidoPolitico)
                .WithMany(x => x.Candidatos)
                .HasForeignKey(x => x.PartidoPoliticoId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}