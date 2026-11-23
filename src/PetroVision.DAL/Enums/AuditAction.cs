namespace PetroVision.DAL.Enums;

// Stored as int: only append new values, never renumber.
public enum AuditAction
{
    AlertAcknowledged = 1,
    AlertDismissed = 2,
    AlertEscalated = 3,

    IncidentCreated = 10,
    IncidentAcknowledged = 11,
    IncidentClosed = 12,
    IncidentReassigned = 13,

    CameraCreated = 20,
    CameraUpdated = 21,
    CameraModelsUpdated = 22,
    CameraMuted = 23,
    CameraUnmuted = 24,
    CameraDeleted = 25,

    CameraRequestSubmitted = 30,
    CameraRequestApproved = 31,
    CameraRequestRejected = 32,

    SignupApproved = 40,
    SignupRejected = 41,
    AgentCreated = 42,
    PermissionsUpdated = 43,
    ServiceEnded = 44,
    AccountReactivated = 45,

    ProfileUpdated = 50,
    PasswordChanged = 51,
    PasswordReset = 52,

    AnnouncementPosted = 60,
    AnnouncementDeleted = 61,
    HandoverCreated = 62
}
