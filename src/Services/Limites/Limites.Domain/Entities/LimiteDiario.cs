using Limites.Domain.Enums;
using Limites.Domain.Exceptions;
using Limites.Domain.ValueObjects;

namespace Limites.Domain.Entities
{
    public class LimiteDiario
    {
        public Guid ContaId { get; private set; }
        public DateOnly Data { get; private set; }
        public Dinheiro ValorLimite { get; private set; }
        public Dinheiro ValorUtilizado { get; private set; }
        private readonly List<Reserva> _reservas = new();
        public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

        private LimiteDiario(Guid contaId, DateOnly data, Dinheiro valorLimite, Dinheiro valorUtilizado)
        {
            ArgumentNullException.ThrowIfNull(valorLimite);
            ArgumentNullException.ThrowIfNull(valorUtilizado);
            if (contaId == Guid.Empty)
                throw new ArgumentException("O ID da conta não pode ser vazio.", nameof(contaId));
            if (valorLimite == Dinheiro.Zero)
                throw new ArgumentException("O valor do limite não pode ser zero.", nameof(valorLimite));

            if (data == DateOnly.MinValue)
                throw new ArgumentException("A data não pode ser o valor padrão.", nameof(data));

            ContaId = contaId;
            Data = data;
            ValorLimite = valorLimite;
            ValorUtilizado = valorUtilizado;
        }

        public static LimiteDiario Criar(Guid contaId, DateOnly data, Dinheiro valorLimite) => new LimiteDiario(contaId, data, valorLimite, Dinheiro.Zero);

        // ValorUtilizado soma reservas Reservadas + Confirmadas. Só Liberar devolve limite.
        public Dinheiro ValorDisponivel => ValorLimite - ValorUtilizado;

        /// <summary>
        /// Idempotente por transferenciaId: repetir o comando devolve a reserva existente sem consumir limite de novo.
        /// </summary>
        public Reserva Reservar(Guid transferenciaId, Dinheiro valor, DateTime criadaEm)
        {
            ArgumentNullException.ThrowIfNull(valor);

            var existente = BuscarReserva(transferenciaId);
            if (existente is not null)
            {
                // Mesmo ID com valor diferente não é reenvio: é bug ou adulteração.
                if (existente.Valor != valor)
                    throw new DomainException(
                        $"A transferência {transferenciaId} já possui reserva de {existente.Valor.Valor}, diferente de {valor.Valor}.");
                return existente;
            }

            if (valor > ValorDisponivel)
                throw new DomainException("Limite diário insuficiente.");

            var reserva = new Reserva(transferenciaId, valor, criadaEm);
            _reservas.Add(reserva);
            ValorUtilizado += valor;
            return reserva;
        }

        public void Confirmar(Guid transferenciaId)
        {
            var reserva = ObterReserva(transferenciaId);
            if (reserva.Status == StatusReserva.Confirmada)
                return;

            reserva.Confirmar();
        }

        public void Liberar(Guid transferenciaId)
        {
            var reserva = ObterReserva(transferenciaId);
            if (reserva.Status == StatusReserva.Liberada)
                return;

            reserva.Liberar();
            ValorUtilizado -= reserva.Valor;
        }

        private Reserva? BuscarReserva(Guid transferenciaId) =>
            _reservas.FirstOrDefault(r => r.TransferenciaId == transferenciaId);

        private Reserva ObterReserva(Guid transferenciaId) =>
            BuscarReserva(transferenciaId)
            ?? throw new DomainException($"Reserva da transferência {transferenciaId} não encontrada.");
    }
}
