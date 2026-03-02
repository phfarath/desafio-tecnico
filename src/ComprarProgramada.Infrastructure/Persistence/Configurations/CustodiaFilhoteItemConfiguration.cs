using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class CustodiaFilhoteItemConfiguration : IEntityTypeConfiguration<CustodiaFilhoteItem>
{
    public void Configure(EntityTypeBuilder<CustodiaFilhoteItem> builder)
    {
        builder.ToTable("CustodiasFilhoteItens");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.CustodiaFilhoteId).IsRequired();

        builder.Property(c => c.Ticker)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion(
                t => t.Valor,
                v => Ticker.Criar(v));

        builder.HasIndex(c => new { c.CustodiaFilhoteId, c.Ticker }).IsUnique();

        builder.Property(c => c.Quantidade).IsRequired();

        builder.Property(c => c.PrecoMedio)
            .IsRequired()
            .HasColumnType("decimal(18,6)");
    }
}
