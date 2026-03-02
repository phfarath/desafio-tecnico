using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class CestaItemConfiguration : IEntityTypeConfiguration<CestaItem>
{
    public void Configure(EntityTypeBuilder<CestaItem> builder)
    {
        builder.ToTable("CestaItens");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.CestaTopFiveId).IsRequired();

        builder.Property(c => c.Ticker)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion(
                t => t.Valor,
                v => Ticker.Criar(v));

        builder.HasIndex(c => new { c.CestaTopFiveId, c.Ticker }).IsUnique();

        builder.Property(c => c.Percentual)
            .IsRequired()
            .HasColumnType("decimal(6,3)");
    }
}
