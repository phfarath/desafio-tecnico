using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class OrdemCompraConfiguration : IEntityTypeConfiguration<OrdemCompra>
{
    public void Configure(EntityTypeBuilder<OrdemCompra> builder)
    {
        builder.ToTable("OrdensCompra");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.ContaMasterId).IsRequired();
        builder.Property(o => o.CestaTopFiveId).IsRequired();

        builder.Property(o => o.DataCompra).IsRequired();
        builder.HasIndex(o => o.DataCompra).IsUnique();

        builder.Property(o => o.ValorTotalConsolidado)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.CriadoEm).IsRequired();

        builder.HasOne(o => o.ContaMaster)
            .WithMany()
            .HasForeignKey(o => o.ContaMasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.CestaTopFive)
            .WithMany()
            .HasForeignKey(o => o.CestaTopFiveId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Itens)
            .WithOne()
            .HasForeignKey(i => i.OrdemCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Itens).HasField("_itens");
    }
}
