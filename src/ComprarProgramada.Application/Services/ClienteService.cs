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
        var conta = ContaFilhote.Criar(cliente, sequencial);

        // 3. Associa conta ao cliente
        cliente.AssociarContaFilhote(conta);
        await _contasFilhote.AdicionarAsync(conta, ct);

        // 4. Cria a custódia filhote
        var custodia = CustodiaFilhote.Criar(conta);
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

    public async Task<IReadOnlyList<ClienteResumoResponse>> ListarAsync(
        bool? ativo = null,
        string? nome = null,
        CancellationToken ct = default)
    {
        var clientes = await _clientes.ListarAsync(ativo, nome, ct);

        return clientes
            .Select(c => new ClienteResumoResponse(
                c.Id,
                c.Nome,
                MascararCpf(c.Cpf.Valor),
                c.Ativo,
                c.ValorMensal,
                c.ContaFilhote?.NumeroConta ?? string.Empty,
                c.DataAdesao))
            .ToList();
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

    private static string MascararCpf(string cpf)
    {
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
            return cpf;

        return $"***.{digitos[3..6]}.{digitos[6..9]}-**";
    }
}
