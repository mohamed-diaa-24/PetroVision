using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.BLL.Mapping;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class CameraRequestService : ICameraRequestService
{
    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IAuditLogger _audit;
    private readonly IEventNotifier _events;
    private readonly ILogger<CameraRequestService> _logger;

    public CameraRequestService(PetroVisionDbContext db, ICurrentUser user, IAuditLogger audit,
        IEventNotifier events, ILogger<CameraRequestService> logger)
    {
        _db = db;
        _user = user;
        _audit = audit;
        _events = events;
        _logger = logger;
    }

    private int DepartmentId => _user.DepartmentId
        ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

    private IQueryable<CameraRequest> WithDetails() => _db.CameraRequests
        .Include(r => r.Camera).ThenInclude(c => c!.ModelConfigs)
        .Include(r => r.RequestedBy)
        .Include(r => r.ReviewedBy);

    // ---------- Agent ----------
    public async Task<CameraRequestDto> CreateAsync(CreateCameraRequestDto dto)
    {
        var departmentId = DepartmentId;
        var request = new CameraRequest
        {
            Type = dto.Type,
            DepartmentId = departmentId,
            RequestedById = _user.UserId,
            Reason = Clean(dto.Reason)
        };

        if (dto.Type == CameraRequestType.Add)
        {
            var name = Clean(dto.Name) ?? throw new BadRequestException("Camera name is required.");
            var location = Clean(dto.Location) ?? throw new BadRequestException("Location is required.");

            if (await NameTakenAsync(departmentId, name, exceptCameraId: null))
                throw new ConflictException("A camera with this name already exists in your department.");

            request.Name = name;
            request.Location = location;
            request.StreamUrl = Clean(dto.StreamUrl) ?? string.Empty;
            request.Models = FormatModels(dto.Models ?? new());
        }
        else
        {
            if (dto.CameraId is null)
                throw new BadRequestException("CameraId is required for edit and remove requests.");

            var camera = await _db.Cameras
                .Include(c => c.ModelConfigs)
                .FirstOrDefaultAsync(c => c.Id == dto.CameraId && c.DepartmentId == departmentId && !c.IsDeleted)
                ?? throw new NotFoundException("Camera not found.");

            if (await _db.CameraRequests.AnyAsync(r => r.CameraId == camera.Id && r.Status == RequestStatus.Pending))
                throw new ConflictException("There is already a pending request for this camera.");

            request.CameraId = camera.Id;

            if (dto.Type == CameraRequestType.Remove)
            {
                request.Name = camera.Name;
                request.Location = camera.Location;
                request.StreamUrl = camera.StreamUrl;
                request.Models = FormatModels(camera.ModelConfigs.Select(m => m.Type));
            }
            else
            {
                request.Name = Clean(dto.Name) ?? camera.Name;
                request.Location = Clean(dto.Location) ?? camera.Location;
                request.StreamUrl = Clean(dto.StreamUrl) ?? camera.StreamUrl;
                request.Models = dto.Models is null ? null : FormatModels(dto.Models);

                if (request.Name != camera.Name && await NameTakenAsync(departmentId, request.Name, camera.Id))
                    throw new ConflictException("A camera with this name already exists in your department.");
            }
        }

        _db.CameraRequests.Add(request);
        await _db.SaveChangesAsync();

        _audit.Add(AuditAction.CameraRequestSubmitted, nameof(CameraRequest), request.Id,
            $"{request.Type} camera · {request.Name}", request.Reason);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(request.Id);
    }

    public async Task<List<CameraRequestDto>> GetMineAsync()
    {
        var requests = await WithDetails().AsNoTracking()
            .Where(r => r.RequestedById == _user.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(ToDto).ToList();
    }

    // ---------- Manager ----------
    public async Task<List<CameraRequestDto>> GetDepartmentAsync(RequestStatus? status)
    {
        var departmentId = DepartmentId;
        var query = WithDetails().AsNoTracking().Where(r => r.DepartmentId == departmentId);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return requests.Select(ToDto).ToList();
    }

    // Manager: any request of the department. Agent: only their own.
    public async Task<CameraRequestDto> GetByIdAsync(int id)
    {
        var departmentId = DepartmentId;
        var request = await WithDetails().AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.DepartmentId == departmentId
                                      && (_user.Role == "Manager" || r.RequestedById == _user.UserId))
            ?? throw new NotFoundException("Camera request not found.");

        return ToDto(request);
    }

    public async Task<CameraRequestDto> ApproveAsync(int id)
    {
        var request = await LoadPendingAsync(id);
        string? detail = null;
        Camera changedCamera = null!;
        string cameraAction;

        switch (request.Type)
        {
            case CameraRequestType.Add:
            {
                if (await NameTakenAsync(request.DepartmentId, request.Name, exceptCameraId: null))
                    throw new ConflictException("A camera with this name was added meanwhile. Reject this request or rename the camera.");

                var camera = new Camera
                {
                    Name = request.Name,
                    Location = request.Location,
                    StreamUrl = request.StreamUrl,
                    DepartmentId = request.DepartmentId,
                    Health = CameraHealth.Offline,
                    ModelConfigs = ParseModels(request.Models).Select(t => new CameraModelConfig { Type = t }).ToList()
                };
                _db.Cameras.Add(camera);
                await _db.SaveChangesAsync();

                request.CameraId = camera.Id;
                _audit.Add(AuditAction.CameraCreated, nameof(Camera), camera.Id, camera.Name, $"From request #{request.Id}");
                (changedCamera, cameraAction) = (camera, CameraChangeActions.Created);
                break;
            }
            case CameraRequestType.Edit:
            {
                var camera = ActiveCamera(request);

                if (request.Name != camera.Name && await NameTakenAsync(camera.DepartmentId, request.Name, camera.Id))
                    throw new ConflictException("Another camera already uses this name.");

                camera.Name = request.Name;
                camera.Location = request.Location;
                camera.StreamUrl = request.StreamUrl;

                if (request.Models is not null)
                {
                    var wanted = ParseModels(request.Models);
                    _db.CameraModelConfigs.RemoveRange(camera.ModelConfigs.Where(m => !wanted.Contains(m.Type)).ToList());
                    foreach (var type in wanted.Where(t => camera.ModelConfigs.All(m => m.Type != t)))
                        camera.ModelConfigs.Add(new CameraModelConfig { Type = type });
                }

                _audit.Add(AuditAction.CameraUpdated, nameof(Camera), camera.Id, camera.Name, $"From request #{request.Id}");
                (changedCamera, cameraAction) = (camera, CameraChangeActions.Updated);
                break;
            }
            case CameraRequestType.Remove:
            {
                var camera = ActiveCamera(request);
                camera.IsDeleted = true;
                camera.DeletedAt = DateTime.UtcNow;
                camera.Health = CameraHealth.Offline;
                detail = camera.Name;

                _audit.Add(AuditAction.CameraDeleted, nameof(Camera), camera.Id, camera.Name, $"From request #{request.Id}");
                (changedCamera, cameraAction) = (camera, CameraChangeActions.Deleted);
                break;
            }
            default:
                throw new BadRequestException("Unknown request type.");
        }

        MarkReviewed(request, RequestStatus.Approved, note: null);
        _audit.Add(AuditAction.CameraRequestApproved, nameof(CameraRequest), request.Id,
            $"{request.Type} camera · {detail ?? request.Name}", $"Requested by {request.RequestedBy.FullName}");
        await _db.SaveChangesAsync();

        await _events.TryNotifyAsync(_logger, "CamerasChanged", n => n.CamerasChangedAsync(
            changedCamera.DepartmentId, cameraAction, changedCamera.Id,
            cameraAction == CameraChangeActions.Deleted ? null : changedCamera.ToDto()));

        return await GetByIdAsync(id);
    }

    public async Task<CameraRequestDto> RejectAsync(int id, string reason)
    {
        var note = Clean(reason) ?? throw new BadRequestException("A rejection reason is required.");
        var request = await LoadPendingAsync(id);

        MarkReviewed(request, RequestStatus.Rejected, note);
        _audit.Add(AuditAction.CameraRequestRejected, nameof(CameraRequest), request.Id,
            $"{request.Type} camera · {request.Name}", note);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    // ---------- helpers ----------
    private async Task<CameraRequest> LoadPendingAsync(int id)
    {
        var departmentId = DepartmentId;
        var request = await WithDetails().FirstOrDefaultAsync(r => r.Id == id && r.DepartmentId == departmentId)
            ?? throw new NotFoundException("Camera request not found.");

        if (request.Status != RequestStatus.Pending)
            throw new ConflictException("This request was already reviewed.");

        return request;
    }

    private static Camera ActiveCamera(CameraRequest request)
        => request.Camera is { IsDeleted: false } camera
            ? camera
            : throw new ConflictException("The camera in this request no longer exists.");

    private void MarkReviewed(CameraRequest request, RequestStatus status, string? note)
    {
        request.Status = status;
        request.ReviewNote = note;
        request.ReviewedById = _user.UserId;
        request.ReviewedAt = DateTime.UtcNow;
    }

    private Task<bool> NameTakenAsync(int departmentId, string name, int? exceptCameraId)
        => _db.Cameras.AnyAsync(c => c.DepartmentId == departmentId && !c.IsDeleted && c.Name == name
                                     && (exceptCameraId == null || c.Id != exceptCameraId));

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string FormatModels(IEnumerable<DetectionType> models)
        => string.Join(",", models.Distinct().OrderBy(m => m));

    private static List<DetectionType> ParseModels(string? models)
        => string.IsNullOrEmpty(models)
            ? new()
            : models.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(m => Enum.TryParse<DetectionType>(m, out var t) ? t : (DetectionType?)null)
                .Where(t => t.HasValue).Select(t => t!.Value)
                .ToList();

    private static CameraValuesDto Values(Camera c) => new()
    {
        Name = c.Name,
        Location = c.Location,
        StreamUrl = c.StreamUrl,
        Models = c.ModelConfigs.Select(m => m.Type).OrderBy(t => t).ToList()
    };

    private static CameraRequestDto ToDto(CameraRequest r)
    {
        // "Before" only makes sense while the request is pending; after approval the camera
        // already holds the new values.
        var before = r.Type != CameraRequestType.Add && r.Camera is not null ? Values(r.Camera) : null;

        return new CameraRequestDto
        {
            Id = r.Id,
            Type = r.Type,
            Status = r.Status,
            CameraId = r.CameraId,
            Before = r.Status == RequestStatus.Pending ? before : null,
            After = new CameraValuesDto
            {
                Name = r.Name,
                Location = r.Location,
                StreamUrl = r.StreamUrl,
                Models = r.Models is null ? before?.Models ?? new() : ParseModels(r.Models)
            },
            Reason = r.Reason,
            RequestedById = r.RequestedById,
            RequestedBy = r.RequestedBy.FullName,
            CreatedAt = r.CreatedAt,
            ReviewedBy = r.ReviewedBy?.FullName,
            ReviewedAt = r.ReviewedAt,
            ReviewNote = r.ReviewNote
        };
    }
}
