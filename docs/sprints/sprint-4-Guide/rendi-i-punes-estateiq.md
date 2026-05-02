# Rendi i punes per EstateIQ

Ky eshte rendi i thjeshte i punes per 31 ticket-at e Sprint 4 qe jane krijuar dhe assign ne Jira.

## Ndarja e pergjithshme

| Personi | Nr. tickets |
|---|---:|
| Jon / jondukmata@gmail.com | 12 |
| Valeza Dobruna | 11 |
| Diola Sokoli | 8 |

## Epics

- ES-104: Auth Database Foundation
- ES-105: Authentication Core
- ES-106: Roles, Permissions, and API Protection
- ES-107: User Management
- ES-108: Property Image Upload
- ES-109: Frontend Auth and Protected UI
- ES-110: Tests and Stabilization

## Rendi i punes

### 1. Fillimi: baza e databazes dhe autentikimit

Ne fillim punohet pjesa baze, sepse ticket-at tjere varen prej saj.

**Jon**
- ES-111: Create Users Table with GUID Id
- ES-112: Create Roles and Permissions Tables with Seed Data
- ES-113: Create UserRoles and RolePermissions Join Tables
- ES-114: Create Refresh, Email Verification, and Password Reset Token Tables
- ES-116: Implement Auth Support Services for Password Hashing and Token Generation

**Valeza**
- ES-115: Create Files Table for Property Image Metadata
- ES-117: Implement Public User Register Endpoint
- ES-118: Implement Email Verification Endpoint

Qellimi i kesaj faze eshte qe backend-i dhe struktura baze per autentikim te jene gati para se te vazhdohet me role, usera dhe frontend.

### 2. Login, roles, permissions dhe mbrojtja e API-ve

Pasi autentikimi baze eshte gati, vazhdohet me login, tokena dhe kontrollin e qasjes.

**Jon**
- ES-119: Implement Login Endpoint with JWT Access Token and Refresh Token
- ES-122: Add Role-Based Authorization Rules
- ES-123: Configure JWT Authentication Middleware
- ES-124: Add Permission-Based Authorization Policies
- ES-125: Protect Property Create, Edit, and Delete APIs

**Valeza**
- ES-120: Implement Refresh Token Endpoint
- ES-121: Implement Logout Endpoint with Refresh Token Revocation
- ES-126: Protect Company and Agent APIs

Kjo faze duhet te perfundoje para se te lidhen mire pjeset e user management dhe frontend protected UI.

### 3. User management

Ketu punohet menaxhimi i perdoruesve.

**Valeza**
- ES-127: Implement Admin User List Endpoint
- ES-128: Implement Admin Create CompanyAdmin Endpoint

**Diola**
- ES-129: Implement CompanyAdmin/Admin Create Agent Endpoint
- ES-130: Implement Activate and Deactivate User Endpoint


### 4. Property image upload dhe frontend auth

Pasi backend-i kryesor dhe user management jane ne rregull, vazhdohet me upload dhe pjesen frontend.

**Jon**
- ES-131: Implement Backend Property Image Upload Endpoint

**Valeza**
- ES-132: Centralize Property Image Validation Logic
- ES-133: Implement Property Image Retrieval Endpoint and Include Images in Property Details

**Diola**
- ES-134: Implement Delete Property Image Endpoint
- ES-135: Implement Public Register Page for User Accounts
- ES-136: Implement Login Page and Auth State Management
- ES-137: Implement Verify Email Page
- ES-138: Implement Protected Routes and Role-Based Navigation
- ES-139: Implement Property Image Upload UI and Details Gallery

Diola ketu merr me shume ticket-a, por shumica jane te ndara ne hapa te qarte dhe mund te punohen me radhe.

### 5. Testim dhe stabilizim

Ne fund punohet testimi, pastrimi dhe stabilizimi.

**Valeza**
- ES-140: Add Backend Authentication and Authorization Tests

**Jon**
- ES-141: Add Backend Property Image Upload Tests and Sprint 4 Final Verification

Kjo faze duhet te behet ne fund, pasi shumica e funksionaliteteve jane implementuar.

## Rendi praktik per ekipin

1. Jon fillon me bazen e DB/auth: ES-111, ES-112, ES-113, ES-114, ES-116.
2. Valeza fillon paralelisht me ES-115, ES-117, ES-118.
3. Pastaj punohet login, refresh/logout dhe JWT/policies: ES-119 deri ES-126.
4. User management vazhdon me ES-127 deri ES-130.
5. Image upload dhe frontend vazhdojne me ES-131 deri ES-139.
6. ES-140 dhe ES-141 lihen per fund, sepse jane testim dhe final verification.

## Shperndarja finale

### Jon / jondukmata@gmail.com

- ES-111: Create Users Table with GUID Id
- ES-112: Create Roles and Permissions Tables with Seed Data
- ES-113: Create UserRoles and RolePermissions Join Tables
- ES-114: Create Refresh, Email Verification, and Password Reset Token Tables
- ES-116: Implement Auth Support Services for Password Hashing and Token Generation
- ES-119: Implement Login Endpoint with JWT Access Token and Refresh Token
- ES-122: Add Role-Based Authorization Rules
- ES-123: Configure JWT Authentication Middleware
- ES-124: Add Permission-Based Authorization Policies
- ES-125: Protect Property Create, Edit, and Delete APIs
- ES-131: Implement Backend Property Image Upload Endpoint
- ES-141: Add Backend Property Image Upload Tests and Sprint 4 Final Verification

### Valeza Dobruna

- ES-115: Create Files Table for Property Image Metadata
- ES-117: Implement Public User Register Endpoint
- ES-118: Implement Email Verification Endpoint
- ES-120: Implement Refresh Token Endpoint
- ES-121: Implement Logout Endpoint with Refresh Token Revocation
- ES-126: Protect Company and Agent APIs
- ES-127: Implement Admin User List Endpoint
- ES-128: Implement Admin Create CompanyAdmin Endpoint
- ES-132: Centralize Property Image Validation Logic
- ES-133: Implement Property Image Retrieval Endpoint and Include Images in Property Details
- ES-140: Add Backend Authentication and Authorization Tests

### Diola Sokoli

- ES-129: Implement CompanyAdmin/Admin Create Agent Endpoint
- ES-130: Implement Activate and Deactivate User Endpoint
- ES-134: Implement Delete Property Image Endpoint
- ES-135: Implement Public Register Page for User Accounts
- ES-136: Implement Login Page and Auth State Management
- ES-137: Implement Verify Email Page
- ES-138: Implement Protected Routes and Role-Based Navigation
- ES-139: Implement Property Image Upload UI and Details Gallery
