# DamiFYP Backend — CLAUDE.md

## Project structure

```
DamiFYP.Domain        — Entities, enums. No dependencies.
DamiFYP.Application   — MediatR commands/queries, ViewModels, services, helpers.
DamiFYP.Persistence   — EF Core context, migrations.
DamiFYP.Infrastructure — External services (email, face-verification, Gemini, blood-availability).
DamiFYP.Common        — Shared utilities.
DamiFYP               — ASP.NET host: controllers, middleware, Program.cs, hub.
```

## Tech stack

- **.NET 10** · MediatR · EF Core 10 + Npgsql (PostgreSQL) · SignalR · Keycloak JWT auth
- API docs: Scalar at `/scalar/v1` (dev only)
- Docker: `compose.yaml` in repo root starts Postgres + Keycloak

## Authentication

- Keycloak issues JWTs. Bearer token validated in `Program.cs` via `AddJwtBearer()`.
- `sub` claim = Keycloak user ID (`KeyCloakId` column in DB).
- **Middleware** (`CurrentUserProfileMiddleware`) resolves the DB user on every authenticated request and stores it in `HttpContext.Items`.
- **In controllers**: `HttpContext.GetUserProfile()` returns the resolved `UserProfile` (never null for authenticated routes — it was set by middleware).
- **In handlers**: inject `ICurrentUserProfileService` and call `GetCurrentAsync()`.
- Profile is cached in-memory for 2 minutes by `keycloakId`. Invalidate with `_profileService.InvalidateAsync(keycloakId)` after any mutation.

## Business roles (enum `BusinessRole`)

```
None           = 0   (unfinished onboarding)
Admin          = 1
Donor          = 2
Seeker         = 3
DonorAndSeeker = 4
ManageAccount  = 5   (admin-like)
```

## Authorization policies (all defined in `Program.cs`, names in `AuthorizationPolicies.cs`)

| Policy | Who |
|--------|-----|
| `CanAccessConversations` | Donor, Seeker, DonorAndSeeker, ManageAccount |
| `CanManageDonationRequests` | Seeker, DonorAndSeeker, ManageAccount |
| `CanManageDonationPosts` | Donor, DonorAndSeeker |
| `CanManageBloodTypes` | Donor, Seeker, ManageAccount |
| `CanViewAvailableDonationRequests` | Donor |
| `CanViewBloodAvailabilityPredictions` | Seeker, DonorAndSeeker, ManageAccount |
| `CanUseAssistant` | Donor, Seeker, DonorAndSeeker, ManageAccount |

Use `[Authorize(Policy = AuthorizationPolicies.XYZ)]` on controller actions.

## Controller routing patterns

Two patterns in use — check the existing controller before adding a new action:

- **`[Route("/api/[action]")]`** — action method name becomes the URL segment.  
  Used by: `AuthenticationController` (`/api/CompleteOnboarding`, `/api/GetUserProfile`, etc.)

- **`[Route("api/[controller]/")]`** — controller name is the segment, actions add sub-paths.  
  Used by: `DonationPostController`, `DonationRequestController`, `ConversationController`, etc.

## MediatR command/query pattern

1. Define `Command : IRequest<ViewModel>` and `CommandHandler : IRequestHandler<Command, ViewModel>` in `DamiFYP.Application/Features/<Feature>/`.
2. Controller sends: `await _mediator.Send(command, token)`.
3. Never put business logic in controllers. Controllers only: extract current user from `HttpContext`, populate `[JsonIgnore]` fields on the command, call `_mediator.Send`.

## EF Core / DB notes

- Context: `DamiContext` in `DamiFYP.Persistence/Contexts/`.
- **Table names are inconsistent** — check the snapshot before assuming:
  - Singular: `"DamiUser"`, `"DonationRequest"`, `"Match"`, `"Message"`, `"BotMessage"`, `"VerificationAttempt"`
  - Plural: `"DonationPosts"`, `"Conversations"`, `"ConversationParticipants"`, `"BloodTypes"`
- Migrations live in `DamiFYP.Persistence/Migrations/`. Always update **both** the `.cs` migration file **and** `DamiContextModelSnapshot.cs` **and** the `.Designer.cs` file for the new migration.
- Migration naming: `YYYYMMDDHHMMSS_PascalCaseName`.
- Adding a nullable column: `migrationBuilder.AddColumn<string>("ColumnName", "TableName", nullable: true)`.
- Run migrations: `dotnet ef database update` from the `DamiFYP.Persistence` project, or apply via the host on startup if auto-migration is configured.

## SignalR

- Hub class: `DamiHub` in `DamiFYP/DamiHub.cs`, mapped at `/hubs/chat`.
- Auth: JWT passed as `?access_token=` query string (WebSocket can't set headers). Bridge configured in `Program.cs` `OnMessageReceived`.
- Personal group per user: `SignalRGroups.ForUser(userId)` — used to push notifications to a specific user.
- `ShareLocation` uses **`OthersInGroup`** — each sender only receives the other person's updates, never their own echo.
- `IHubContext<DamiHub>` is injected wherever SignalR pushes need to happen outside the hub (e.g., `MatchService.NotifyBothPartiesAsync`).

## Static files (profile pictures)

- `UseStaticFiles` in `Program.cs` serves profile pictures at `/profile-pictures/...`.
- **Always use `ContentRootPath + "/wwwroot"` — never `WebRootPath`.**  
  `WebRootPath` is `null` when the `wwwroot` folder doesn't exist at startup, so `UseStaticFiles()` with no args silently serves nothing. Using `ContentRootPath` is unconditional.
- `Program.cs` pattern:
  ```csharp
  var wwwRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
  Directory.CreateDirectory(wwwRoot);
  app.UseStaticFiles(new StaticFileOptions {
      FileProvider = new PhysicalFileProvider(wwwRoot),
      RequestPath = ""
  });
  ```
- Controller saves to: `Path.Combine(_env.ContentRootPath, "wwwroot", "profile-pictures", fileName)`.
- DB stores only the relative path: `/profile-pictures/filename.jpg`.
- Frontend prepends `VITE_API_URL` to construct the full URL.
- `IWebHostEnvironment` is injected in `AuthenticationController` to get `ContentRootPath`.

## Key files to know

| File | Purpose |
|------|---------|
| `Program.cs` | DI wiring, middleware pipeline, auth, CORS, rate limiter |
| `DamiFYP/DamiHub.cs` | SignalR hub (chat, live location, notifications) |
| `Application/Helpers/CurrentUserProfileService.cs` | Profile resolution + 2-min cache |
| `Application/Helpers/UserProfile.cs` | DTO for the cached current user |
| `Application/Features/Authentication/` | Onboarding, profile update commands |
| `Application/Features/DonationRequests/MatchService.cs` | Candidate search + match confirmation + email + SignalR notify |
| `Application/Features/Conversations/GetAllConversationsRequest.cs` | Heavy LINQ projection — touch carefully |
| `Domain/Models/DamiUser.cs` | Main user entity |
| `Persistence/Migrations/DamiContextModelSnapshot.cs` | EF snapshot — always keep in sync with migrations |

## CORS

Policy name `"ReactClient"` allows `http://localhost:5173`. To add origins, extend in `Program.cs`.

## External services

| Service | Config section | Interface |
|---------|---------------|-----------|
| Email (SMTP) | `Email` | `IEmailService` |
| Face verification | `FaceVerificationService` | `IFaceVerificationService` |
| Blood availability ML | `BloodAvailabilityService` | `IBloodAvailabilityServiceClient` |
| Gemini AI assistant | `Gemini` | `IAssistantService` |

All configured in `Config/Development/appsettings.Development.json`.
