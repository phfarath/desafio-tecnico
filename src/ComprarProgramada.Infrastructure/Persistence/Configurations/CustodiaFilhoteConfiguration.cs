using ComprarProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class CustodiaFilhoteConfiguration : IEntityTypeConfiguration<CustodiaFilhote>
{
    public void Configure(EntityTypeBuilder<CustodiaFilhote> builder)
    {
        builder.ToTable("CustodiasFilhote");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.ContaFilhoteId).IsRequired();

        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.CustodiaFilhoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backing field para a collection privada
        builder.Navigation(c => c.Itens).HasField("_itens");
    }
}
