using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;

namespace PetroVision.BLL.Services;

public class DepartmentService : IDepartmentService
{
    private readonly PetroVisionDbContext _db;

    public DepartmentService(PetroVisionDbContext db) => _db = db;

    public Task<List<DepartmentDto>> GetAllAsync()
        => _db.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto { Id = d.Id, Name = d.Name })
            .ToListAsync();
}
