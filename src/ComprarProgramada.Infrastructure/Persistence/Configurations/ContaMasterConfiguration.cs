using ComprarProgramada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class ContaMasterConfiguration : IEntityTypeConfiguration<ContaMaster>
{
    public void Configure(EntityTypeBuilder<ContaMaster> builder)
    {
        builder.ToTable("ContasMaster");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.NumeroConta)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.NumeroConta).IsUnique();

        builder.Property(c => c.DataCriacao).IsRequired();

        builder.HasOne(c => c.Custodia)
            .WithOne(cm => cm.ContaMaster)
            .HasForeignKey<CustodiaMaster>(cm => cm.ContaMasterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
