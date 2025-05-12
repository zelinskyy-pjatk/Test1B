using System.ComponentModel.DataAnnotations;

namespace Test1B.DTOs;

public class ClientDTO
{   
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public DateTime DateOfBirth { get; set; }
}