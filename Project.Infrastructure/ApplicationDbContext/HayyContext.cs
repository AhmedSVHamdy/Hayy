using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Project.Infrastructure.ApplicationDbContext;

public partial class HayyContext : DbContext
{
    public HayyContext()
    {
    }

    public HayyContext(DbContextOptions<HayyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Test> Tests { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Test>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Test");

            entity.Property(e => e.Test1)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("test");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
