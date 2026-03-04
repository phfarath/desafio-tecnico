using ComprarProgramada.Application.DTOs.Cliente;
using ComprarProgramada.Domain.Entities;
using ComprarProgramada.Domain.Interfaces;
using ComprarProgramada.Domain.Interfaces.Repositories;
using ComprarProgramada.Domain.ValueObjects;

namespace ComprarProgramada.Application.Services;

public sealed class ClienteService : IClienteService
{
    private readonly IClienteRepository _clientes;
    private readonly IContaFilhoteRepository _contasFilhote;
    private readonly ICustodiaFilhoteRepository _custodiasFilhote;
    private readonly IUnitOfWork _uow;

    public ClienteService(
        IClienteRepository clientes,
        IContaFilhoteRepository contasFilhote,
        ICustodiaFilhoteRepository custodiasFilhote,
        IUnitOfWork uow)
    {
        _clientes = clientes;
        _contasFilhote = contasFilhote;
        _custodiasFilhote = custodiasFilhote;
        _uow = uow;
    }

    public async Task<AdesaoResponse> AderirAsync(AdesaoRequest request, CancellationToken ct = default)
    {
        if (await _clientes.ExisteCpfAsync(Cpf.Criar(request.Cpf), ct))
            throw new InvalidOperationException($"CPF {request.Cpf} já cadastrado no sistema.");

        // 1. Cria o cliente
        var cliente = Cliente.Criar(request.Nome, request.Cpf, request.Email, request.ValorMensal);
        await _clientes.AdicionarAsync(cliente, ct);

        // 2. Cria a conta filhote (numeração gerada no repositório)
        var sequencial = await _contasFilhote.ObterProximoSequencialAsync(ct);
        var conta = ContaFilhote.Criar(cliente.Id, sequencial);
        await _contasFilhote.AdicionarAsync(conta, ct);

        // 3. Associa conta ao cliente
        cliente.AssociarContaFilhote(conta);

        // 4. Cria a custódia filhote
        var custodia = CustodiaFilhote.Criar(conta.Id);
        await _custodiasFilhote.AdicionarAsync(custodia, ct);

        await _uow.CommitAsync(ct);

        return new AdesaoResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Cpf.Valor,
            cliente.Email.Valor,
            cliente.ValorMensal,
            cliente.ValorParcela,
            conta.NumeroConta,
            cliente.DataAdesao);
    }

    public async Task AlterarValorMensalAsync(
        int clienteId, AlterarValorMensalRequest request, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(clienteId, ct)
            ?? throw new KeyNotFoundException($"Cliente {clienteId} não encontrado.");

        cliente.AlterarValorMensal(request.NovoValorMensal);
        await _uow.CommitAsync(ct);
    }

    public async Task DesativarAsync(int clienteId, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObterPorIdAsync(clienteId, ct)
            ?? throw new KeyNotFoundException($"Cliente {clienteId} não encontrado.");

        cliente.Desativar();
        await _uow.CommitAsync(ct);
    }
}
