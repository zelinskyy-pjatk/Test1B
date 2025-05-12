using Microsoft.AspNetCore.Mvc;
using Test1B.DTOs;
using Test1B.Exceptions;
using Test1B.Model;
using Test1B.Services;

namespace Test1B.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class VisitsController : ControllerBase
{
    private readonly IVisitsService _visitsService;
    public VisitsController(IVisitsService visitsService) => _visitsService = visitsService;
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetVisitById(int id)
    {
        try
        {
            var result = _visitsService.GetVisitByIdAsync(id);
            return Ok(result);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateVisit([FromBody] VisitRequestDTO requestDto)
    {
        try
        {
            var result = await _visitsService.CreateNewVisitAsync(requestDto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ConflictException e)
        {
            return StatusCode(StatusCodes.Status409Conflict, e.Message);
        }
        catch (NotFoundException e)
        {
            return StatusCode(StatusCodes.Status404NotFound, e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e);
        }
    }
}