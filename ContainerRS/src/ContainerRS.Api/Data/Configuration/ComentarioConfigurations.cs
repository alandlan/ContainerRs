using ContainerRS.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerRS.Api.Data.Configuration
{
    public class ComentarioConfigurations : IEntityTypeConfiguration<Comentario>
    {
        public void Configure(EntityTypeBuilder<Comentario> builder)
        {
            builder.HasOne(c => c.Proposta)
                .WithMany(p => p.Comentarios)
                .HasForeignKey(c => c.PropostaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
