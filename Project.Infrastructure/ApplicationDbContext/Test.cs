using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Infrastructure.ApplicationDbContext;

public partial class Test
{
    [Key]
    public int Id { get; set; }
    public string? Test1 { get; set; }
}
