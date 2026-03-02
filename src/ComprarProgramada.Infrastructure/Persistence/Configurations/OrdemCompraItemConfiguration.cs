using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class OrdemCompraItemConfiguration : IEntityTypeConfiguration<OrdemCompraItem>
{
    public void Configure(EntityTypeBuilder<OrdemCompraItem> builder)
    {
        builder.ToTable("OrdensCompraItens");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.OrdemCompraId).IsRequired();

        builder.Property(o => o.Ticker)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion(
                t => t.Valor,
                v => Ticker.Criar(v));

        builder.HasIndex(o => new { o.OrdemCompraId, o.Ticker }).IsUnique();

        builder.Property(o => o.QuantidadeLotePadrao).IsRequired();
        builder.Property(o => o.QuantidadeFracionario).IsRequired();

        builder.Property(o => o.PrecoUnitario)
            .IsRequired()
            .HasColumnType("decimal(18,6)");
    }
}
