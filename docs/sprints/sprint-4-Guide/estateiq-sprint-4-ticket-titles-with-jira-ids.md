# EstateIQ — Sprint 4 Tickets

Theme: Authentication + Roles + Permissions + Image Upload

Sprint 4 introduces the security layer, user lifecycle, role/permission-based access, and property image upload support. Tickets are grouped by Epics and ordered by implementation dependency.

---

# Epic 1: Auth Database Foundation

## Ticket ES-111: Create Users Table with GUID Id

## Epic

Auth Database Foundation

## Type

Database

## Priority

Must-have

## Goal

Create the core `Users` table for all login-capable accounts in EstateIQ. This table will support public users, admins, company admins, and agents. It is the base dependency for register, login, email verification, refresh tokens, and authorization.

## Current Context

The backend already uses ASP.NET Core Web API, EF Core 9, SQL Server/LocalDB, DTOs, repositories, services, and migrations. Existing domain tables include Companies, Agents, AgentCompanies, PropertyTypes, PropertyStatuses, and Properties. Build on the current EF Core entity and DbContext patterns.

## Required Behavior

* Create a `User` entity with `Guid Id`.
* Store user profile fields: first name, last name, email.
* Store only hashed passwords in `PasswordHash`.
* Add `IsEmailConfirmed` and `IsActive` flags.
* Add timestamps.
* Add unique email index.

## API Contract

N/A

## Database Changes

Create `Users` table:

* `Id uniqueidentifier PK`
* `FirstName nvarchar(100) not null`
* `LastName nvarchar(100) not null`
* `Email nvarchar(255) not null`
* `PasswordHash nvarchar(max) not null`
* `IsEmailConfirmed bit not null default 0`
* `IsActive bit not null default 1`
* `CreatedAt datetime2 not null`
* `UpdatedAt datetime2 null`

Indexes/constraints:

* Unique index on `Email`.

## Authorization Rules

N/A

## Role / Permission Rules

N/A

## Frontend Requirements

N/A

## Backend Requirements

* Add entity: `backend/EstateIQ/Entities/User.cs`.
* Add `DbSet<User> Users` to DbContext.
* Configure unique email index using Fluent API.
* Generate migration.

## Validation Rules

* `FirstName` required, max 100.
* `LastName` required, max 100.
* `Email` required, max 255, unique.
* `PasswordHash` required.

## Business Rules

* Never store plain text passwords.
* New users should later be created with `IsEmailConfirmed = false` and `IsActive = true`.

## Files Likely To Change

* `backend/EstateIQ/Entities/User.cs`
* `backend/EstateIQ/Data/ApplicationDbContext.cs`
* `backend/EstateIQ/Migrations/*`

## Acceptance Criteria

* Users table exists in DB.
* Email has a unique index.
* Migration applies successfully.
* Backend builds successfully.

## Tests Required

* Verify DbContext includes Users.
* Verify unique email constraint if existing migration tests pattern supports it.

## Manual Verification Steps

* Run `dotnet ef migrations add AddUsersTable`.
* Run `dotnet ef database update`.
* Run `dotnet build backend/EstateIQ/EstateIQ.csproj --configuration Release`.
* Check SQL Server table schema.

## Out Of Scope

* Do not implement register/login.
* Do not implement roles.
* Do not implement password hashing service.

## Dependencies

None.

## Blocks

* Register endpoint
* Login endpoint
* Refresh tokens
* User management

## Notes For Codex

Follow existing Entity -> DbContext -> Migration style. Keep changes scoped. Do not refactor existing property/map/domain logic.

---

## Ticket ES-112: Create Roles and Permissions Tables with Seed Data

## Epic

Auth Database Foundation

## Type

Database

## Priority

Must-have

## Goal

Create the base authorization catalog for EstateIQ. Roles define high-level user identity, while permissions define fine-grained actions. This enables role-based and permission-based security later in the sprint.

## Current Context

The project currently has no security tables. The sprint scope requires roles Admin, CompanyAdmin, Agent, and User, plus permissions for users, companies, agents, properties, images, and future booking.

## Required Behavior

* Create `Role` entity.
* Create `Permission` entity.
* Seed exact roles.
* Seed exact permissions.
* Use GUID Id values for seed stability.

## API Contract

N/A

## Database Changes

Create `Roles` table:

* `Id uniqueidentifier PK`
* `Name nvarchar(100) not null`
* `Description nvarchar(255) null`
* `CreatedAt datetime2 not null`

Create `Permissions` table:

* `Id uniqueidentifier PK`
* `Name nvarchar(100) not null`
* `Description nvarchar(255) null`

Indexes:

* Unique index on `Roles.Name`.
* Unique index on `Permissions.Name`.

Seed roles:

* `Admin`
* `CompanyAdmin`
* `Agent`
* `User`

Seed permissions:

* `ManageUsers`
* `ManageCompanies`
* `ManageAgents`
* `CreateProperty`
* `EditProperty`
* `DeleteProperty`
* `UploadPropertyImages`
* `ViewProperties`
* `BookViewing`

## Authorization Rules

N/A

## Role / Permission Rules

This ticket creates the role and permission catalog only. It does not assign permissions to roles yet.

## Frontend Requirements

N/A

## Backend Requirements

* Add `Role` entity.
* Add `Permission` entity.
* Add DbSets to DbContext.
* Configure unique indexes.
* Seed roles and permissions through EF Core model builder or existing seed pattern.

## Validation Rules

* Role name required, max 100.
* Permission name required, max 100.
* Names must be unique.

## Business Rules

* Role and permission names must remain stable because backend policies and frontend navigation will depend on them.

## Files Likely To Change

* `backend/EstateIQ/Entities/Role.cs`
* `backend/EstateIQ/Entities/Permission.cs`
* `backend/EstateIQ/Data/ApplicationDbContext.cs`
* `backend/EstateIQ/Migrations/*`

## Acceptance Criteria

* Roles table exists.
* Permissions table exists.
* Required roles are seeded.
* Required permissions are seeded.
* Names are unique.
* Backend builds successfully.

## Tests Required

* Test seed data exists if seed tests pattern exists.

## Manual Verification Steps

* Run migration and database update.
* Query `Roles` and `Permissions`.
* Run backend build.

## Out Of Scope

* Do not assign permissions to roles.
* Do not protect APIs yet.
* Do not create auth endpoints.

## Dependencies

* Create Users Table with GUID Id may be done before or in parallel.

## Blocks

* UserRoles and RolePermissions
* Authorization policies

## Notes For Codex

Use exact role and permission names. Do not rename them or introduce extra roles unless explicitly required.

---

## Ticket ES-113: Create UserRoles and RolePermissions Join Tables

## Epic

Auth Database Foundation

## Type

Database

## Priority

Must-have

## Goal

Create many-to-many relationships between Users and Roles, and between Roles and Permissions. This enables assigning roles to users and permissions to roles.

## Current Context

Users, Roles, and Permissions are created in earlier tickets. The project uses EF Core relationships in existing domain entities, such as AgentCompanies linking Agents and Companies.

## Required Behavior

* Create `UserRole` entity.
* Create `RolePermission` entity.
* Configure relationships with foreign keys.
* Seed RolePermissions for each role.
* Prevent duplicate assignments.

## API Contract

N/A

## Database Changes

Create `UserRoles` table:

* `Id uniqueidentifier PK`
* `UserId uniqueidentifier not null FK Users.Id`
* `RoleId uniqueidentifier not null FK Roles.Id`
* `AssignedAt datetime2 not null`

Create `RolePermissions` table:

* `Id uniqueidentifier PK`
* `RoleId uniqueidentifier not null FK Roles.Id`
* `PermissionId uniqueidentifier not null FK Permissions.Id`

Indexes:

* Unique composite index on `(UserId, RoleId)`.
* Unique composite index on `(RoleId, PermissionId)`.

Suggested role permission mapping:

* Admin: all permissions.
* CompanyAdmin: `ManageAgents`, `CreateProperty`, `EditProperty`, `DeleteProperty`, `UploadPropertyImages`, `ViewProperties`.
* Agent: `CreateProperty`, `EditProperty`, `DeleteProperty`, `UploadPropertyImages`, `ViewProperties`.
* User: `ViewProperties`, `BookViewing`.

## Authorization Rules

N/A

## Role / Permission Rules

Seed exact RolePermissions using stable seeded role and permission IDs.

## Frontend Requirements

N/A

## Backend Requirements

* Add `UserRole` entity.
* Add `RolePermission` entity.
* Add navigation properties where useful.
* Add DbSets.
* Configure FK relationships and composite unique indexes.
* Add seed mapping.

## Validation Rules

* Duplicate user-role pair not allowed.
* Duplicate role-permission pair not allowed.

## Business Rules

* A user may have one or more roles.
* A role may have multiple permissions.

## Files Likely To Change

* `backend/EstateIQ/Entities/UserRole.cs`
* `backend/EstateIQ/Entities/RolePermission.cs`
* `backend/EstateIQ/Entities/User.cs`
* `backend/EstateIQ/Entities/Role.cs`
* `backend/EstateIQ/Entities/Permission.cs`
* `backend/EstateIQ/Data/ApplicationDbContext.cs`
* `backend/EstateIQ/Migrations/*`

## Acceptance Criteria

* UserRoles and RolePermissions tables exist.
* Relationships are valid.
* Duplicate assignments are prevented.
* RolePermissions are seeded.
* Backend builds successfully.

## Tests Required

* Seed validation for RolePermissions if feasible.

## Manual Verification Steps

* Run migration and database update.
* Query RolePermissions and verify mapping.
* Run backend build.

## Out Of Scope

* No API endpoints for assigning roles yet.
* No authorization middleware yet.

## Dependencies

* Create Users Table with GUID Id.
* Create Roles and Permissions Tables with Seed Data.

## Blocks

* Register role assignment
* Login claims
* Permission policies
* User management

## Notes For Codex

Use existing many-to-many relationship style if present. Keep seed IDs deterministic.

---

## Ticket ES-114: Create Refresh, Email Verification, and Password Reset Token Tables

## Epic

Auth Database Foundation

## Type

Database

## Priority

Must-have

## Goal

Create token storage tables needed for secure authentication flows. Refresh tokens support session continuation, email verification enforces verified accounts, and password reset tokens prepare the system for account recovery.

## Current Context

Sprint 4 requires JWT with refresh tokens and simulated email verification. Password reset token structure is included now, even if full UI is not completed in Sprint 4.

## Required Behavior

* Create RefreshTokens table.
* Create EmailVerificationTokens table.
* Create PasswordResetTokens table.
* Link all token tables to Users.
* Store refresh tokens as hashes, not plain token strings.
* Include expiration and revoked/used metadata.

## API Contract

N/A

## Database Changes

Create `RefreshTokens`:

* `Id uniqueidentifier PK`
* `UserId uniqueidentifier not null FK Users.Id`
* `TokenHash nvarchar(max) not null`
* `ExpiresAt datetime2 not null`
* `RevokedAt datetime2 null`
* `CreatedAt datetime2 not null`

Create `EmailVerificationTokens`:

* `Id uniqueidentifier PK`
* `UserId uniqueidentifier not null FK Users.Id`
* `Token nvarchar(255) not null`
* `ExpiresAt datetime2 not null`
* `UsedAt datetime2 null`
* `CreatedAt datetime2 not null`

Create `PasswordResetTokens`:

* `Id uniqueidentifier PK`
* `UserId uniqueidentifier not null FK Users.Id`
* `Token nvarchar(255) not null`
* `ExpiresAt datetime2 not null`
* `UsedAt datetime2 null`
* `CreatedAt datetime2 not null`

Indexes:

* Index on `UserId` for all token tables.
* Unique index on token for email verification and password reset if practical.

## Authorization Rules

N/A

## Role / Permission Rules

N/A

## Frontend Requirements

N/A

## Backend Requirements

* Add token entities.
* Add DbSets.
* Configure relationships to User.
* Generate migration.

## Validation Rules

* Expiration dates required.
* Token strings required.
* Refresh token hash required.

## Business Rules

* Refresh tokens must be revocable.
* Expired or used verification/reset tokens must not be accepted later.

## Files Likely To Change

* `backend/EstateIQ/Entities/RefreshToken.cs`
* `backend/EstateIQ/Entities/EmailVerificationToken.cs`
* `backend/EstateIQ/Entities/PasswordResetToken.cs`
* `backend/EstateIQ/Data/ApplicationDbContext.cs`
* `backend/EstateIQ/Migrations/*`

## Acceptance Criteria

* All three token tables exist.
* FK relationship to Users exists.
* Backend builds successfully.

## Tests Required

* N/A for this database-only ticket unless migration tests exist.

## Manual Verification Steps

* Run migration and database update.
* Query table schemas.
* Run backend build.

## Out Of Scope

* Do not implement token generation service here.
* Do not implement auth endpoints here.

## Dependencies

* Create Users Table with GUID Id.

## Blocks

* Register endpoint
* Verify email endpoint
* Refresh endpoint
* Logout endpoint

## Notes For Codex

Refresh tokens must be stored hashed when implemented later. This ticket only creates storage.

---

## Ticket ES-115: Create Files Table for Property Image Metadata

## Epic

Auth Database Foundation

## Type

Database

## Priority

Must-have

## Goal

Create a generic Files table to store metadata for uploaded files, starting with property images. This supports multiple images per property while keeping actual files on local disk.

## Current Context

Properties already exist and have details/map/list UI. Sprint 4 will add local image upload using `wwwroot/uploads/properties/{propertyId}` and store metadata in DB.

## Required Behavior

* Create `FileRecord` or `FileEntity` entity mapped to `Files` table.
* Use GUID Id.
* Store entity name and entity id.
* Store file name, path, content type, size, uploader, and timestamp.
* Support `Entity = "Property"`.

## API Contract

N/A

## Database Changes

Create `Files` table:

* `Id uniqueidentifier PK`
* `Entity nvarchar(100) not null`
* `EntityId uniqueidentifier not null`
* `FileName nvarchar(255) not null`
* `FilePath nvarchar(500) not null`
* `ContentType nvarchar(100) not null`
* `FileSize bigint not null`
* `UploadedBy uniqueidentifier null FK Users.Id`
* `CreatedAt datetime2 not null`

Indexes:

* Index on `(Entity, EntityId)`.
* Index on `UploadedBy`.

## Authorization Rules

N/A

## Role / Permission Rules

N/A

## Frontend Requirements

N/A

## Backend Requirements

* Add file metadata entity.
* Add DbSet.
* Configure optional relationship to User for UploadedBy.
* Generate migration.

## Validation Rules

* FileName required.
* FilePath required.
* Entity required.
* FileSize must be greater than 0 when inserting.

## Business Rules

* Files table stores metadata only.
* Binary files are stored locally, not in SQL Server.

## Files Likely To Change

* `backend/EstateIQ/Entities/FileRecord.cs`
* `backend/EstateIQ/Data/ApplicationDbContext.cs`
* `backend/EstateIQ/Migrations/*`

## Acceptance Criteria

* Files table exists.
* `(Entity, EntityId)` index exists.
* Backend builds successfully.

## Tests Required

* N/A unless migration tests exist.

## Manual Verification Steps

* Run migration and update DB.
* Verify Files schema.
* Run backend build.

## Out Of Scope

* Do not implement upload endpoint.
* Do not implement validation logic.
* Do not change property UI yet.

## Dependencies

* Create Users Table with GUID Id.

## Blocks

* Image upload backend
* Image gallery frontend

## Notes For Codex

Use a neutral entity name like `FileRecord` if `File` conflicts with system types. Map table name explicitly to `Files`.

---

# Epic 2: Authentication Core

## Ticket ES-116: Implement Auth Support Services for Password Hashing and Token Generation

## Epic

Authentication Core

## Type

Backend

## Priority

Must-have

## Goal

Create reusable services for password hashing, JWT access token generation, refresh token generation, token hashing, and verification token generation. This keeps authentication logic clean and prevents duplication across register, login, refresh, and logout endpoints.

## Current Context

The project already uses layered architecture: Controller -> Service -> Repository. Custom exceptions exist and should be reused. Auth tables must already exist.

## Required Behavior

* Add password hashing abstraction using ASP.NET `PasswordHasher<User>`.
* Add JWT generation service.
* Add refresh token generation service.
* Hash refresh tokens before DB storage.
* Add verification token generator.
* Register services in DI.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

N/A

## Role / Permission Rules

* JWT should include user id, email, role claims, and permission claims or role claims sufficient for later policy checks.

## Frontend Requirements

N/A

## Backend Requirements

* Create interfaces, e.g. `IPasswordService`, `ITokenService`.
* Implement password hash/verify methods.
* Implement access token generation using JWT settings from configuration.
* Implement refresh token generation using cryptographically secure random bytes.
* Implement token hashing with SHA-256 or stronger deterministic hash for refresh token lookup.
* Add config section for JWT issuer, audience, key, access token minutes, refresh token days.

## Validation Rules

* JWT secret must exist in configuration.
* Access token expiration recommended: 15 minutes.
* Refresh token expiration recommended: 7 days.

## Business Rules

* Never return or store plain password.
* Never store plain refresh token in DB.
* Verification token may be stored as plain token for Sprint 4 simulation unless project chooses to hash it later.

## Files Likely To Change

* `backend/EstateIQ/Services/Auth/*`
* `backend/EstateIQ/Program.cs`
* `backend/EstateIQ/appsettings.json`
* `backend/EstateIQ/appsettings.Development.json`

## Acceptance Criteria

* Password hashing service works.
* JWT generation service works.
* Refresh token generation and hashing work.
* Services registered in DI.
* Backend builds successfully.

## Tests Required

* Password verify succeeds with correct password.
* Password verify fails with wrong password.
* Refresh token hashing is deterministic.
* JWT service returns non-empty token.

## Manual Verification Steps

* Run backend tests.
* Run backend build.

## Out Of Scope

* No API endpoints in this ticket.
* No frontend changes.

## Dependencies

* Create Users Table with GUID Id.
* Create Roles and Permissions Tables with Seed Data.
* Create UserRoles and RolePermissions Join Tables.
* Create Refresh, Email Verification, and Password Reset Token Tables.

## Blocks

* Register endpoint
* Login endpoint
* Refresh endpoint
* Logout endpoint

## Notes For Codex

Keep service names simple and consistent with existing service naming. Do not place auth logic in controllers.

---

## Ticket ES-117: Implement Public User Register Endpoint

## Epic

Authentication Core

## Type

Backend

## Priority

Must-have

## Goal

Allow public visitors to register as normal Users. Registration creates an inactive-for-login account until email verification is completed.

## Current Context

There is no auth endpoint yet. Users, Roles, UserRoles, EmailVerificationTokens, and auth support services should exist. Custom exceptions should be used for validation and business rules.

## Required Behavior

* Add `POST /api/auth/register`.
* Register only role `User` publicly.
* Hash password with PasswordHasher.
* Create user with `IsEmailConfirmed = false` and `IsActive = true`.
* Assign User role.
* Generate email verification token.
* Store token in `EmailVerificationTokens`.
* Simulate email by returning token in response and/or logging it.

## API Contract

Endpoint: `POST /api/auth/register`

Request:

```json
{
  "firstName": "Jon",
  "lastName": "Ukmata",
  "email": "jon@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!"
}
```

Success 201:

```json
{
  "message": "Registration successful. Please verify your email before logging in.",
  "verificationToken": "token-for-development"
}
```

Errors:

* 400 if validation fails.
* 409 or 400 if email already exists.

## Database Changes

Uses:

* Users
* UserRoles
* EmailVerificationTokens

## Authorization Rules

Public.

## Role / Permission Rules

* Always assign role `User`.
* Do not allow public registration as Admin, CompanyAdmin, or Agent.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add `AuthController` if not existing.
* Add request/response DTOs.
* Add `IAuthService.RegisterAsync`.
* Add repository methods as needed.
* Use custom exceptions.
* Normalize email consistently, preferably lower-case trim.

## Validation Rules

* FirstName required, max 100.
* LastName required, max 100.
* Email required, valid format, max 255.
* Password required, minimum 8 chars, should include uppercase, lowercase, number, and symbol.
* ConfirmPassword must match Password.

## Business Rules

* Email must be unique.
* New user cannot login before email verification.
* Public register creates only User accounts.

## Files Likely To Change

* `backend/EstateIQ/Controllers/AuthController.cs`
* `backend/EstateIQ/DTOs/Auth/RegisterRequestDto.cs`
* `backend/EstateIQ/DTOs/Auth/RegisterResponseDto.cs`
* `backend/EstateIQ/Services/Auth/AuthService.cs`
* `backend/EstateIQ/Repositories/*`
* `backend/EstateIQ/Program.cs`

## Acceptance Criteria

* Public user can register.
* Password is hashed.
* User role is assigned.
* Verification token is stored.
* Verification token is visible for development.
* Existing tests still pass.

## Tests Required

* Register creates user.
* Register stores hashed password, not plain password.
* Register assigns User role.
* Register creates verification token.
* Duplicate email rejected.

## Manual Verification Steps

* Run API.
* Use Swagger to call register.
* Check Users table.
* Check UserRoles table.
* Check EmailVerificationTokens table.

## Out Of Scope

* No real SMTP email sending.
* No frontend register page.
* No login implementation.

## Dependencies

* Implement Auth Support Services for Password Hashing and Token Generation.

## Blocks

* Verify email endpoint
* Login endpoint
* Frontend register page

## Notes For Codex

Keep register logic inside service. Controller should only accept request and return response.

---

## Ticket ES-118: Implement Email Verification Endpoint

## Epic

Authentication Core

## Type

Backend

## Priority

Must-have

## Goal

Allow registered users to verify their email using the generated verification token. This enables the rule that users may only login after email confirmation.

## Current Context

Register endpoint creates `EmailVerificationTokens` and users with `IsEmailConfirmed = false`. Email delivery is simulated in Sprint 4.

## Required Behavior

* Add `POST /api/auth/verify-email`.
* Accept verification token.
* Validate token exists, not expired, not used.
* Set `User.IsEmailConfirmed = true`.
* Mark token as used or delete it.
* Return success message.

## API Contract

Endpoint: `POST /api/auth/verify-email`

Request:

```json
{
  "token": "verification-token"
}
```

Success 200:

```json
{
  "message": "Email verified successfully. You can now login."
}
```

Errors:

* 400 invalid token.
* 400 expired token.
* 404 user not found if user linked to token no longer exists.

## Database Changes

Uses:

* Users
* EmailVerificationTokens

## Authorization Rules

Public.

## Role / Permission Rules

N/A

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add request DTO.
* Add service method.
* Use UTC timestamps if project uses UTC.
* Handle already verified user gracefully.

## Validation Rules

* Token required.

## Business Rules

* Login should remain blocked until `IsEmailConfirmed = true`.
* Expired token cannot verify email.
* Used token cannot be reused.

## Files Likely To Change

* `backend/EstateIQ/Controllers/AuthController.cs`
* `backend/EstateIQ/DTOs/Auth/VerifyEmailRequestDto.cs`
* `backend/EstateIQ/Services/Auth/AuthService.cs`
* `backend/EstateIQ/Repositories/*`

## Acceptance Criteria

* Valid token verifies user.
* Expired/invalid token fails.
* Token cannot be reused.
* Backend builds successfully.

## Tests Required

* Verify email changes `IsEmailConfirmed` to true.
* Invalid token returns error.
* Expired token returns error.

## Manual Verification Steps

* Register user.
* Copy token from response.
* Call verify endpoint.
* Check Users table.

## Out Of Scope

* No resend verification email.
* No real email sending.

## Dependencies

* Implement Public User Register Endpoint.

## Blocks

* Login success flow
* Frontend verify email page

## Notes For Codex

Do not bypass token validation. Keep error messages clear for frontend.

---

## Ticket ES-119: Implement Login Endpoint with JWT Access Token and Refresh Token

## Epic

Authentication Core

## Type

Backend

## Priority

Must-have

## Goal

Allow verified active users to login and receive an access token plus refresh token. This is the main authentication entry point for all roles.

## Current Context

Users can register and verify email. Password hashing and token generation services should already exist. Roles and permissions exist and should be included in claims or made available for frontend/auth checks.

## Required Behavior

* Add `POST /api/auth/login`.
* Validate email and password.
* Reject inactive users.
* Reject users with unverified email.
* Generate JWT access token.
* Generate refresh token.
* Store hashed refresh token in DB.
* Return access token, expiration, user info, roles, permissions.
* Prefer setting refresh token as httpOnly cookie if current API helper/frontend can support credentials. If not, return token in response temporarily but structure code to support cookies.

## API Contract

Endpoint: `POST /api/auth/login`

Request:

```json
{
  "email": "jon@example.com",
  "password": "Password123!"
}
```

Success 200:

```json
{
  "accessToken": "jwt-token",
  "expiresAt": "2026-05-02T12:15:00Z",
  "user": {
    "id": "guid",
    "firstName": "Jon",
    "lastName": "Ukmata",
    "email": "jon@example.com",
    "roles": ["User"],
    "permissions": ["ViewProperties", "BookViewing"]
  }
}
```

Errors:

* 400 invalid credentials.
* 403 email not verified.
* 403 inactive account.

## Database Changes

Uses:

* Users
* UserRoles
* Roles
* RolePermissions
* Permissions
* RefreshTokens

## Authorization Rules

Public.

## Role / Permission Rules

* JWT should include role claims.
* Response should include roles and permissions for frontend navigation.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add login DTOs.
* Add login service method.
* Load user roles and permissions efficiently.
* Add refresh token DB record.
* Add cookie helper if using httpOnly cookie.

## Validation Rules

* Email required.
* Password required.

## Business Rules

* Only verified users can login.
* Only active users can login.
* Failed login should not reveal whether email exists.

## Files Likely To Change

* `backend/EstateIQ/Controllers/AuthController.cs`
* `backend/EstateIQ/DTOs/Auth/LoginRequestDto.cs`
* `backend/EstateIQ/DTOs/Auth/LoginResponseDto.cs`
* `backend/EstateIQ/Services/Auth/AuthService.cs`
* `backend/EstateIQ/Services/Auth/TokenService.cs`
* `backend/EstateIQ/Repositories/*`
* `backend/EstateIQ/Program.cs`

## Acceptance Criteria

* Verified user can login.
* Unverified user cannot login.
* Wrong password fails.
* Inactive user cannot login.
* Refresh token saved hashed.
* Response includes roles and permissions.

## Tests Required

* Login fails before email verification.
* Login succeeds after verification.
* Invalid password fails.
* Refresh token is stored hashed.

## Manual Verification Steps

* Register user.
* Try login before verification; expect failure.
* Verify email.
* Login; expect JWT and user data.

## Out Of Scope

* No frontend login page.
* No refresh endpoint implementation.

## Dependencies

* Implement Email Verification Endpoint.
* Implement Auth Support Services for Password Hashing and Token Generation.

## Blocks

* JWT middleware testing
* Frontend auth store

## Notes For Codex

Keep auth response stable because frontend will depend on it. Do not expose password hash or refresh token hash.

---

## Ticket ES-120: Implement Refresh Token Endpoint

## Epic

Authentication Core

## Type

Backend

## Priority

Must-have

## Goal

Allow authenticated sessions to obtain a new access token using a valid refresh token. This supports short-lived JWT access tokens without forcing users to login repeatedly.

## Current Context

Login creates refresh token records in DB. Access token expiration should be short, recommended 15 minutes, with refresh token expiration of 7 days.

## Required Behavior

* Add `POST /api/auth/refresh`.
* Read refresh token from httpOnly cookie or request body depending on chosen login behavior.
* Hash incoming refresh token and find matching DB record.
* Reject expired/revoked tokens.
* Generate new access token.
* Optionally rotate refresh token if manageable; otherwise keep same token until expiry for Sprint 4.

## API Contract

Endpoint: `POST /api/auth/refresh`

Request if body-based fallback:

```json
{
  "refreshToken": "plain-refresh-token"
}
```

Success 200:

```json
{
  "accessToken": "new-jwt-token",
  "expiresAt": "2026-05-02T12:30:00Z"
}
```

Errors:

* 401 missing token.
* 401 invalid token.
* 401 expired or revoked token.

## Database Changes

Uses `RefreshTokens`.

## Authorization Rules

Public endpoint, but requires valid refresh token.

## Role / Permission Rules

New JWT must include current roles/permissions.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add refresh DTO if body-based.
* Add service method.
* Load user and claims.
* Respect revoked/expired logic.

## Validation Rules

* Refresh token required.

## Business Rules

* Revoked refresh tokens cannot be used.
* Expired refresh tokens cannot be used.
* Inactive users cannot refresh.

## Files Likely To Change

* `backend/EstateIQ/Controllers/AuthController.cs`
* `backend/EstateIQ/DTOs/Auth/RefreshTokenRequestDto.cs`
* `backend/EstateIQ/DTOs/Auth/RefreshTokenResponseDto.cs`
* `backend/EstateIQ/Services/Auth/AuthService.cs`
* `backend/EstateIQ/Repositories/*`

## Acceptance Criteria

* Valid refresh token returns new access token.
* Expired/revoked token rejected.
* Inactive user rejected.
* Backend builds successfully.

## Tests Required

* Valid refresh token succeeds.
* Revoked token fails.
* Expired token fails.

## Manual Verification Steps

* Login.
* Use refresh token/cookie.
* Call refresh endpoint.
* Verify new access token returned.

## Out Of Scope

* No complex multi-device session management UI.
* Refresh rotation is optional for Sprint 4.

## Dependencies

* Implement Login Endpoint with JWT Access Token and Refresh Token.

## Blocks

* Frontend automatic token refresh if implemented.

## Notes For Codex

Keep implementation simple and secure. If cookies are used, make sure CORS credentials are configured correctly.

---

## Ticket ES-121: Implement Logout Endpoint with Refresh Token Revocation

## Epic

Authentication Core

## Type

Backend

## Priority

Must-have

## Goal

Allow users to logout by revoking their refresh token. This prevents reuse of the refresh token after logout.

## Current Context

Login stores hashed refresh tokens. Refresh endpoint checks revoked tokens.

## Required Behavior

* Add `POST /api/auth/logout`.
* Accept refresh token from cookie or request body.
* Hash and find token.
* Set `RevokedAt`.
* Clear cookie if cookie-based.
* Return success even if token is already invalid to avoid leaking details.

## API Contract

Endpoint: `POST /api/auth/logout`

Success 200:

```json
{
  "message": "Logged out successfully."
}
```

Errors:

* 200 may still be returned for missing/invalid token for safe logout behavior.

## Database Changes

Updates `RefreshTokens.RevokedAt`.

## Authorization Rules

Authenticated users preferred, but endpoint may allow missing/expired access token if valid refresh token exists.

## Role / Permission Rules

N/A

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add logout method.
* Revoke refresh token.
* Clear cookie if applicable.

## Validation Rules

* N/A; logout should be tolerant.

## Business Rules

* Revoked token cannot be used again.
* Logout should not fail loudly for already-logged-out state.

## Files Likely To Change

* `backend/EstateIQ/Controllers/AuthController.cs`
* `backend/EstateIQ/Services/Auth/AuthService.cs`
* `backend/EstateIQ/Repositories/*`

## Acceptance Criteria

* Logout revokes refresh token.
* Refresh after logout fails.
* Backend builds successfully.

## Tests Required

* Logout revokes token.
* Refresh after logout fails.

## Manual Verification Steps

* Login.
* Logout.
* Try refresh.
* Expect 401.

## Out Of Scope

* No logout from all devices.
* No session management UI.

## Dependencies

* Implement Refresh Token Endpoint.

## Blocks

* Frontend logout behavior.

## Notes For Codex

Avoid exposing whether a token existed. Keep logout idempotent.

---

# Epic 3: Roles, Permissions, and API Protection

## Ticket ES-123: Configure JWT Authentication Middleware

## Epic

Roles, Permissions, and API Protection

## Type

Backend

## Priority

Must-have

## Goal

Configure ASP.NET Core JWT Bearer authentication so protected endpoints can validate access tokens. This is required before applying `[Authorize]` to APIs.

## Current Context

JWT tokens are generated by login endpoint. Program.cs already configures services, controllers, Swagger, CORS, Redis health checks, and existing routes.

## Required Behavior

* Add JWT Bearer authentication.
* Validate issuer, audience, lifetime, and signing key.
* Configure Swagger to accept Bearer tokens.
* Preserve existing APIs and Swagger behavior.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

Enables `[Authorize]` attributes.

## Role / Permission Rules

JWT should support role claims recognized by ASP.NET Core authorization.

## Frontend Requirements

N/A

## Backend Requirements

* Update `Program.cs`.
* Add authentication/authorization middleware in correct order.
* Configure Swagger security definition.
* Ensure existing public endpoints still work.

## Validation Rules

* JWT config must be present.

## Business Rules

* Expired tokens are rejected.
* Invalid signatures are rejected.

## Files Likely To Change

* `backend/EstateIQ/Program.cs`
* `backend/EstateIQ/appsettings.json`
* `backend/EstateIQ/appsettings.Development.json`

## Acceptance Criteria

* Valid JWT authenticates user.
* Missing JWT fails on protected endpoint.
* Swagger supports Bearer token input.
* Existing public endpoints still work.

## Tests Required

* Protected test endpoint returns 401 without token.
* Protected test endpoint succeeds with valid token if test infrastructure supports it.

## Manual Verification Steps

* Login and copy JWT.
* Authorize in Swagger.
* Call a temporary/protected endpoint.

## Out Of Scope

* Do not protect property APIs yet.
* Do not implement frontend token storage.

## Dependencies

* Implement Login Endpoint with JWT Access Token and Refresh Token.

## Blocks

* Role-based authorization
* Permission policies
* Protected APIs

## Notes For Codex

Middleware order matters: `UseAuthentication()` before `UseAuthorization()`.

---

## Ticket ES-122: Add Role-Based Authorization Rules

## Epic

Roles, Permissions, and API Protection

## Type

Backend

## Priority

Must-have

## Goal

Enable APIs to restrict access by role, such as Admin, CompanyAdmin, Agent, and User. This provides the first layer of API protection.

## Current Context

JWT middleware is configured. Login returns JWT with role claims. Roles are seeded in DB.

## Required Behavior

* Ensure JWT contains ASP.NET-compatible role claims.
* Add constants for role names.
* Apply or prepare role-based attributes.
* Ensure role checks work in tests/API.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

* Admin-only endpoints must require Admin role.
* Property management later requires Admin, CompanyAdmin, or Agent.

## Role / Permission Rules

Roles:

* Admin
* CompanyAdmin
* Agent
* User

## Frontend Requirements

N/A

## Backend Requirements

* Add role constants if project has constants folder.
* Ensure token service emits role claims correctly.
* Add helper methods if needed.

## Validation Rules

N/A

## Business Rules

* Do not trust frontend role values; backend JWT claims and DB rules are source of truth.

## Files Likely To Change

* `backend/EstateIQ/Services/Auth/TokenService.cs`
* `backend/EstateIQ/Constants/Roles.cs`
* `backend/EstateIQ/Program.cs`

## Acceptance Criteria

* `[Authorize(Roles = "Admin")]` works.
* Multi-role authorization works.
* Backend builds successfully.

## Tests Required

* Admin token can access Admin endpoint.
* User token cannot access Admin endpoint.

## Manual Verification Steps

* Login as seeded/admin user or create manually.
* Test role-protected endpoint through Swagger.

## Out Of Scope

* Permission policies are separate.
* No frontend role nav yet.

## Dependencies

* Configure JWT Authentication Middleware.

## Blocks

* Protect Property APIs
* User management

## Notes For Codex

Use exact role names. Avoid magic strings in controllers if constants are feasible.

---

## Ticket ES-124: Add Permission-Based Authorization Policies

## Epic

Roles, Permissions, and API Protection

## Type

Backend

## Priority

Must-have

## Goal

Add authorization policies for fine-grained permissions such as ManageUsers and UploadPropertyImages. This allows the project to demonstrate both roles and permissions.

## Current Context

Permissions are seeded and connected to roles through RolePermissions. JWT can include permission claims or backend can resolve permissions as claims at login.

## Required Behavior

* Add permission constants.
* Add authorization policies for each permission.
* Ensure JWT contains permission claims, or implement policy handler that checks claims.
* Use simple claim-based policies for Sprint 4.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

Policies:

* `ManageUsers`
* `ManageCompanies`
* `ManageAgents`
* `CreateProperty`
* `EditProperty`
* `DeleteProperty`
* `UploadPropertyImages`
* `ViewProperties`
* `BookViewing`

## Role / Permission Rules

Use seeded RolePermissions. Token should reflect current permissions at login/refresh.

## Frontend Requirements

N/A

## Backend Requirements

* Add permission constants.
* Update TokenService to include permission claims.
* Add policies in Program.cs.
* Prefer claim type `permission`.

## Validation Rules

N/A

## Business Rules

* Backend policies must not rely on frontend state.

## Files Likely To Change

* `backend/EstateIQ/Constants/Permissions.cs`
* `backend/EstateIQ/Services/Auth/TokenService.cs`
* `backend/EstateIQ/Program.cs`

## Acceptance Criteria

* Policies are registered.
* JWT contains permission claims.
* Policy-protected endpoint works.

## Tests Required

* User without permission gets 403.
* User with permission succeeds.

## Manual Verification Steps

* Login and inspect JWT claims.
* Test policy-protected endpoint.

## Out Of Scope

* No dynamic permission editing UI.

## Dependencies

* Add Role-Based Authorization Rules.
* Create UserRoles and RolePermissions Join Tables.

## Blocks

* Protect Property APIs
* Protect Company/Agent APIs
* User management permission checks

## Notes For Codex

Use policies for permission checks and roles only when simpler. Keep claims format consistent.

---

## Ticket ES-125: Protect Property Create, Edit, and Delete APIs

## Epic

Roles, Permissions, and API Protection

## Type

Backend

## Priority

Must-have

## Goal

Prevent unauthorized users from creating, editing, or deleting properties. Public browsing should remain available, but write operations must require authenticated users with proper roles/permissions.

## Current Context

Properties API already exists with GET list/detail, POST, PUT, DELETE. Frontend property list, details, create, edit, delete already work publicly. This ticket must preserve read behavior while protecting write behavior.

## Required Behavior

* Keep `GET /api/properties` public.
* Keep `GET /api/properties/{id}` public.
* Protect `POST /api/properties`.
* Protect `PUT /api/properties/{id}`.
* Protect `DELETE /api/properties/{id}`.
* Allowed roles: Admin, CompanyAdmin, Agent.
* Use permission policies where possible.

## API Contract

Existing API contracts remain unchanged.

Error responses:

* 401 if unauthenticated.
* 403 if authenticated but lacks permission.

## Database Changes

N/A

## Authorization Rules

* POST requires `CreateProperty`.
* PUT requires `EditProperty`.
* DELETE requires `DeleteProperty`.
* GET endpoints remain Public.

## Role / Permission Rules

Allowed through permissions assigned to:

* Admin
* CompanyAdmin
* Agent

User role should not create/edit/delete.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add `[Authorize(Policy = ...)]` to write endpoints.
* Preserve existing service/repository behavior.
* If ownership/company checks are feasible, add basic checks for Agent/CompanyAdmin; otherwise document ownership hardening for later.

## Validation Rules

Existing property validation remains.

## Business Rules

* Users can browse properties but cannot manage them.
* Write operations require authenticated permissions.

## Files Likely To Change

* `backend/EstateIQ/Controllers/PropertiesController.cs`
* possibly `backend/EstateIQ/Services/Properties/*`

## Acceptance Criteria

* Public can still view list/details.
* Unauthenticated POST/PUT/DELETE returns 401.
* User role POST/PUT/DELETE returns 403.
* Agent/Admin/CompanyAdmin with permissions can write.
* Existing property behavior remains intact.

## Tests Required

* Unauthorized create property fails.
* User role create property fails.
* Authorized role create property succeeds.
* Public GET still succeeds.

## Manual Verification Steps

* Call GET properties without token.
* Call POST without token.
* Login as User and call POST.
* Login as Admin/Agent and call POST.

## Out Of Scope

* Do not redesign property domain.
* Do not implement frontend protected route logic here.

## Dependencies

* Add Permission-Based Authorization Policies.

## Blocks

* Frontend role-based property actions
* Image upload authorization

## Notes For Codex

Do not break current filters, pagination, map markers, or details page data contracts.

---

## Ticket ES-126: Protect Company and Agent APIs

## Epic

Roles, Permissions, and API Protection

## Type

Backend

## Priority

Must-have

## Goal

Restrict company and agent management operations according to role rules. Lookup APIs may remain readable if required by existing property forms, but management actions must be protected.

## Current Context

Existing lookup endpoints include GET companies, GET agents, and GET agents by companyId. There may not yet be full CRUD for companies/agents. User management tickets will add create CompanyAdmin and create Agent flows.

## Required Behavior

* Keep necessary lookup GET endpoints available for property forms unless security requires authentication.
* Admin manages companies.
* Admin and CompanyAdmin manage agents.
* CompanyAdmin can only manage agents for own company.
* Prepare authorization rules for new user management endpoints.

## API Contract

Existing lookup API contracts unchanged unless already protected intentionally.

## Database Changes

N/A

## Authorization Rules

* ManageCompanies: Admin only.
* ManageAgents: Admin + CompanyAdmin.
* CompanyAdmin limited to own company.

## Role / Permission Rules

Permissions:

* `ManageCompanies`
* `ManageAgents`

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Apply policies to any existing company/agent write endpoints.
* If only GET lookup endpoints exist, document them as public/readonly.
* Add helper to get current user id and roles from claims if needed for later ownership checks.

## Validation Rules

N/A

## Business Rules

* CompanyAdmin cannot manage agents outside own company.
* Normal User cannot access admin/company management actions.

## Files Likely To Change

* `backend/EstateIQ/Controllers/CompaniesController.cs`
* `backend/EstateIQ/Controllers/AgentsController.cs`
* `backend/EstateIQ/Services/*`
* `backend/EstateIQ/Extensions/*` or `Helpers/*`

## Acceptance Criteria

* Company write actions require Admin.
* Agent write actions require Admin or CompanyAdmin.
* Read lookup behavior remains compatible with existing frontend.
* Backend builds successfully.

## Tests Required

* User cannot manage companies/agents.
* CompanyAdmin cannot manage agents outside own company where applicable.

## Manual Verification Steps

* Test existing dropdown endpoints in frontend.
* Test protected management endpoint through Swagger.

## Out Of Scope

* Full user management implementation is separate.
* No frontend role navigation here.

## Dependencies

* Add Permission-Based Authorization Policies.

## Blocks

* User management tickets

## Notes For Codex

Be careful not to break property create/edit dropdowns that depend on company/agent lookup endpoints.

---

# Epic 4: User Management

## Ticket ES-127: Implement Admin User List Endpoint

## Epic

User Management

## Type

Backend

## Priority

Must-have

## Goal

Allow Admin to view users with their roles and account status. This provides the foundation for admin user management.

## Current Context

Users, roles, permissions, JWT authorization, and policy checks should already exist. There is no user management API yet.

## Required Behavior

* Add `GET /api/users`.
* Return paginated user list.
* Include roles.
* Include active/email verification status.
* Support optional search by name/email.

## API Contract

Endpoint: `GET /api/users?search=&page=1&pageSize=10`

Success 200:

```json
{
  "items": [
    {
      "id": "guid",
      "firstName": "Jon",
      "lastName": "Ukmata",
      "email": "jon@example.com",
      "isActive": true,
      "isEmailConfirmed": true,
      "roles": ["User"]
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

Errors:

* 401 unauthenticated.
* 403 not Admin / missing ManageUsers.

## Database Changes

N/A

## Authorization Rules

Admin only or permission `ManageUsers`.

## Role / Permission Rules

Requires `ManageUsers`.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add `UsersController`.
* Add DTOs.
* Add user service and repository methods.
* Reuse existing pagination style from properties if available.

## Validation Rules

* Page >= 1.
* PageSize between 1 and 100.

## Business Rules

* Do not return password hash or tokens.

## Files Likely To Change

* `backend/EstateIQ/Controllers/UsersController.cs`
* `backend/EstateIQ/DTOs/Users/*`
* `backend/EstateIQ/Services/Users/*`
* `backend/EstateIQ/Repositories/Users/*`

## Acceptance Criteria

* Admin can list users.
* Non-admin cannot list users.
* Response excludes sensitive data.
* Pagination works.

## Tests Required

* Admin list users succeeds.
* User list users fails.
* Response does not contain password hash.

## Manual Verification Steps

* Login as Admin.
* Call GET /api/users.
* Login as User.
* Confirm 403.

## Out Of Scope

* No frontend admin page.
* No create/update user in this ticket.

## Dependencies

* Protect Company and Agent APIs.
* Add Permission-Based Authorization Policies.

## Blocks

* Admin user management frontend later

## Notes For Codex

Follow existing pagination response pattern from properties.

---

## Ticket ES-128: Implement Admin Create CompanyAdmin Endpoint

## Epic

User Management

## Type

Backend

## Priority

Must-have

## Goal

Allow Admin to create CompanyAdmin accounts and link them to a company. This supports the selected business flow where companies do not self-register and are verified outside the system.

## Current Context

Companies already exist as seed/domain data. CompanyAdmin is a role. Users and UserRoles exist. There may not yet be a CompanyAdmin-specific relationship table, so use a practical relationship pattern consistent with current domain model.

## Required Behavior

* Add endpoint for Admin to create CompanyAdmin.
* Create user with role CompanyAdmin.
* Link CompanyAdmin to selected Company.
* User should be active.
* Decide whether email is auto-confirmed for admin-created accounts. Recommended: `IsEmailConfirmed = true` for admin-created accounts in Sprint 4.
* Generate temporary password or accept password in request. Recommended for Sprint 4: accept password in request.

## API Contract

Endpoint: `POST /api/users/company-admins`

Request:

```json
{
  "firstName": "Company",
  "lastName": "Admin",
  "email": "companyadmin@example.com",
  "password": "Password123!",
  "companyId": "guid"
}
```

Success 201:

```json
{
  "id": "guid",
  "email": "companyadmin@example.com",
  "role": "CompanyAdmin",
  "companyId": "guid"
}
```

Errors:

* 400 validation failure.
* 404 company not found.
* 409 email exists.
* 403 not Admin.

## Database Changes

May require relationship storage for CompanyAdmin -> Company.
Recommended approach:

* If Agents table represents agent domain only, do not force CompanyAdmin into Agents.
* Add `CompanyUsers` table if needed:

  * `Id Guid PK`
  * `CompanyId Guid FK`
  * `UserId Guid FK`
  * `CreatedAt`
  * unique `(CompanyId, UserId)`
  * optional `RelationshipType` if needed.

If adding this table is too much, add `CompanyId` nullable on Users only if acceptable, but CompanyUsers is cleaner.

## Authorization Rules

Admin only / `ManageUsers` + `ManageCompanies`.

## Role / Permission Rules

Creates user with role `CompanyAdmin`.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add DTO.
* Add service method.
* Validate company exists.
* Hash password.
* Assign CompanyAdmin role.
* Link user to company.

## Validation Rules

* FirstName required.
* LastName required.
* Email required valid unique.
* Password meets policy.
* CompanyId required and must exist.

## Business Rules

* CompanyAdmin cannot self-register.
* Only Admin can create CompanyAdmin.
* Company should be assumed legally verified outside system.

## Files Likely To Change

* `backend/EstateIQ/Controllers/UsersController.cs`
* `backend/EstateIQ/DTOs/Users/CreateCompanyAdminRequestDto.cs`
* `backend/EstateIQ/Services/Users/*`
* `backend/EstateIQ/Repositories/*`
* `backend/EstateIQ/Entities/CompanyUser.cs` if needed
* `backend/EstateIQ/Data/ApplicationDbContext.cs` if needed

## Acceptance Criteria

* Admin can create CompanyAdmin.
* CompanyAdmin is linked to company.
* Password is hashed.
* Duplicate email rejected.
* Non-admin rejected.

## Tests Required

* Admin create CompanyAdmin succeeds.
* Non-admin fails.
* Invalid company fails.
* Duplicate email fails.

## Manual Verification Steps

* Login as Admin.
* Call create CompanyAdmin.
* Verify Users, UserRoles, and company link.

## Out Of Scope

* No real legal company verification workflow.
* No email invite flow.
* No frontend admin form.

## Dependencies

* Implement Admin User List Endpoint.

## Blocks

* CompanyAdmin agent creation

## Notes For Codex

Prefer a clean `CompanyUsers` link table if current schema does not already support linking users to companies.

---

## Ticket ES-129: Implement CompanyAdmin/Admin Create Agent Endpoint

## Epic

User Management

## Type

Backend

## Priority

Must-have

## Goal

Allow Admin or CompanyAdmin to create Agent accounts. CompanyAdmin must only create agents for their own company.

## Current Context

Agents and AgentCompanies already exist as domain tables. Auth users now exist separately. This ticket must connect login-capable users to agent records in a practical way.

## Required Behavior

* Add endpoint to create Agent user.
* Admin can create agent for any company.
* CompanyAdmin can create agent only for own company.
* Create User with Agent role.
* Create or link Agent domain record.
* Link Agent to Company through existing AgentCompanies relationship.
* Admin-created/company-created agent may be email-confirmed by default for Sprint 4.

## API Contract

Endpoint: `POST /api/users/agents`

Request:

```json
{
  "firstName": "Agent",
  "lastName": "One",
  "email": "agent@example.com",
  "password": "Password123!",
  "phone": "+38344111222",
  "companyId": "guid"
}
```

Success 201:

```json
{
  "userId": "guid",
  "agentId": "guid",
  "email": "agent@example.com",
  "companyId": "guid"
}
```

Errors:

* 400 validation failure.
* 403 CompanyAdmin creating outside own company.
* 404 company not found.
* 409 email exists.

## Database Changes

May require adding `UserId` nullable/required to `Agents` table if not already present.
Recommended:

* Add `UserId uniqueidentifier null/unique FK Users.Id` to Agents.
* Keep AgentCompanies as existing company link.

## Authorization Rules

Admin + CompanyAdmin.

## Role / Permission Rules

Requires `ManageAgents`.
Creates user with role `Agent`.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add DTO.
* Add service logic for current user role and company scope.
* Hash password.
* Assign Agent role.
* Create/link Agent entity.
* Link agent to company.

## Validation Rules

* FirstName required.
* LastName required.
* Email required valid unique.
* Password meets policy.
* CompanyId required.
* Phone optional/max length according to existing Agent model.

## Business Rules

* Agents do not self-register.
* CompanyAdmin cannot create agents for another company.
* Agent must be linked to a company.

## Files Likely To Change

* `backend/EstateIQ/Controllers/UsersController.cs`
* `backend/EstateIQ/DTOs/Users/CreateAgentRequestDto.cs`
* `backend/EstateIQ/Services/Users/*`
* `backend/EstateIQ/Entities/Agent.cs`
* `backend/EstateIQ/Data/ApplicationDbContext.cs`
* `backend/EstateIQ/Migrations/*`

## Acceptance Criteria

* Admin can create agent for any company.
* CompanyAdmin can create agent for own company.
* CompanyAdmin cannot create agent for other company.
* Agent user has Agent role.
* AgentCompanies relation is created.

## Tests Required

* Admin create agent succeeds.
* CompanyAdmin own company succeeds.
* CompanyAdmin other company fails.
* Duplicate email fails.

## Manual Verification Steps

* Login as Admin and create agent.
* Login as CompanyAdmin and create agent for own company.
* Try other company; expect 403.

## Out Of Scope

* No frontend company admin page.
* No invitation email.

## Dependencies

* Implement Admin Create CompanyAdmin Endpoint.

## Blocks

* CompanyAdmin agent management UI later

## Notes For Codex

Be careful with current `Agents` model. Preserve existing `/api/agents` lookup behavior.

---

## Ticket ES-130: Implement Activate and Deactivate User Endpoint

## Epic

User Management

## Type

Backend

## Priority

Should-have

## Goal

Allow Admin to activate or deactivate accounts. Deactivated users must not be able to login or refresh tokens.

## Current Context

Users have `IsActive`. Login and refresh should already check active status.

## Required Behavior

* Add endpoint to update active status.
* Admin can activate/deactivate any user except maybe self-protection.
* Deactivation should revoke existing refresh tokens if feasible.

## API Contract

Endpoint: `PATCH /api/users/{id}/status`

Request:

```json
{
  "isActive": false
}
```

Success 200:

```json
{
  "id": "guid",
  "isActive": false
}
```

Errors:

* 404 user not found.
* 403 not Admin.

## Database Changes

Updates `Users.IsActive`; optionally updates `RefreshTokens.RevokedAt`.

## Authorization Rules

Admin only / `ManageUsers`.

## Role / Permission Rules

Requires `ManageUsers`.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add DTO.
* Add service method.
* Revoke active refresh tokens when deactivating if practical.
* Prevent accidental deletion; this is status only.

## Validation Rules

* `isActive` required.

## Business Rules

* Inactive users cannot login.
* Inactive users cannot refresh token.
* Do not hard delete users.

## Files Likely To Change

* `backend/EstateIQ/Controllers/UsersController.cs`
* `backend/EstateIQ/DTOs/Users/UpdateUserStatusRequestDto.cs`
* `backend/EstateIQ/Services/Users/*`
* `backend/EstateIQ/Repositories/*`

## Acceptance Criteria

* Admin can deactivate user.
* Deactivated user cannot login.
* Deactivated user cannot refresh.
* Admin can reactivate user.

## Tests Required

* Deactivate user blocks login.
* Non-admin cannot change status.
* Reactivated user can login again if verified.

## Manual Verification Steps

* Login as Admin.
* Deactivate a user.
* Try login as that user.
* Reactivate and try again.

## Out Of Scope

* No delete user.
* No audit log yet unless already implemented.

## Dependencies

* Implement Admin User List Endpoint.

## Blocks

N/A

## Notes For Codex

Keep this endpoint small. Do not add complex user profile editing here.

---

# Epic 5: Property Image Upload

## Ticket ES-131: Implement Backend Property Image Upload Endpoint

## Epic

Property Image Upload

## Type

Backend

## Priority

Must-have

## Goal

Allow authorized property managers to upload multiple images for a property. Files are stored locally and metadata is saved in the Files table.

## Current Context

Properties exist with details pages and CRUD. Files table exists. Authorization policies exist. Sprint decision: max 10 images per property, local folder `wwwroot/uploads/properties/{propertyId}`.

## Required Behavior

* Add `POST /api/properties/{id}/images`.
* Accept multipart/form-data with one or more files.
* Save images to local folder.
* Create Files records.
* Enforce max 10 total images per property.
* Return uploaded file metadata.

## API Contract

Endpoint: `POST /api/properties/{id}/images`

Request:

* multipart/form-data
* field name: `files`

Success 201:

```json
[
  {
    "id": "guid",
    "fileName": "image.jpg",
    "filePath": "/uploads/properties/{propertyId}/generated-name.jpg",
    "contentType": "image/jpeg",
    "fileSize": 123456
  }
]
```

Errors:

* 400 invalid file.
* 400 max images exceeded.
* 404 property not found.
* 401 unauthenticated.
* 403 missing permission.

## Database Changes

Uses `Files` table.

## Authorization Rules

Authenticated users with `UploadPropertyImages`.
Allowed roles through permission:

* Admin
* CompanyAdmin
* Agent

## Role / Permission Rules

Requires `UploadPropertyImages`.

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add endpoint to PropertiesController or separate PropertyImagesController.
* Add file service.
* Validate property exists.
* Save to `wwwroot/uploads/properties/{propertyId}`.
* Use generated unique file names.
* Store relative public path.

## Validation Rules

* Max 10 images per property total.
* Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.webp`.
* Allowed content types: `image/jpeg`, `image/png`, `image/webp`.
* Max size: 5 MB per image.
* At least one file required.

## Business Rules

* Do not store binary file content in SQL Server.
* Do not overwrite existing files.
* Only property managers can upload.

## Files Likely To Change

* `backend/EstateIQ/Controllers/PropertiesController.cs` or `PropertyImagesController.cs`
* `backend/EstateIQ/Services/Files/*`
* `backend/EstateIQ/DTOs/Files/*`
* `backend/EstateIQ/Repositories/*`
* `backend/EstateIQ/Program.cs` for static files if not enabled

## Acceptance Criteria

* Authorized user can upload valid images.
* Files saved to correct folder.
* Files records saved in DB.
* Max 10 enforced.
* Invalid type rejected.

## Tests Required

* Upload valid image succeeds.
* More than 10 images rejected.
* Invalid extension rejected.
* Missing property rejected.

## Manual Verification Steps

* Login as Agent/Admin.
* Upload image in Swagger/Postman.
* Check folder and Files table.
* Open returned file path in browser if static files enabled.

## Out Of Scope

* No frontend upload UI.
* No cloud storage.

## Dependencies

* Create Files Table for Property Image Metadata.
* Protect Property Create, Edit, and Delete APIs.

## Blocks

* Property image retrieval
* Frontend gallery/upload UI

## Notes For Codex

Keep file upload logic in a service, not controller. Preserve existing property endpoints.

---

## Ticket ES-132: Centralize Property Image Validation Logic

## Epic

Property Image Upload

## Type

Backend

## Priority

Must-have

## Goal

Create reusable validation logic for property image uploads. This keeps upload endpoint clean and makes tests focused.

## Current Context

The image upload endpoint needs validation for file count, type, and size. Custom exceptions exist and should be used.

## Required Behavior

* Add validation method/service for image files.
* Validate extension, content type, file size, and total property image count.
* Return clear validation errors.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

N/A

## Role / Permission Rules

N/A

## Frontend Requirements

N/A

## Backend Requirements

* Add constants for max image count and max file size.
* Max image size: 5 MB.
* Use existing `ValidationException` for invalid uploads.
* Ensure validation runs before writing files to disk.

## Validation Rules

* Max 10 images per property total.
* Max 5 MB per image.
* Allowed: jpg, jpeg, png, webp.
* Empty file rejected.

## Business Rules

* Validation must prevent unsupported files before storage.

## Files Likely To Change

* `backend/EstateIQ/Services/Files/FileValidationService.cs`
* `backend/EstateIQ/Constants/FileUploadConstants.cs`
* `backend/EstateIQ/Services/Files/*`

## Acceptance Criteria

* Validation service exists.
* Upload endpoint uses validation service.
* Error messages are clear.
* Backend builds successfully.

## Tests Required

* Invalid extension fails.
* Invalid content type fails.
* Oversized file fails.
* Empty file fails.
* More than 10 total images fails.

## Manual Verification Steps

* Try uploading `.txt`.
* Try uploading oversized image.
* Try uploading 11 images.

## Out Of Scope

* No image resizing/compression.
* No virus scanning.

## Dependencies

* Implement Backend Property Image Upload Endpoint.

## Blocks

* Image upload tests

## Notes For Codex

Make validation deterministic and easy to unit test.

---

## Ticket ES-133: Implement Property Image Retrieval Endpoint and Include Images in Property Details

## Epic

Property Image Upload

## Type

Backend

## Priority

Must-have

## Goal

Expose property images so frontend can show galleries on property details pages. This connects uploaded image metadata with property read APIs.

## Current Context

Property details endpoint already exists. Files table stores metadata with Entity=`Property` and EntityId=propertyId.

## Required Behavior

* Add `GET /api/properties/{id}/images`.
* Return image metadata for a property.
* Include images in property details response if feasible without breaking frontend.
* Keep existing property details fields intact.

## API Contract

Endpoint: `GET /api/properties/{id}/images`

Success 200:

```json
[
  {
    "id": "guid",
    "fileName": "image.jpg",
    "url": "/uploads/properties/{propertyId}/image.jpg",
    "contentType": "image/jpeg",
    "fileSize": 123456
  }
]
```

Errors:

* 404 property not found.

## Database Changes

N/A

## Authorization Rules

Public read.

## Role / Permission Rules

N/A

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add file DTO.
* Add service/repository method to get files by entity.
* Optionally extend property details DTO with `images` array.
* Ensure static files are served from `wwwroot`.

## Validation Rules

* Property id must exist.

## Business Rules

* Property images are public because property listings are public.

## Files Likely To Change

* `backend/EstateIQ/Controllers/PropertiesController.cs` or `PropertyImagesController.cs`
* `backend/EstateIQ/DTOs/Files/FileResponseDto.cs`
* `backend/EstateIQ/DTOs/Properties/PropertyDetailsDto.cs`
* `backend/EstateIQ/Services/Files/*`
* `backend/EstateIQ/Repositories/*`

## Acceptance Criteria

* Property images endpoint returns images.
* Property details can expose image data if implemented.
* Public user can view images.
* Existing property details UI still works.

## Tests Required

* Get images for property succeeds.
* Missing property returns 404.
* Property with no images returns empty list.

## Manual Verification Steps

* Upload images.
* Call GET images endpoint.
* Open returned image URL.

## Out Of Scope

* No frontend gallery yet.
* No image ordering/cover image.

## Dependencies

* Implement Backend Property Image Upload Endpoint.

## Blocks

* Frontend property details gallery

## Notes For Codex

Avoid breaking existing property details response. Add fields in backward-compatible way.

---

## Ticket ES-134: Implement Delete Property Image Endpoint

## Epic

Property Image Upload

## Type

Backend

## Priority

Should-have

## Goal

Allow authorized property managers to delete a property image. This removes metadata and the local file when possible.

## Current Context

Images can be uploaded and retrieved. Files table stores metadata and paths.

## Required Behavior

* Add `DELETE /api/properties/{propertyId}/images/{imageId}`.
* Validate property exists.
* Validate image belongs to property.
* Delete local file if it exists.
* Delete Files record.

## API Contract

Endpoint: `DELETE /api/properties/{propertyId}/images/{imageId}`

Success 204.

Errors:

* 404 property/image not found.
* 401 unauthenticated.
* 403 missing permission.

## Database Changes

Deletes row from `Files`.

## Authorization Rules

Authenticated users with `UploadPropertyImages` or `EditProperty`.

## Role / Permission Rules

Allowed:

* Admin
* CompanyAdmin
* Agent

## Frontend Requirements

N/A in this ticket.

## Backend Requirements

* Add delete endpoint.
* Add service method.
* Handle missing physical file gracefully.
* Keep DB and file system reasonably consistent.

## Validation Rules

* imageId required.
* propertyId required.
* File must belong to property.

## Business Rules

* Do not allow deleting images from another property.

## Files Likely To Change

* `backend/EstateIQ/Controllers/PropertyImagesController.cs`
* `backend/EstateIQ/Services/Files/*`
* `backend/EstateIQ/Repositories/*`

## Acceptance Criteria

* Authorized user can delete image.
* DB record removed.
* Local file removed if present.
* Wrong property/image pair returns 404.

## Tests Required

* Delete valid image succeeds.
* Delete image from wrong property fails.
* Unauthorized delete fails.

## Manual Verification Steps

* Upload image.
* Delete image.
* Confirm DB row removed.
* Confirm file removed.

## Out Of Scope

* No bulk delete.
* No soft delete unless existing system uses soft delete.

## Dependencies

* Implement Property Image Retrieval Endpoint and Include Images in Property Details.

## Blocks

* Frontend image management UI if included.

## Notes For Codex

Do not fail if physical file is already missing; remove metadata safely.

---

# Epic 6: Frontend Auth and Protected UI

## Ticket ES-135: Implement Public Register Page for User Accounts

## Epic

Frontend Auth and Protected UI

## Type

Frontend

## Priority

Must-have

## Goal

Create a public register page where only normal Users can register. Company registration is not public and should be represented by a contact message.

## Current Context

Frontend uses React 19, Vite, TypeScript, React Router, and API helper in `frontend/src/services/api.ts`. Existing routes include `/login`, but no register/verify flow yet.

## Required Behavior

* Add `/register` route.
* Add register form.
* Call `POST /api/auth/register`.
* Show returned verification token in development UI or success message.
* Show company contact message: “Jeni kompani? Na kontaktoni për verifikim.”
* Navigate or link to verify email page.

## API Contract

Uses `POST /api/auth/register`.

## Database Changes

N/A

## Authorization Rules

Public.

## Role / Permission Rules

Registers only User role through backend.

## Frontend Requirements

* Fields: firstName, lastName, email, password, confirmPassword.
* Client-side validation matching backend basics.
* Loading state.
* Error state.
* Success state with token/link for Sprint 4 simulated email.
* Link to `/login`.
* Company contact box.

## Backend Requirements

N/A

## Validation Rules

* Required fields.
* Password and confirm password must match.
* Basic email format.

## Business Rules

* Do not expose role selection.
* Do not allow CompanyAdmin/Agent registration from public UI.

## Files Likely To Change

* `frontend/src/pages/RegisterPage.tsx`
* `frontend/src/App.tsx` or route config
* `frontend/src/services/api.ts`
* `frontend/src/types/auth.ts`

## Acceptance Criteria

* User can register from UI.
* Success message appears.
* Verification token/link is visible for dev flow.
* Company contact message appears.
* Build passes.

## Tests Required

* Optional frontend component test if framework exists.
* Manual validation is acceptable if no frontend test setup.

## Manual Verification Steps

* Run `npm run build`.
* Open `/register`.
* Register user.
* Confirm success.

## Out Of Scope

* No real email inbox flow.
* No company registration form.

## Dependencies

* Implement Public User Register Endpoint.

## Blocks

* Verify email frontend flow

## Notes For Codex

Use existing API helper pattern. Keep UI consistent with current layout.

---

## Ticket ES-136: Implement Login Page and Auth State Management

## Epic

Frontend Auth and Protected UI

## Type

Frontend

## Priority

Must-have

## Goal

Connect the login page to the backend auth API and store authenticated user state for the frontend. This enables protected routes, role-based navigation, and logout.

## Current Context

A `/login` route already exists. API helper exists. Backend login returns accessToken and user info with roles/permissions.

## Required Behavior

* Update login page to call `POST /api/auth/login`.
* Store access token and user data in auth state.
* Attach access token to API requests.
* Handle unverified email error clearly.
* Add logout function that calls backend logout and clears local auth state.
* Add company contact message on login page.

## API Contract

Uses:

* `POST /api/auth/login`
* `POST /api/auth/logout`

## Database Changes

N/A

## Authorization Rules

Public login; logout for authenticated users.

## Role / Permission Rules

Frontend stores roles and permissions from backend response.

## Frontend Requirements

* Auth context/store.
* Login form email/password.
* Loading/error states.
* Store token in memory if feasible. If page refresh persistence is needed for Sprint 4, localStorage can be used temporarily, but note security tradeoff.
* API helper adds `Authorization: Bearer <token>`.
* Logout clears token/user and navigates to login/home.

## Backend Requirements

N/A

## Validation Rules

* Email required.
* Password required.

## Business Rules

* Unverified users see message telling them to verify email.
* Company registration is contact-only.

## Files Likely To Change

* `frontend/src/pages/LoginPage.tsx`
* `frontend/src/context/AuthContext.tsx` or `frontend/src/store/authStore.ts`
* `frontend/src/services/api.ts`
* `frontend/src/types/auth.ts`
* `frontend/src/components/Layout/*`

## Acceptance Criteria

* Verified user can login from UI.
* Access token is used for protected API calls.
* Logout works.
* Unverified email error shown.
* Build passes.

## Tests Required

* Manual UI verification if no frontend tests.

## Manual Verification Steps

* Register and verify user.
* Login through UI.
* Confirm user state visible in navbar/sidebar.
* Logout.
* Run `npm run build`.

## Out Of Scope

* No automatic refresh token retry unless simple to implement.
* No full session persistence hardening.

## Dependencies

* Implement Login Endpoint with JWT Access Token and Refresh Token.
* Implement Logout Endpoint with Refresh Token Revocation.

## Blocks

* Protected routes
* Role navigation

## Notes For Codex

Keep API helper backward compatible for public endpoints. Do not break existing property list/map calls.

---

## Ticket ES-137: Implement Verify Email Page

## Epic

Frontend Auth and Protected UI

## Type

Frontend

## Priority

Must-have

## Goal

Create a UI for verifying email using the token generated during registration. This completes the simulated email verification flow for Sprint 4.

## Current Context

Register returns/logs a verification token. Backend verify endpoint accepts token. There is no real email in Sprint 4.

## Required Behavior

* Add `/verify-email` route.
* Read token from query string if present.
* Allow manual token paste.
* Call `POST /api/auth/verify-email`.
* Show success and link to login.
* Show validation errors.

## API Contract

Uses `POST /api/auth/verify-email`.

## Database Changes

N/A

## Authorization Rules

Public.

## Role / Permission Rules

N/A

## Frontend Requirements

* Input for token.
* Auto-fill from `?token=`.
* Loading/success/error states.
* Link to login after success.

## Backend Requirements

N/A

## Validation Rules

* Token required.

## Business Rules

* User cannot login before verification.

## Files Likely To Change

* `frontend/src/pages/VerifyEmailPage.tsx`
* `frontend/src/App.tsx` or route config
* `frontend/src/services/api.ts`

## Acceptance Criteria

* Token from register can verify account.
* Query string token works.
* Manual paste works.
* Build passes.

## Tests Required

* Manual UI verification.

## Manual Verification Steps

* Register user.
* Copy token.
* Open `/verify-email?token=...`.
* Verify account.
* Login.

## Out Of Scope

* No resend verification email.
* No real SMTP email.

## Dependencies

* Implement Email Verification Endpoint.
* Implement Public Register Page for User Accounts.

## Blocks

* End-to-end auth demo

## Notes For Codex

Make the page simple and clear for demo purposes.

---

## Ticket ES-138: Implement Protected Routes and Role-Based Navigation

## Epic

Frontend Auth and Protected UI

## Type

Frontend

## Priority

Must-have

## Goal

Prevent unauthorized users from accessing dashboard/admin actions in the frontend and adjust navigation based on user roles/permissions.

## Current Context

Routes include `/dashboard`, `/properties`, `/map`, `/login`, details and edit pages. Some routes/actions should now depend on authentication and permissions.

## Required Behavior

* Add protected route wrapper.
* Protect `/dashboard` from normal User if dashboard is admin/company/agent-oriented.
* Hide create/edit/delete actions from users without permissions.
* Show login/logout state in nav.
* Role-based navigation:

  * Admin sees management/admin links.
  * CompanyAdmin sees company/agent/property management links.
  * Agent sees property management links.
  * User sees browsing links.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

Frontend mirrors backend but backend remains source of truth.

## Role / Permission Rules

Use roles/permissions from auth response.

## Frontend Requirements

* ProtectedRoute component.
* Permission helper functions.
* Update layout/sidebar/navbar.
* Redirect unauthorized users to login or safe page.
* Do not show property edit/delete/create buttons to plain User.

## Backend Requirements

N/A

## Validation Rules

N/A

## Business Rules

* UI should not invite users to actions they cannot perform.
* Backend authorization still controls actual access.

## Files Likely To Change

* `frontend/src/components/ProtectedRoute.tsx`
* `frontend/src/components/Layout/*`
* `frontend/src/App.tsx` or route config
* `frontend/src/pages/PropertiesPage.tsx`
* `frontend/src/pages/PropertyDetailsPage.tsx`
* `frontend/src/pages/EditPropertyPage.tsx`

## Acceptance Criteria

* Logged-out user cannot access protected pages.
* User role cannot see admin/dashboard actions.
* Agent/Admin/CompanyAdmin see property management actions.
* Existing public property browsing still works.
* Build passes.

## Tests Required

* Manual UI verification by role.

## Manual Verification Steps

* Login as User and inspect nav/actions.
* Login as Admin/Agent and inspect nav/actions.
* Try direct URL to protected route.
* Run `npm run build`.

## Out Of Scope

* No full admin user management UI unless already planned separately.
* No dashboard redesign.

## Dependencies

* Implement Login Page and Auth State Management.
* Protect Property Create, Edit, and Delete APIs.

## Blocks

* Stable Sprint 4 frontend demo

## Notes For Codex

Do not remove existing routes unless required. Preserve map/list synchronization behavior.

---

## Ticket ES-139: Implement Property Image Upload UI and Details Gallery

## Epic

Frontend Auth and Protected UI

## Type

Frontend

## Priority

Must-have

## Goal

Allow authorized users to upload property images and allow all users to view property image galleries on property details.

## Current Context

Property details and edit pages already exist. Backend image upload/retrieval endpoints exist. Auth state contains permissions.

## Required Behavior

* Show gallery on property details page.
* Fetch property images from backend or use images embedded in details response.
* Add upload UI for Admin/CompanyAdmin/Agent with `UploadPropertyImages`.
* Enforce client-side file type/size/count hints.
* Show upload progress/loading state if simple.
* Refresh gallery after upload.

## API Contract

Uses:

* `GET /api/properties/{id}/images`
* `POST /api/properties/{id}/images`
* optionally `DELETE /api/properties/{propertyId}/images/{imageId}` if delete UI included.

## Database Changes

N/A

## Authorization Rules

* Gallery view public.
* Upload UI only for users with `UploadPropertyImages`.

## Role / Permission Rules

Upload allowed for:

* Admin
* CompanyAdmin
* Agent

## Frontend Requirements

* Gallery component.
* Upload component.
* Multiple file input.
* Accept `.jpg,.jpeg,.png,.webp`.
* Max 10 total images message.
* Max 5MB per image message.
* Error handling for backend validation.

## Backend Requirements

N/A

## Validation Rules

* Client-side extension/type hint.
* Client-side 5MB size check where possible.
* Backend remains final validation.

## Business Rules

* Normal User can view images but not upload.
* Image upload should not break property details if no images exist.

## Files Likely To Change

* `frontend/src/pages/PropertyDetailsPage.tsx`
* `frontend/src/pages/EditPropertyPage.tsx`
* `frontend/src/components/properties/PropertyImageGallery.tsx`
* `frontend/src/components/properties/PropertyImageUpload.tsx`
* `frontend/src/services/api.ts`
* `frontend/src/types/files.ts`

## Acceptance Criteria

* Property details shows images.
* No images state is handled.
* Authorized users can upload images.
* Unauthorized users do not see upload UI.
* Build passes.

## Tests Required

* Manual UI verification.

## Manual Verification Steps

* Open property details without images.
* Login as Agent/Admin.
* Upload valid image.
* Confirm gallery updates.
* Login as User and confirm upload hidden.
* Run `npm run build`.

## Out Of Scope

* No drag-and-drop required.
* No image cropping/compression.
* No cloud storage.

## Dependencies

* Implement Property Image Retrieval Endpoint and Include Images in Property Details.
* Implement Backend Property Image Upload Endpoint.
* Implement Protected Routes and Role-Based Navigation.

## Blocks

N/A

## Notes For Codex

Keep UI simple. Preserve current property details layout and map/list features.

---

# Epic 7: Tests and Stabilization

## Ticket ES-140: Add Backend Authentication and Authorization Tests

## Epic

Tests and Stabilization

## Type

Tests

## Priority

Must-have

## Goal

Add focused backend tests for the authentication and authorization behavior introduced in Sprint 4. This protects the most critical security flows.

## Current Context

Backend tests already exist in `backend/EstateIQ.Tests` and currently pass. Existing command: `dotnet test backend\EstateIQ.Tests\EstateIQ.Tests.csproj`.

## Required Behavior

* Add tests for register.
* Add tests for email verification.
* Add tests for login before/after verification.
* Add tests for authorization on property write APIs.
* Preserve existing 59 passing tests.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

Test expected 401/403 behavior.

## Role / Permission Rules

Test role/permission access:

* User cannot create property.
* Agent/Admin/CompanyAdmin can create property where test setup allows.

## Frontend Requirements

N/A

## Backend Requirements

* Use existing test patterns.
* Seed required roles/permissions in test database.
* Generate valid JWTs through real login if possible.

## Validation Rules

N/A

## Business Rules

* Login only succeeds after email verification.
* Unverified users are blocked.
* Protected write APIs reject unauthorized users.

## Files Likely To Change

* `backend/EstateIQ.Tests/*Auth*Tests.cs`
* `backend/EstateIQ.Tests/*Authorization*Tests.cs`
* test fixtures/helpers

## Acceptance Criteria

* Register creates unverified user.
* Login fails if email is not verified.
* Verify email changes `IsEmailConfirmed`.
* Login succeeds after verification.
* Unauthorized user cannot create property.
* Role-based access works.
* All backend tests pass.

## Tests Required

This ticket itself is tests.

## Manual Verification Steps

* Run `dotnet test backend\EstateIQ.Tests\EstateIQ.Tests.csproj`.
* Run `dotnet build backend\EstateIQ\EstateIQ.csproj --configuration Release`.

## Out Of Scope

* No frontend tests unless existing setup already exists.
* No load/security penetration testing.

## Dependencies

* Implement Login Endpoint with JWT Access Token and Refresh Token.
* Configure JWT Authentication Middleware.
* Protect Property Create, Edit, and Delete APIs.

## Blocks

* Sprint 4 completion

## Notes For Codex

Add focused tests without rewriting existing test infrastructure. Keep tests deterministic.

---

## Ticket ES-141: Add Backend Property Image Upload Tests and Sprint 4 Final Verification

## Epic

Tests and Stabilization

## Type

Tests

## Priority

Must-have

## Goal

Validate image upload behavior and run final Sprint 4 stability checks. This ensures uploads are secure, constrained, and do not break existing property/map behavior.

## Current Context

Image upload backend exists, Files metadata exists, and frontend gallery/upload UI may exist. Existing backend and frontend builds passed before Sprint 4.

## Required Behavior

* Test valid image upload.
* Test invalid file type rejection.
* Test more than 10 images rejection.
* Test missing property rejection.
* Test unauthorized upload rejection.
* Run final backend tests and frontend build.

## API Contract

N/A

## Database Changes

N/A

## Authorization Rules

Upload requires `UploadPropertyImages`.

## Role / Permission Rules

Test authorized vs unauthorized upload.

## Frontend Requirements

* Run `npm run build`.

## Backend Requirements

* Use test file streams or mock `IFormFile`.
* Clean up test files after tests if writing to disk.
* Avoid polluting real `wwwroot/uploads` if possible.

## Validation Rules

* Max 10 images.
* Max 5MB per image.
* Allowed jpg/jpeg/png/webp.

## Business Rules

* Files metadata and disk write should stay consistent.

## Files Likely To Change

* `backend/EstateIQ.Tests/*PropertyImage*Tests.cs`
* backend test helpers
* optional test config for upload root

## Acceptance Criteria

* Valid image upload test passes.
* Invalid type test passes.
* More than 10 images rejected.
* Unauthorized upload rejected.
* All backend tests pass.
* Backend Release build passes.
* Frontend build passes.

## Tests Required

This ticket itself is tests.

## Manual Verification Steps

Run:

```bash
dotnet test backend\EstateIQ.Tests\EstateIQ.Tests.csproj
```

Run:

```bash
dotnet build backend\EstateIQ\EstateIQ.csproj --configuration Release
```

Run:

```bash
npm run build
```

Manual UI:

* Login as Agent/Admin.
* Upload image.
* View gallery.
* Login as User.
* Confirm upload UI hidden.

## Out Of Scope

* No cloud upload tests.
* No image optimization tests.

## Dependencies

* Implement Backend Property Image Upload Endpoint.
* Centralize Property Image Validation Logic.
* Implement Property Image Retrieval Endpoint and Include Images in Property Details.
* Implement Property Image Upload UI and Details Gallery.

## Blocks

* Sprint 4 completion

## Notes For Codex

Make file upload tests isolated and clean up generated files. Do not make tests depend on local machine paths.

---

# Recommended Implementation Order

1. Create Users Table with GUID Id
2. Create Roles and Permissions Tables with Seed Data
3. Create UserRoles and RolePermissions Join Tables
4. Create Refresh, Email Verification, and Password Reset Token Tables
5. Create Files Table for Property Image Metadata
6. Implement Auth Support Services for Password Hashing and Token Generation
7. Implement Public User Register Endpoint
8. Implement Email Verification Endpoint
9. Implement Login Endpoint with JWT Access Token and Refresh Token
10. Implement Refresh Token Endpoint
11. Implement Logout Endpoint with Refresh Token Revocation
12. Configure JWT Authentication Middleware
13. Add Role-Based Authorization Rules
14. Add Permission-Based Authorization Policies
15. Protect Property Create, Edit, and Delete APIs
16. Protect Company and Agent APIs
17. Implement Admin User List Endpoint
18. Implement Admin Create CompanyAdmin Endpoint
19. Implement CompanyAdmin/Admin Create Agent Endpoint
20. Implement Activate and Deactivate User Endpoint
21. Implement Backend Property Image Upload Endpoint
22. Centralize Property Image Validation Logic
23. Implement Property Image Retrieval Endpoint and Include Images in Property Details
24. Implement Delete Property Image Endpoint
25. Implement Public Register Page for User Accounts
26. Implement Login Page and Auth State Management
27. Implement Verify Email Page
28. Implement Protected Routes and Role-Based Navigation
29. Implement Property Image Upload UI and Details Gallery
30. Add Backend Authentication and Authorization Tests
31. Add Backend Property Image Upload Tests and Sprint 4 Final Verification

Note: The implementation order has 31 steps because the frontend image UI is separated before final tests, but the sprint ticket count remains 30 because the final verification ticket includes image tests and final checks.

---

# Critical Blockers / Dependencies

* Register cannot start before Users, Roles, UserRoles, and EmailVerificationTokens exist.
* Login cannot start before password/token services, register, and email verification exist.
* JWT middleware cannot be fully validated before login works.
* Property API protection cannot start before JWT middleware and policies exist.
* User management cannot start before roles/permissions and API protection exist.
* Image upload cannot start before Files table and property authorization exist.
* Frontend protected routes cannot start before login response and auth state exist.
* Image gallery UI cannot start before backend image retrieval/upload endpoints exist.
* Final tests should be done after backend and frontend work is complete.

---

# Must-Have Reduced Scope If Sprint Time Is Limited

If Sprint 4 becomes too large, keep only these Must-have items:

1. Auth database tables: Users, Roles, UserRoles, RefreshTokens, EmailVerificationTokens, Files.
2. Register User publicly.
3. Verify email with simulated token.
4. Login only after verification.
5. JWT access token.
6. Refresh token.
7. Role-based authorization for property create/edit/delete.
8. Basic Admin create CompanyAdmin.
9. Basic CompanyAdmin/Admin create Agent.
10. Property image upload and gallery.
11. Core backend tests.

Can simplify/skip temporarily:

* Password reset endpoints/UI.
* Delete image endpoint.
* Full permission UI.
* Detailed admin users page on frontend.
* Automatic refresh token handling on frontend.

---

# Move To Sprint 5 If Needed

Move these if time is limited:

* Real SMTP email sending.
* Resend verification email.
* Forgot password/reset password UI.
* Full Admin dashboard for user management.
* Full CompanyAdmin agent management page.
* Image ordering/cover image.
* AuditLogs.
* Notifications.
* Advanced ownership checks for every property operation.
* Dashboard analytics.

---

# Global Notes For Codex

* Follow Controller -> Service -> Repository.
* Use DTOs for all request/response contracts.
* Use existing custom exceptions: `ValidationException`, `NotFoundException`, `BusinessRuleException`.
* Keep changes scoped to the ticket being implemented.
* Preserve existing property list, filters, pagination, details, edit, delete, and map behavior.
* Do not refactor unrelated files.
* Add focused tests for new behavior.
* Run backend tests and builds after meaningful backend changes.
* Run frontend build after frontend changes.
