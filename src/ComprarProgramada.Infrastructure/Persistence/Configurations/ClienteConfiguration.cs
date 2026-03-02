using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprarProgramada.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(200);

        // Value Object Cpf → string
        builder.Property(c => c.Cpf)
            .IsRequired()
            .HasMaxLength(11)
            .HasConversion(
                cpf => cpf.Valor,
                valor => Cpf.Criar(valor));

        builder.HasIndex(c => c.Cpf).IsUnique();

        // Value Object Email → string
        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200)
            .HasConversion(
                email => email.Valor,
                valor => Email.Criar(valor));

        builder.Property(c => c.ValorMensal)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.Ativo).IsRequired();
        builder.Property(c => c.DataAdesao).IsRequired();
        builder.Property(c => c.DataSaida);

        builder.HasOne(c => c.ContaFilhote)
            .WithOne(cf => cf.Cliente)
            .HasForeignKey<ContaFilhote>(cf => cf.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
