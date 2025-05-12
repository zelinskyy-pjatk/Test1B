using System.ComponentModel.DataAnnotations;

namespace Test1B.DTOs;

public class VisitResponseDTO
{
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public ClientDTO ClientInfo { get; set; }
    [Required]
    public MechanicDTO MechanicInfo { get; set; }
    [Required]
    [MinLength(1)]
    public List<VisitServiceDTO> VisitServices { get; set; }
}