using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistance.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class UsuarioEntityConfig : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("Usuario");
            #endregion

            #region Property configuration
            builder.Property(x => x.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Apellido)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(150);
            builder.Property(x => x.NombreUsuario)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(x => x.Rol)
                .IsRequired();
            builder.Property(x => x.Estado)
                .IsRequired();
            #endregion

            #region Indexes
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.NombreUsuario).IsUnique();
            #endregion

            #region Relationship configuration
            builder.HasMany(x => x.dirigentePoliticos)
                .WithOne(d => d.Usuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
