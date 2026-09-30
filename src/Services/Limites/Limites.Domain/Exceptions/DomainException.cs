namespace Limites.Domain.Exceptions
{
    /// <summary>
    /// Violação de regra de negócio. A API traduz para 422; qualquer outra exceção vira 500.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
