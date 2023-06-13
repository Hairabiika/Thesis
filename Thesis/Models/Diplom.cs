#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Thesis.Models;

[Serializable]
[Table("Diplom")]
public partial class Diplom
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("word")]
    public string Word { get; set; }

    [Required]
    [Column("image")]
    public string Image { get; set; }
}