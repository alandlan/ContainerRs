using System.Text.RegularExpressions;

namespace ContainerRS.Api.Domain
{
    public class Email
    {
        private static readonly Regex EmaiLRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Email(string endereco)
        {
            if (string.IsNullOrWhiteSpace(endereco))
                throw new ArgumentException("O endereço de e-mail não pode ser vazio.", nameof(endereco));
            if (!EmaiLRegex.IsMatch(endereco))
                throw new ArgumentException("Endereço de e-mail inválido.", nameof(endereco));
            Endereco = endereco;
        }

        public string Endereco { get; private set; }
    }
}
