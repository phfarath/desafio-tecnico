using ComprarProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class DistribuicaoConfiguration : IEntityTypeConfiguration<Distribuicao>
{
    public void Configure(EntityTypeBuilder<Distribuicao> builder)
    {
        builder.ToTable("Distribuicoes");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedOnAdd();

        builder.Property(d => d.OrdemCompraId).IsRequired();
        builder.Property(d => d.ClienteId).IsRequired();
        builder.Property(d => d.ContaFilhoteId).IsRequired();
        builder.Property(d => d.DataDistribuicao).IsRequired();

        builder.HasIndex(d => new { d.OrdemCompraId, d.ClienteId }).IsUnique();

        builder.Property(d => d.ValorAporte)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.ValorTotalIrDedoDuro)
            .IsRequired()
            .HasColumnType("decimal(18,6)");

        builder.Property(d => d.CriadoEm).IsRequired();

        builder.HasOne(d => d.OrdemCompra)
            .WithMany()
            .HasForeignKey(d => d.OrdemCompraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Cliente)
            .WithMany()
            .HasForeignKey(d => d.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Itens)
            .WithOne()
            .HasForeignKey(i => i.DistribuicaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Itens).HasField("_itens");
    }
}
