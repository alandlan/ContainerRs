using ContainerRS.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerRS.Api.Data.Configuration
{
    public class ClienteConfigurations : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.OwnsOne(c => c.Email, cfg =>
            {
                cfg.Property(e => e.Endereco)
                    .HasColumnName("Email")
                    .IsRequired();
            });
        }
    }
}
