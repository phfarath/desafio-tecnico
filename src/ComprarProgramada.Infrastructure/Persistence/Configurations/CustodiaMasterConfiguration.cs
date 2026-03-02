using ComprarProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class CustodiaMasterConfiguration : IEntityTypeConfiguration<CustodiaMaster>
{
    public void Configure(EntityTypeBuilder<CustodiaMaster> builder)
    {
        builder.ToTable("CustodiasMaster");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.ContaMasterId).IsRequired();

        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.CustodiaMasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Itens).HasField("_itens");
    }
}
