using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;

namespace PetroVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departments;

    public DepartmentsController(IDepartmentService departments) => _departments = departments;

    // Public: the sign-up form needs the list before the user has an account.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll() => Ok(await _departments.GetAllAsync());
}
