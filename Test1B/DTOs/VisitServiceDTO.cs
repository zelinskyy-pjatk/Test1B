using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Test1B.DTOs;

public class VisitServiceDTO
{
    [Required]
    public string Name { get; set; }
    [Required]
    public decimal ServiceFee { get; set; }
}