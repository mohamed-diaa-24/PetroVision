# PetroVision 360 · Backend progress

Project guide for the backend team. The endpoint list lives in [README.md](README.md);
this file tracks features that need more explanation.

## Real-time events (SignalR)

Status: done. Server -> client only: clients receive events and never call hub methods.

### Hub

| | |
|---|---|
| URL | `/hubs/events` (e.g. `http://localhost:5187/hubs/events`) |
| Auth | the same JWT as the REST API. The browser client sends it as `?access_token=` (accepted only for `/hubs`) |
| No / invalid / disabled-user token | negotiate returns **401** |
| CORS | existing `Frontend` policy (any origin, no credentials). Connect with `withCredentials: false` |
| Enums | sent as strings, exactly like REST (`"PPE"`, `"Online"`, ...) |
| JSON | camelCase, same DTOs as the REST responses |

### Groups

On connect the hub puts the connection in one group, from the token claims:

| Role | Group |
|---|---|
| Manager, Agent | `dept-{department}` |
| Owner | `owners` |
| anything else (e.g. no department) | connection aborted |

### Events

| Event | Sent when | Payload | Goes to |
|---|---|---|---|
| `AlertCreated` | AI detection (PPE) creates an alert | `AlertDto` | `dept-{id}` |
| `AlertUpdated` | alert acknowledged or dismissed; escalated while still `New` | `AlertDto` | `dept-{id}` |
| `IncidentCreated` | AI detection (Fall / Fire), manual create, alert escalation | `IncidentDto` | `dept-{id}` + `owners` |
| `IncidentUpdated` | incident acknowledged, closed or reassigned | `IncidentDto` | `dept-{id}` + `owners` |
| `CameraStatusChanged` | camera health really changes (detection or heartbeat), e.g. Offline -> Online | `{ cameraId, health, lastSeenAt }` | `dept-{id}` |
| `CamerasChanged` | camera created, edited, models changed, muted/unmuted, deleted, or changed by an approved camera request | `{ action, cameraId, camera }`, action = `created` / `updated` / `deleted`, `camera` is `null` on delete | `dept-{id}` |

The Owner never receives alert or camera events (read-only, incidents only).

Rules in the code:
- Events are sent only after `SaveChangesAsync` succeeded.
- A failed push is logged and never fails the HTTP request or the detection
  (`EventNotifierExtensions.TryNotifyAsync` in the BLL, plus a try/catch in `SignalREventNotifier`).
- Repeating the same heartbeat sends nothing; only a health transition does.

### Where things live

| Layer | File |
|---|---|
| BLL contract | `src/PetroVision.BLL/Interfaces/IEventNotifier.cs` |
| Emit points | `DetectionService`, `AlertService`, `IncidentService`, `CameraService`, `CameraRequestService` |
| Hub, groups, event names | `src/PetroVision.API/Hubs/EventsHub.cs` |
| SignalR implementation | `src/PetroVision.API/Services/SignalREventNotifier.cs` |
| Wiring (`AddSignalR`, JSON enums, `access_token`, `MapHub`) | `src/PetroVision.API/Program.cs` |
| Browser client | `frontend/js/realtime.js` |
| Test page | `frontend/signalr-test.html` |

### Using it from the frontend

```html
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.7/dist/browser/signalr.min.js"
        integrity="sha384-mU1xC5yC2LldSW74Rj1Ax8wPiLw/28V5eh51uKJMlBbRVsOtUYd4xyzNsgIAJARB"
        crossorigin="anonymous"></script>
<script src="js/realtime.js"></script>
<script>
  const connection = connectRealtime({
    baseUrl: "http://localhost:5187",
    token: loginResponse.token,
    handlers: {
      AlertCreated: alert => addAlertRow(alert),          // same renderer as GET /api/Alerts
      IncidentUpdated: incident => updateIncidentRow(incident),
      CameraStatusChanged: s => setCameraHealth(s.cameraId, s.health)
    }
  });
  await connection.start();   // reconnects automatically after network drops
</script>
```

If you change the SignalR version, update the URL **and** the `integrity` hash
(`sha384` of `dist/browser/signalr.min.js` from that npm version).

### How to test

1. Run the API: `dotnet run --project src/PetroVision.API`.
2. Open `frontend/signalr-test.html` in a browser (double-click works). Keep the base URL
   `http://localhost:5187`, log in as `manager@petrovision.com` / `Manager@123` and press Connect: the state
   should become **Connected**.
3. Open the page in two more tabs as `ahmed@petrovision.com` / `Agent@123` and `owner@petrovision.com` / `Owner@123`.
4. Trigger events and watch the logs:

| Action | Expected |
|---|---|
| `POST /api/Detections` (`X-Api-Key` header) `{"cameraId":2,"type":"PPE","confidence":0.82}` | `AlertCreated` for Manager and Ahmed, not the Owner |
| `POST /api/Detections` `{"cameraId":2,"type":"Fall","confidence":0.9}` | `IncidentCreated` for all three |
| Swagger (as Manager): acknowledge / dismiss an alert | `AlertUpdated` |
| Swagger: acknowledge / close / reassign an incident | `IncidentUpdated` (Owner too) |
| Swagger: escalate a `New` alert | `IncidentCreated` + `AlertUpdated` |
| `POST /api/Detections/heartbeat` `{"cameraId":4,"isOnline":true}` | one `CameraStatusChanged`; the same call again sends nothing |
| Create / edit / mute / delete a camera | `CamerasChanged` |
| Connect with a wrong token | 401, state stays Disconnected |

Notes: cooldowns apply (a second PPE detection on the same camera within the cooldown is ignored,
and a Fall/Fire incident is not duplicated while one is open), so close the incident or use another
camera when you repeat a test. Seeded camera 4 starts Offline, so its first heartbeat is a real transition.

Department isolation: every seeded active user is in Unit 1, so it can't be seen end to end with the
seed. The API logs the group of each connection
(`Realtime connection ... (user 3, Agent) joined dept-1`) and, at Debug level, the target groups of each
event (`Realtime IncidentCreated sent to dept-1, owners`). Turn that on with
`"Logging": { "LogLevel": { "PetroVision": "Debug" } }`.

### Known limits

- The token is checked when the connection starts. A user who is disabled or loses a permission keeps
  their open connection until it drops or the page reloads, and the reconnect then fails with 401.
- The token is in the query string of the hub URL (standard for SignalR in browsers), so don't log full
  request URLs in production.
- One API instance. Running several instances needs a SignalR backplane (Redis or Azure SignalR).
