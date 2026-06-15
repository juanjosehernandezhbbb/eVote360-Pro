using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class DirigentePoliticoEntityConfig : IEntityTypeConfiguration<DirigentePolitico>
    {
        public void Configure(EntityTypeBuilder<DirigentePolitico> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("DirigentePolitico");
            #endregion
            #region Property configuration              
            builder.Property(x => x.UsuarioId)
                    .IsRequired();
            builder.Property(x=>x.PartidoPoliticoId)
                .IsRequired();
            #endregion

            #region Relationship configuration
            builder.HasOne(x => x.Usuario)
                    .WithMany(u => u.dirigentePoliticos)
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.PartidoPolitico)
                    .WithMany(p => p.dirigentePoliticos)
                    .HasForeignKey(x => x.PartidoPoliticoId)
                    .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
