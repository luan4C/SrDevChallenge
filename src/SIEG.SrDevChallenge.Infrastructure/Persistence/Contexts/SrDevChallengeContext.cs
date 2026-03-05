using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using SIEG.SrDevChallenge.Domain.Entities;

namespace SIEG.SrDevChallenge.Infrastructure.Persistence.Contexts;

public class SrDevChallengeContext(DbContextOptions<SrDevChallengeContext> options): DbContext(options)
{
    public virtual DbSet<DocumentoFiscal> DocumentosFiscais {get ; set;}
    public virtual DbSet<DocumentoFiscaisResumoMensal> DocumentosFiscaisResumoMensal { get; set; }
    
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

        modelBuilder.Entity<DocumentoFiscaisResumoMensal>(bd =>
        {
            bd.ToCollection("documentosFiscaisResumoMensal");
            bd.HasIndex(x => new { x.Ano, x.Mes, x.TipoDocumento }).IsUnique();
            bd.HasIndex(x => x.Ano);
            bd.HasIndex(x => x.Mes);
            bd.HasIndex(x => x.TipoDocumento);
            bd.HasIndex(x => x.UltimaAtualizacao);
        });
        
    }
}
