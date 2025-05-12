using Test1B.DTOs;

namespace Test1B.Services;

public interface IVisitsService
{
    Task<VisitResponseDTO> GetVisitByIdAsync(int id);
    Task<int> CreateNewVisitAsync(VisitRequestDTO requestDto);
}