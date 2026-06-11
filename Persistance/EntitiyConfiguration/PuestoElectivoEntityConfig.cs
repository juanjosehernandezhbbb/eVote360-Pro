using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistance.Entities;


namespace Persistance.EntitiyConfiguration
{
    public class PuestoElectivoEntityConfig : IEntityTypeConfiguration<PuestoElectivo>
    {
        public void Configure(EntityTypeBuilder<PuestoElectivo> builder)
        {
            #region basic configuration
            builder.HasKey(x => x.Id);
            builder.ToTable("PuestoElectivo");
            #endregion

            #region properties configuration

            #endregion
        }

    }
}