
namespace Limites.Domain.ValueObjects
{
    public sealed record Dinheiro
    {
        public decimal Valor { get; }
        public static Dinheiro Zero { get; } = new(0);
        private Dinheiro(decimal valor)
        {
            if (valor < 0)
                throw new ArgumentException("O valor não pode ser negativo.", nameof(valor));

            if(Math.Round(valor, 2) != valor)
                throw new ArgumentException("O valor não pode ter mais de duas casas decimais.", nameof(valor));
            Valor = valor;
        }
        public static Dinheiro Criar(decimal valor) => new Dinheiro(valor);
        public static Dinheiro operator -(Dinheiro a, Dinheiro b) => new Dinheiro(a.Valor - b.Valor);
        public static Dinheiro operator +(Dinheiro a, Dinheiro b) => new Dinheiro(a.Valor + b.Valor);
        public static bool operator <(Dinheiro a, Dinheiro b) => a.Valor < b.Valor;
        public static bool operator >(Dinheiro a, Dinheiro b) => a.Valor > b.Valor;
    }
}
