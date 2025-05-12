using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Test1B.DTOs;

public class VisitRequestDTO
{
    [Required]
    public int VisitId { get; set; }
    [Required]
    public int ClientId { get; set; }
    [Required]
    public MechanicDTO MechanicInfo { get; set; }
    [Required]
    [MinLength(1)]
    public List<VisitServiceDTO> VisitServices { get; set; }
}