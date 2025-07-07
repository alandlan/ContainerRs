namespace ContainerRS.Api.Domain
{
    public class Cliente
    {
        private Cliente() { }
        public Cliente(string nome, Email email, string cpf)
        {
            Nome = nome;
            Email = email;
            CPF = cpf;
        }

        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public Email Email { get; private set; }
        public string CPF { get; private set; }
        public string? Celular { get; set; }

        public ICollection<Endereco> Enderecos { get; set; }

        public Endereco AddEndereco(Endereco endereco)
        {
            Enderecos ??= [];
            Enderecos.Add(endereco);
            return endereco;
        }

        public void RemoveEndereco(Endereco endereco)
        {
            Enderecos.Remove(endereco);
        }

        public Endereco AddEndereco(string cep, string rua, string? numero, string? complemento, string? bairro, string municipio, UnidadeFederativa? estado)
        {
            var endereco = new Endereco
            {
                CEP = cep,
                Rua = rua,
                Numero = numero,
                Complemento = complemento,
                Bairro = bairro,
                Municipio = municipio,
                Estado = estado
            };
            return AddEndereco(endereco);
        }
    }
}
