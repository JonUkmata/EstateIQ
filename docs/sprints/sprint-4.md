# Sprint 4 - Authentication, Authorization, And Property Images

## Sprint Goal

Add the security layer for EstateIQ by introducing user accounts, authentication, role and permission based authorization, user management workflows, and property image upload support.

## Sprint Status

**Status:** Completed  
**Primary Theme:** Authentication, roles, permissions, protected APIs, user management, property images  
**Result:** Users can register, verify email, login, access role-aware frontend routes, use protected property actions, and upload/view property images when authorized.

## What Was Delivered

### Auth Database Foundation

- Users table with GUID primary keys.
- Roles and permissions catalog with seeded values.
- User-role and role-permission assignment tables.
- Refresh token, email verification token, and password reset token tables.
- Files table for uploaded file metadata.
- Company-user relationship support for company administrators.
- Agent-user link support for agent accounts.

Seeded roles:

- `Admin`
- `CompanyAdmin`
- `Agent`
- `User`

Seeded permissions:

- `ManageUsers`
- `ManageCompanies`
- `ManageAgents`
- `CreateProperty`
- `EditProperty`
- `DeleteProperty`
- `UploadPropertyImages`
- `ViewProperties`
- `BookViewing`

### Authentication Core

- `POST /api/auth/register`
- `POST /api/auth/verify-email`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

Authentication behavior:

- Public users register with the `User` role.
- New accounts must verify email before login.
- Passwords are stored as hashes.
- Login returns JWT access token and authenticated user details.
- Refresh tokens are generated, hashed before storage, and revocable.
- Logout revokes the refresh token and clears the refresh token cookie.

### Authorization And API Protection

- JWT authentication middleware configured.
- Role claim and permission claim support added to tokens.
- Permission-based authorization policies configured.
- Property write APIs are protected:
  - `POST /api/properties`
  - `PUT /api/properties/{id}`
  - `DELETE /api/properties/{id}`
- Property image upload/delete APIs are protected.
- Company and agent APIs are protected for management use.
- Public property browsing, property details, and map flows remain available.

### User Management

- `GET /api/users`
- `POST /api/users/company-admins`
- `POST /api/users/agents`
- `PATCH /api/users/{id}/status`

User management behavior:

- Admin users can list users.
- Admin users can create company administrator accounts.
- Admin and CompanyAdmin users can create agent accounts.
- CompanyAdmin users can create agents only for their own company.
- User activation/deactivation is supported.
- Deactivated users cannot login or refresh sessions.
- Existing active sessions are revoked when a user is deactivated.

### Property Image Upload

- `POST /api/properties/{id}/images`
- `GET /api/properties/{id}/images`
- `DELETE /api/properties/{id}/images/{imageId}`

Image behavior:

- Property images are stored on local disk.
- File metadata is stored in the `Files` table.
- Property details include image data.
- Image gallery can be loaded for each property.
- Authorized users can upload images.
- Authorized users can delete images.
- Normal users can view property images but cannot upload/delete them.

Validation rules:

- Allowed image types: `.jpg`, `.jpeg`, `.png`, `.webp`
- Maximum size: 5 MB per image
- Maximum count: 10 images per property
- Upload rejects missing properties, invalid file types, oversized files, and too many images.

### Frontend Authentication Flow

- `/register` page.
- `/login` page.
- `/verify-email` page.
- Auth context for storing logged-in user state.
- API helper attaches bearer tokens to protected requests.
- Logout clears frontend auth state.
- Verification page supports query string token and manual token entry.

### Frontend Protected UI

- Protected route wrapper.
- Role and permission constants.
- Role-aware navigation.
- Login/logout state shown in the layout.
- Create, edit, delete, and image upload actions are hidden when the user lacks permission.
- Public browsing routes remain accessible:
  - `/properties`
  - `/properties/:id`
  - `/map`

### Frontend Property Image UI

- Property details page shows image gallery.
- Empty image state is handled.
- Upload UI appears only for users with `UploadPropertyImages`.
- Gallery refreshes after upload.
- Client-side file type, size, and count checks added before upload.

## Key Files

### Backend

- `/backend/EstateIQ/Controllers/AuthController.cs`
- `/backend/EstateIQ/Controllers/UsersController.cs`
- `/backend/EstateIQ/Controllers/PropertiesController.cs`
- `/backend/EstateIQ/Models/User.cs`
- `/backend/EstateIQ/Models/Role.cs`
- `/backend/EstateIQ/Models/Permission.cs`
- `/backend/EstateIQ/Models/UserRole.cs`
- `/backend/EstateIQ/Models/RolePermission.cs`
- `/backend/EstateIQ/Models/RefreshToken.cs`
- `/backend/EstateIQ/Models/EmailVerificationToken.cs`
- `/backend/EstateIQ/Models/PasswordResetToken.cs`
- `/backend/EstateIQ/Models/FileRecord.cs`
- `/backend/EstateIQ/Models/CompanyUser.cs`
- `/backend/EstateIQ/Repositories/AuthRepository.cs`
- `/backend/EstateIQ/Repositories/UserRepository.cs`
- `/backend/EstateIQ/Repositories/FileRepository.cs`
- `/backend/EstateIQ/Services/Auth/AuthService.cs`
- `/backend/EstateIQ/Services/Auth/PasswordService.cs`
- `/backend/EstateIQ/Services/Auth/TokenService.cs`
- `/backend/EstateIQ/Services/UserService.cs`
- `/backend/EstateIQ/Services/Files/FileValidationService.cs`
- `/backend/EstateIQ/Services/Files/PropertyImageService.cs`
- `/backend/EstateIQ/Constants/Roles.cs`
- `/backend/EstateIQ/Constants/Permissions.cs`
- `/backend/EstateIQ/Constants/AuthorizationPolicies.cs`
- `/backend/EstateIQ/Constants/FileUploadConstants.cs`
- `/backend/EstateIQ/Data/AppDbContext.cs`
- `/backend/EstateIQ/Program.cs`

### Frontend

- `/frontend/src/context/AuthContext.tsx`
- `/frontend/src/components/ProtectedRoute.tsx`
- `/frontend/src/components/properties/PropertyImageGallery.tsx`
- `/frontend/src/components/properties/PropertyImageUpload.tsx`
- `/frontend/src/pages/RegisterPage.tsx`
- `/frontend/src/pages/LoginPage.tsx`
- `/frontend/src/pages/VerifyEmailPage.tsx`
- `/frontend/src/pages/PropertyDetailsPage.tsx`
- `/frontend/src/pages/PropertiesPage.tsx`
- `/frontend/src/routes/AppRouter.tsx`
- `/frontend/src/services/api.ts`
- `/frontend/src/types/auth.ts`
- `/frontend/src/types/files.ts`
- `/frontend/src/constants/auth.ts`
- `/frontend/src/styles.css`

### Tests

- `/backend/EstateIQ.Tests/AppDbContextUserTests.cs`
- `/backend/EstateIQ.Tests/AppDbContextAuthorizationCatalogTests.cs`
- `/backend/EstateIQ.Tests/AppDbContextRoleAssignmentTests.cs`
- `/backend/EstateIQ.Tests/AppDbContextTokenTests.cs`
- `/backend/EstateIQ.Tests/AppDbContextFileRecordTests.cs`
- `/backend/EstateIQ.Tests/AuthSupportServiceTests.cs`
- `/backend/EstateIQ.Tests/AuthRegisterServiceTests.cs`
- `/backend/EstateIQ.Tests/AuthVerifyEmailServiceTests.cs`
- `/backend/EstateIQ.Tests/AuthLoginServiceTests.cs`
- `/backend/EstateIQ.Tests/AuthRefreshServiceTests.cs`
- `/backend/EstateIQ.Tests/AuthLogoutServiceTests.cs`
- `/backend/EstateIQ.Tests/AuthControllerTests.cs`
- `/backend/EstateIQ.Tests/JwtAuthenticationMiddlewareTests.cs`
- `/backend/EstateIQ.Tests/PermissionAuthorizationPolicyTests.cs`
- `/backend/EstateIQ.Tests/AuthorizationRulesTests.cs`
- `/backend/EstateIQ.Tests/CompanyAgentAuthorizationTests.cs`
- `/backend/EstateIQ.Tests/UsersControllerTests.cs`
- `/backend/EstateIQ.Tests/FileValidationServiceTests.cs`
- `/backend/EstateIQ.Tests/PropertiesControllerTests.cs`

## Verification

Current verification status:

```text
dotnet test backend\EstateIQ.Tests\EstateIQ.Tests.csproj
Result: 166/166 passing

dotnet build backend\EstateIQ\EstateIQ.csproj --configuration Release
Result: passed, 0 warnings, 0 errors

npm run build
Result: passed
```

Manual verification scope:

- Public user registration creates an unverified account.
- Email verification enables login.
- Login returns user data, roles, permissions, access token, and refresh token.
- Refresh token flow issues a new access token.
- Logout revokes the refresh token.
- Protected property create/edit/delete endpoints reject unauthenticated users.
- Authorized roles can perform property write actions.
- Admin can list users and create CompanyAdmin accounts.
- Admin/CompanyAdmin can create agent accounts.
- CompanyAdmin cannot create agents for another company.
- Deactivated users cannot login or refresh sessions.
- Property image upload accepts valid images and rejects invalid files.
- Property image gallery loads on property details.
- Upload UI is hidden for users without permission.
- Public list, details, filters, pagination, and map behavior remain available.

## Known Gaps After Sprint 4

- No real SMTP email sending yet; verification uses a simulated token flow.
- No forgot password or reset password UI yet.
- No full admin dashboard for user management yet.
- No dedicated company/agent management frontend pages yet.
- No cloud file storage yet; images are stored locally.
- No image ordering, cover image, cropping, or optimization yet.
- Frontend verification is still manual/build-based; there is no dedicated automated frontend test suite yet.

## Sprint 5 Planning Handoff

Recommended Sprint 5 theme:

**Admin Operations, Account Recovery, And Production Readiness**

Recommended next tickets:

1. Add real email sending for verification and account recovery.
2. Add resend verification email flow.
3. Implement forgot password and reset password endpoints/UI.
4. Build admin user management frontend pages.
5. Build company and agent management frontend pages.
6. Add image ordering and cover image support.
7. Add dashboard analytics backed by real API metrics.
8. Harden deployment configuration for production.
9. Add frontend automated tests for auth, protected routes, and image upload flows.
