using Limites.Domain.Enums;
using Limites.Domain.Exceptions;
using Limites.Domain.ValueObjects;
namespace Limites.Domain.Entities
{
    public class Reserva
    {
        public Guid TransferenciaId { get; private set; }
        public Dinheiro Valor { get; private set; }
        public StatusReserva Status { get; private set; }
        public DateTime CriadaEm { get; private set; }

        internal Reserva(Guid transferenciaId, Dinheiro valor, DateTime criadaEm)
        {
            ArgumentNullException.ThrowIfNull(valor);

            if (transferenciaId == Guid.Empty)
                throw new ArgumentException("O ID da transferência não pode ser vazio.", nameof(transferenciaId));
            if(valor == Dinheiro.Zero)
                throw new ArgumentException("Dinheiro não pode ser zero", nameof(valor));
            if(criadaEm == DateTime.MinValue)
                throw new ArgumentException("A data de criação não pode ser o valor padrão.", nameof(criadaEm));         

            TransferenciaId = transferenciaId;
            Valor = valor;
            Status = StatusReserva.Reservada;
            CriadaEm = criadaEm;
        }

        internal void Confirmar()
        {
            if (Status != StatusReserva.Reservada)
                throw new DomainException("A reserva só pode ser confirmada se estiver no status Reservada.");
            Status = StatusReserva.Confirmada;
        }

        internal void Liberar()
        {
            if (Status != StatusReserva.Reservada)
                throw new DomainException("A reserva só pode ser liberada se estiver no status Reservada.");
            Status = StatusReserva.Liberada;
        }
    }
}
