using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using SIEG.SrDevChallenge.Domain.Entities;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;

public class SrDevChallengeContext(DbContextOptions<SrDevChallengeContext> options): DbContext(options)
{
    public virtual DbSet<DocumentoFiscal> DocumentosFiscais {get ; set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DocumentoFiscal>(bd =>
        {
            bd.ToCollection("documentosFiscais");
            bd.Property(x => x.HashXml).HasMaxLength(64).IsRequired();
            bd.HasIndex(x => x.HashXml).IsUnique();
            bd.HasIndex(x=> x.ChaveAcesso);
            bd.HasIndex(x=>x.CriadoEm);
            bd.HasIndex(x=>x.Data);            
        });
        
    }
}
