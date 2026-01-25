using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Project.Core.Domain.Entities;
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

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
