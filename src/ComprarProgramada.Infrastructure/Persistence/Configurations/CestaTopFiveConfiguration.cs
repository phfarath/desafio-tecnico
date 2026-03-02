using ComprarProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class CestaTopFiveConfiguration : IEntityTypeConfiguration<CestaTopFive>
{
    public void Configure(EntityTypeBuilder<CestaTopFive> builder)
    {
        builder.ToTable("CestasTopFive");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Ativa).IsRequired();
        builder.Property(c => c.DataCriacao).IsRequired();
        builder.Property(c => c.DataDesativacao);

        // Índice parcial: apenas uma cesta pode estar ativa
        builder.HasIndex(c => c.Ativa);

        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.CestaTopFiveId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Itens).HasField("_itens");
    }
}
