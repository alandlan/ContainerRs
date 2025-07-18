﻿using ContainerRS.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContainerRS.Api.Data.Configuration
{
    public class LocacaoConfigurations : IEntityTypeConfiguration<Locacao>
    {
        public void Configure(EntityTypeBuilder<Locacao> builder)
        {
            builder.OwnsOne(locacao => locacao.Status, status =>
            {
                status.Property(s => s.Status)
                    .HasColumnName("Status")
                    .HasConversion<string>();
            });

            builder.HasOne(l => l.Proposta)
                .WithOne()
                .HasForeignKey<Locacao>(l => l.PropostaId);
        }
    }
}
