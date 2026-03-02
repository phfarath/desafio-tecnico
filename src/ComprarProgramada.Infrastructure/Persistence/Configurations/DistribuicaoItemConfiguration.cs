using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class DistribuicaoItemConfiguration : IEntityTypeConfiguration<DistribuicaoItem>
{
    public void Configure(EntityTypeBuilder<DistribuicaoItem> builder)
    {
        builder.ToTable("DistribuicaoItens");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedOnAdd();

        builder.Property(d => d.DistribuicaoId).IsRequired();

        builder.Property(d => d.Ticker)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion(
                t => t.Valor,
                v => Ticker.Criar(v));

        builder.Property(d => d.Quantidade).IsRequired();

        builder.Property(d => d.PrecoUnitario)
            .IsRequired()
            .HasColumnType("decimal(18,6)");

        builder.Property(d => d.ValorOperacao)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.ValorIrDedoDuro)
            .IsRequired()
            .HasColumnType("decimal(18,6)");
    }
}
