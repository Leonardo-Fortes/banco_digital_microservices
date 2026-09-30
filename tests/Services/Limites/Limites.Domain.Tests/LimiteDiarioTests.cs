using Limites.Domain.Entities;
using Limites.Domain.Enums;
using Limites.Domain.Exceptions;
using Limites.Domain.ValueObjects;

namespace Limites.Domain.Tests
{
    public class LimiteDiarioTests
    {
        private static readonly DateTime Agora = new(2026, 9, 30, 10, 0, 0, DateTimeKind.Utc);

        private static LimiteDiario NovoLimite(decimal limite = 1000m) =>
            LimiteDiario.Criar(Guid.NewGuid(), new DateOnly(2026, 9, 30), Dinheiro.Criar(limite));

        private static Dinheiro R(decimal valor) => Dinheiro.Criar(valor);

        [Fact]
        public void Reservar_dentro_do_limite_consome_disponivel()
        {
            var limite = NovoLimite(1000m);

            var reserva = limite.Reservar(Guid.NewGuid(), R(300m), Agora);

            Assert.Equal(StatusReserva.Reservada, reserva.Status);
            Assert.Equal(R(300m), limite.ValorUtilizado);
            Assert.Equal(R(700m), limite.ValorDisponivel);
        }

        [Fact]
        public void Reservar_exatamente_o_disponivel_e_permitido()
        {
            var limite = NovoLimite(1000m);

            limite.Reservar(Guid.NewGuid(), R(1000m), Agora);

            Assert.Equal(Dinheiro.Zero, limite.ValorDisponivel);
        }

        [Fact]
        public void Reservar_acima_do_disponivel_lanca_e_nao_altera_estado()
        {
            var limite = NovoLimite(1000m);
            limite.Reservar(Guid.NewGuid(), R(800m), Agora);

            Assert.Throws<DomainException>(() => limite.Reservar(Guid.NewGuid(), R(200.01m), Agora));
            Assert.Equal(R(800m), limite.ValorUtilizado);
            Assert.Single(limite.Reservas);
        }

        [Fact]
        public void Reservar_duplicado_com_mesmo_valor_e_idempotente()
        {
            var limite = NovoLimite(1000m);
            var transferenciaId = Guid.NewGuid();

            var primeira = limite.Reservar(transferenciaId, R(300m), Agora);
            var segunda = limite.Reservar(transferenciaId, R(300m), Agora.AddSeconds(5));

            Assert.Same(primeira, segunda);
            Assert.Single(limite.Reservas);
            Assert.Equal(R(300m), limite.ValorUtilizado);
        }

        [Fact]
        public void Reservar_duplicado_apos_liberacao_nao_reserva_de_novo()
        {
            var limite = NovoLimite(1000m);
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(transferenciaId, R(300m), Agora);
            limite.Liberar(transferenciaId);

            var reserva = limite.Reservar(transferenciaId, R(300m), Agora);

            Assert.Equal(StatusReserva.Liberada, reserva.Status);
            Assert.Equal(Dinheiro.Zero, limite.ValorUtilizado);
        }

        [Fact]
        public void Reservar_mesmo_id_com_valor_diferente_lanca()
        {
            var limite = NovoLimite(1000m);
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(transferenciaId, R(300m), Agora);

            Assert.Throws<DomainException>(() => limite.Reservar(transferenciaId, R(500m), Agora));
            Assert.Equal(R(300m), limite.ValorUtilizado);
        }

        [Fact]
        public void Confirmar_mantem_valor_utilizado()
        {
            var limite = NovoLimite(1000m);
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(transferenciaId, R(300m), Agora);

            limite.Confirmar(transferenciaId);

            Assert.Equal(StatusReserva.Confirmada, limite.Reservas.Single().Status);
            Assert.Equal(R(300m), limite.ValorUtilizado);
        }

        [Fact]
        public void Confirmar_duas_vezes_e_idempotente()
        {
            var limite = NovoLimite();
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(transferenciaId, R(300m), Agora);

            limite.Confirmar(transferenciaId);
            limite.Confirmar(transferenciaId);

            Assert.Equal(StatusReserva.Confirmada, limite.Reservas.Single().Status);
        }

        [Fact]
        public void Liberar_devolve_limite()
        {
            var limite = NovoLimite(1000m);
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(transferenciaId, R(300m), Agora);

            limite.Liberar(transferenciaId);

            Assert.Equal(StatusReserva.Liberada, limite.Reservas.Single().Status);
            Assert.Equal(R(1000m), limite.ValorDisponivel);
        }

        [Fact]
        public void Liberar_duas_vezes_nao_devolve_limite_em_dobro()
        {
            var limite = NovoLimite(1000m);
            var outra = Guid.NewGuid();
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(outra, R(200m), Agora);
            limite.Reservar(transferenciaId, R(300m), Agora);

            limite.Liberar(transferenciaId);
            limite.Liberar(transferenciaId);

            Assert.Equal(R(200m), limite.ValorUtilizado);
        }

        [Fact]
        public void Liberar_reserva_confirmada_lanca()
        {
            var limite = NovoLimite();
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(transferenciaId, R(300m), Agora);
            limite.Confirmar(transferenciaId);

            Assert.Throws<DomainException>(() => limite.Liberar(transferenciaId));
        }

        [Fact]
        public void Confirmar_reserva_liberada_lanca()
        {
            var limite = NovoLimite();
            var transferenciaId = Guid.NewGuid();
            limite.Reservar(transferenciaId, R(300m), Agora);
            limite.Liberar(transferenciaId);

            Assert.Throws<DomainException>(() => limite.Confirmar(transferenciaId));
        }

        [Fact]
        public void Confirmar_ou_liberar_reserva_inexistente_lanca()
        {
            var limite = NovoLimite();

            Assert.Throws<DomainException>(() => limite.Confirmar(Guid.NewGuid()));
            Assert.Throws<DomainException>(() => limite.Liberar(Guid.NewGuid()));
        }
    }
}
