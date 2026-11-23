# PetroVision 360

Safety intelligence platform for petroleum facilities.
Backend: ASP.NET Core Web API (3-tier: API / BLL / DAL), EF Core + SQL Server, JWT.

## Structure

- `src/PetroVision.API` - Presentation layer (controllers)
- `src/PetroVision.BLL` - Business logic layer (services, DTOs)
- `src/PetroVision.DAL` - Data access layer (entities, DbContext)

## Run

```bash
dotnet ef database update --project src/PetroVision.DAL --startup-project src/PetroVision.API   # optional, Development migrates on start
dotnet run --project src/PetroVision.API
```

Swagger: `/swagger` (Development). Seeded accounts: manager `1001` / `Manager@123`,
agent `4710` / `Agent@123`, owner `1002` / `Owner@123` (login accepts employee ID or email).
`src/PetroVision.API/PetroVision.API.http` has a sample request for every area.

## Endpoints

| Area | Endpoints | Who |
|---|---|---|
| Auth | `POST login`, `POST signup`, `GET/PUT me`, `POST change-password`, `POST forgot-password`, `POST reset-password` | public / signed in |
| Departments | `GET /api/Departments` | public |
| Dashboard | `GET /api/Dashboard`, `GET /api/Dashboard/owner` | Staff / Owner |
| Cameras | `GET`, `GET {id}`, `POST`, `PUT {id}`, `PUT {id}/models`, `PATCH {id}/mute`, `DELETE {id}` (soft), `POST test-connection` | Staff, ManageCameras, EditModelSettings |
| Camera requests | `POST`, `GET mine`, `GET {id}`, `GET ?status=`, `POST {id}/approve`, `POST {id}/reject` | Staff / Manager |
| Alerts | `GET`, `GET {id}`, `POST {id}/acknowledge`, `POST {id}/dismiss`, `POST {id}/escalate` | Staff |
| Incidents | `GET`, `GET {id}`, `GET assignees`, `POST`, `POST {id}/acknowledge`, `POST {id}/close`, `POST {id}/reassign` | Staff (Owner read-only) |
| Sign-up requests | `GET`, `POST {userId}/approve`, `POST {userId}/reject` | Manager (agents) / Owner (managers) |
| Agents | `GET`, `GET {id}`, `POST`, `PUT {id}/permissions`, `POST permissions/senior-template`, `POST {id}/end-service`, `POST {id}/reactivate` | Manager |
| Activity | `GET mine`, `GET team` | signed in / Manager, Owner, ViewTeamLogs |
| Analytics | `GET ?days=7&departmentId=` | Manager / Owner |
| Announcements | `GET`, `POST`, `POST {id}/seen`, `PATCH {id}/pin`, `DELETE {id}` | Staff / PostAnnouncements |
| Handovers | `GET`, `POST`, `POST {id}/read` | Staff |
| Detections | `POST`, `POST heartbeat` | AI service (`X-Api-Key`) |

Notes:
- Every token carries a security stamp; changing permissions, status or password signs the user out.
- Cameras are soft-deleted so alert / incident history is never lost.
- Forgot-password emails go to the log in development (`LoggingEmailSender`); plug in SMTP for production.
