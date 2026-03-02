using ComprarProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class ContaFilhoteConfiguration : IEntityTypeConfiguration<ContaFilhote>
{
    public void Configure(EntityTypeBuilder<ContaFilhote> builder)
    {
        builder.ToTable("ContasFilhote");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.NumeroConta)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.NumeroConta).IsUnique();

        builder.Property(c => c.ClienteId).IsRequired();
        builder.Property(c => c.DataCriacao).IsRequired();

        builder.HasOne(c => c.Custodia)
            .WithOne(cf => cf.ContaFilhote)
            .HasForeignKey<CustodiaFilhote>(cf => cf.ContaFilhoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
