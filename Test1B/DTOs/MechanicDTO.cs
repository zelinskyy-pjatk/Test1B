using System.ComponentModel.DataAnnotations;

namespace Test1B.DTOs;

public class MechanicDTO
{
    [Required]
    public int MechanicID { get; set; }
    [Required]
    public string LicenseNumber { get; set; }
}