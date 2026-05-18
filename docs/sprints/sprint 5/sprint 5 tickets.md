# EstateIQ — Sprint 5 Tickets

## Sprint 5 Goal

Në Sprint 5, EstateIQ do të ndajë eksperiencën e frontend-it sipas roleve. Useri normal do ta përdorë platformën si marketplace për kërkim të pronave, me property cards dhe full map search. CompanyAdmin do të ketë UI për të krijuar dhe menaxhuar agjentët e kompanisë së vet. Admin do të ketë UI për të krijuar dhe menaxhuar CompanyAdmin-at. Gjithashtu dashboard-i do të fillojë të ketë statistika reale të ndara sipas roleve, me Redis caching dhe cache invalidation për të shmangur inkonsistencën.

---

# ES-142 — Improve Unauthenticated Landing Flow

**Labels:** `auth-ui`, `routing`, `frontend`
**Priority:** High

## Description

Aktualisht kur hapet aplikacioni, shfaqet homepage teknike/smoke test dhe useri duhet të klikojë login/register për të testuar flow-in. Kjo nuk duket si produkt real.

Në këtë ticket duhet të ndryshohet entry flow i aplikacionit. Kur useri hap `/`, nëse nuk është authenticated, duhet të shfaqet një auth landing page më profesionale me opsione për Login dhe Register.

Nëse useri është authenticated, `/` duhet ta dërgojë në faqe të përshtatshme sipas rolit.

## Expected Behavior

```text
Not logged in user -> /
Shows auth landing / login-register entry

Logged in Admin -> /
Redirects to /dashboard

Logged in CompanyAdmin -> /
Redirects to /dashboard

Logged in Agent -> /
Redirects to /dashboard

Logged in User -> /
Redirects to /properties
```

## Acceptance Criteria

* Homepage teknike nuk shfaqet më si default për userat normalë.
* `/` vendos redirect ose render sipas auth state.
* Login dhe Register janë të dukshme menjëherë për userat që nuk janë të kyçur.
* Auth state kontrollohet përpara redirect-it.
* Nuk krijohet redirect loop.
* Existing login/register functionality nuk prishet.
* Frontend build kalon.

## Technical Notes

* Përdor AuthContext ekzistues.
* Nëse ka loading state për auth initialization, shfaq një loading screen të thjeshtë.
* Homepage e vjetër mund të ruhet si `/dev/health` ose `/system/status`, nëse ende nevojitet për smoke testing.

## Dependencies

* AuthContext ekzistues.
* React Router ekzistues.

## Risk Notes

* Kujdes me redirect loop kur token është duke u lexuar/validuar.
* Mos e hiq komplet health page nëse përdoret për debugging; më mirë zhvendose në route tjetër.

---

# ES-143 — Redesign Login Page UI

**Labels:** `auth-ui`, `frontend`
**Priority:** High

## Description

Login page aktualisht funksionon, por është shumë basic. Duhet të ridizajnohet që të duket më profesionale dhe më e kuptueshme për userin.

Faqja duhet të ketë layout të pastër, formë në qendër, titull të qartë, error messages të kuptueshme dhe link për register.

## Acceptance Criteria

* Login page ka layout modern dhe të pastër.
* Forma është e qartë: email, password, submit.
* Error messages shfaqen qartë.
* Loading state shfaqet gjatë login.
* Ka link drejt Register page.
* Nëse login është i suksesshëm, useri ridrejtohet në faqe sipas rolit.
* Nëse useri është unverified, mesazhi duhet të jetë i kuptueshëm.
* Existing API call për login nuk ndryshohet pa nevojë.
* Frontend build kalon.

## Technical Notes

* Mos ndrysho auth contract nëse nuk është e nevojshme.
* Përdor styling konsistent me pjesën tjetër të app-it.
* Mos përdor emoji në code/UI labels.

## Dependencies

* Existing login endpoint.
* AuthContext.

## Risk Notes

* Mos e prish ruajtjen e access token/user info.
* Kontrollo redirect pas login për secilin rol.

---

# ES-144 — Redesign Register and Verify Email UI

**Labels:** `auth-ui`, `frontend`
**Priority:** Medium

## Description

Register dhe Verify Email pages duhet të bëhen më të qarta për demo flow. Aktualisht register kthen simulated verification token dhe verify page lejon query token ose manual paste. Kjo është në rregull për projekt, por UI duhet ta shpjegojë më mirë.

## Acceptance Criteria

* Register page ka formë të pastër dhe të kuptueshme.
* Pas register, useri kupton që duhet të bëjë email verification.
* Simulated verification token shfaqet qartë për demo/testing.
* Verify Email page pranon token nga query string ose manual input.
* Pas verification të suksesshëm, useri mund të shkojë te login.
* Error/loading states janë të qarta.
* Existing register/verify flow nuk prishet.
* Frontend build kalon.

## Technical Notes

* Mbaje simulated verification flow sepse nuk kemi email real.
* Mos implemento SMTP në këtë ticket.
* Teksti duhet ta bëjë të qartë që verification është demo/simulated.

## Dependencies

* `POST /api/auth/register`
* `POST /api/auth/verify-email`

## Risk Notes

* Mos premto email real në UI.
* Mos e paraqit token-in si feature production; paraqite si demo/testing flow.

---

# ES-145 — Implement Role-Based Navigation Cleanup

**Labels:** `role-based-ui`, `navigation`, `frontend`
**Priority:** High

## Description

Navigation duhet të ndahet sipas roleve. Aktualisht app-i ka disa links/actions që mund të duken të njëjta për të gjithë userat. Në Sprint 5 duhet që User, Agent, CompanyAdmin dhe Admin të kenë navigation më të qartë dhe të përshtatur me rolin e tyre.

## Role-Based Navigation Proposal

```text
Logged out:
Login
Register
```

```text
User:
Properties
Map Search
Dashboard
Logout
```

```text
Agent:
Dashboard
Properties
Map Search
My Properties
Logout
```

```text
CompanyAdmin:
Dashboard
Properties
Map Search
Company Agents
Logout
```

```text
Admin:
Dashboard
Properties
Map Search
Admin Users
Logout
```

## Acceptance Criteria

* Logged out users shohin vetëm auth-related links.
* User nuk sheh admin/company/agent management links.
* CompanyAdmin sheh link për menaxhim të agjentëve.
* Admin sheh link për user/company admin management.
* Agent nuk sheh Admin/CompanyAdmin management pages.
* Links administrative janë të mbrojtura edhe me ProtectedRoute.
* Backend authorization mbetet source of truth.
* Frontend build kalon.

## Technical Notes

* Përdor role/permissions nga AuthContext.
* Prefero helper functions si `hasRole()` ose `hasPermission()`.
* Mos u mbështet vetëm në fshehje të butonave; routes duhet të jenë protected.

## Dependencies

* AuthContext.
* Protected routes ekzistuese.
* Roles/permissions në user info.

## Risk Notes

* Fshehja në frontend nuk është security e mjaftueshme. Backend duhet të vazhdojë të refuzojë unauthorized requests.

---

# ES-146 — Convert Properties Page to Marketplace Card Grid

**Labels:** `marketplace`, `properties-ui`, `frontend`
**Priority:** High

## Description

Properties page aktualisht është funksionale, por duket më shumë si admin list/table. Për userin normal duhet të duket si marketplace real estate browsing experience.

Në këtë ticket duhet të konvertohet properties list në card grid modern.

## Card Requirements

Çdo property card duhet të shfaqë:

* cover image ose placeholder
* price
* title ose type + city
* city/address
* property type
* property status badge
* area/bedrooms/bathrooms nëse ekzistojnë në model
* short description
* button për details
* edit/delete vetëm për authorized roles

## Acceptance Criteria

* `/properties` shfaq properties si responsive card grid.
* User normal sheh vetëm browsing actions.
* Admin/Agent/CompanyAdmin shohin management actions vetëm nëse kanë permission.
* Nëse property ka images, card përdor image kryesore.
* Nëse property nuk ka images, shfaqet placeholder.
* Search, filters dhe pagination vazhdojnë të punojnë.
* Cards janë responsive për desktop dhe mobile.
* Frontend build kalon.

## Technical Notes

* Mund të përdoret image e parë si cover image për këtë sprint.
* Mos implemento image ordering në këtë ticket.
* Kujdes me null values për images/specs.
* Nëse backend nuk kthen imageUrl në property list, përdor existing image endpoint ose shto minimal DTO support në backend vetëm nëse duhet.

## Dependencies

* Existing `GET /api/properties`.
* Existing property images endpoints.
* Existing role/permission checks në frontend.

## Risk Notes

* Kujdes që të mos prishet pagination dhe filtering ekzistues.
* Nëse secila card thërret veçmas images endpoint, mund të bëhen shumë API calls. Më mirë të kthehet cover image në list response nëse është praktike.

---

# ES-147 — Separate Create Property Flow from Marketplace Page

**Labels:** `properties-ui`, `role-based-ui`, `frontend`
**Priority:** Medium

## Description

Aktualisht create property form është në të njëjtën page me properties list, gjë që e bën browsing experience të rëndë. Për Sprint 5, create flow duhet të ndahet nga marketplace view.

Krijo route të veçantë:

```text
/properties/new
```

Kjo faqe duhet të jetë e aksesueshme vetëm për userat me permission për `CreateProperty`.

## Acceptance Criteria

* `/properties` përdoret për browsing/listing.
* `/properties/new` përdoret për krijim prone.
* Butoni “Create Property” shfaqet vetëm për userat me permission.
* User normal nuk mund të hapë create form.
* Pas krijimit të suksesshëm, useri ridrejtohet te details page ose properties list.
* Existing validation ruhet.
* Existing create property API call vazhdon të punojë.
* Frontend build kalon.

## Technical Notes

* Mund të ripërdoret forma ekzistuese.
* Mos bëj redesign të madh të formës në këtë ticket.
* ProtectedRoute duhet të kontrollojë permission/role.

## Dependencies

* Existing create property form.
* Existing `POST /api/properties`.
* Permission `CreateProperty`.

## Risk Notes

* Mos e dubliko logjikën e formës pa nevojë. Nëse është e mundur, nxirre formën si reusable component.

---

# ES-148 — Improve Full Map Search Page for Users

**Labels:** `map-search`, `marketplace`, `frontend`
**Priority:** High

## Description

Useri duhet të ketë një faqe ku mund të kërkojë prona përmes hartës. Map page ekziston, por duhet të përmirësohet që të ndihet si map search experience, jo vetëm si technical map.

Faqja `/map` duhet të shfaqë hartën si pjesë kryesore të kërkimit të pronave.

## Acceptance Criteria

* `/map` shfaq hartë të madhe me markers për pronat që kanë coordinates.
* Map page përdor filters/search të ngjashme me properties page.
* Marker popup shfaq mini property card me image, price, city dhe details link.
* Ka listë anësore ose poshtë hartës me property cards.
* Klikimi në property card fokusohet te marker në hartë, nëse është praktike.
* Klikimi në marker/popup lejon hapjen e details page.
* User normal mund ta përdorë map page si browsing experience.
* Frontend build kalon.

## Technical Notes

* Përdor Leaflet/React Leaflet ekzistues.
* Mos implemento radius search, clustering ose draw area në këtë sprint.
* Mos ndrysho shumë backend-in nëse filter endpoint ekzistues mjafton.
* Kujdes me properties pa latitude/longitude; ato nuk duhet ta prishin hartën.

## Dependencies

* Existing `/map`.
* Existing `GET /api/properties`.
* Existing latitude/longitude fields.

## Risk Notes

* Leaflet layout mund të thyhet nëse container height nuk është i definuar mirë.
* Kujdes me performance nëse ka shumë markers.

---

# ES-149 — Connect Marketplace List View and Map Search View

**Labels:** `marketplace`, `map-search`, `frontend`
**Priority:** Medium

## Description

Marketplace view dhe map search view duhet të duken të lidhura. Useri duhet të kalojë lehtë nga `/properties` te `/map` dhe anasjelltas.

## Acceptance Criteria

* Në `/properties` ka button/link “View on Map”.
* Në `/map` ka button/link “View as List”.
* Nëse është praktike, query params bazike ruhen gjatë kalimit, si `city`, `propertyTypeId`, `minPrice`, `maxPrice`.
* Useri nuk humb komplet kontekstin kur kalon nga list view në map view.
* Links janë të dukshme dhe të qarta.
* Frontend build kalon.

## Technical Notes

* Mos bëj state sharing kompleks në këtë sprint.
* Query params janë mjaftueshëm për fillim.
* Nëse query sync bëhet shumë e madhe, implemento vetëm navigation links dhe lëre query sync si improvement.

## Dependencies

* ES-146.
* ES-148.

## Risk Notes

* Mos e bëj këtë ticket më të madh se duhet. Qëllimi është lidhje bazike mes dy views.

---

# ES-150 — Build CompanyAdmin Agents Management Page

**Labels:** `company-admin`, `user-management-ui`, `frontend`
**Priority:** High

## Description

CompanyAdmin duhet të ketë UI ku mund të shohë agjentët e kompanisë së vet. Backend tashmë ka user management dhe role/permission logic. Në këtë ticket krijohet faqja e menaxhimit të agjentëve për CompanyAdmin.

Route e propozuar:

```text
/company/agents
```

## Acceptance Criteria

* CompanyAdmin mund të hapë `/company/agents`.
* User normal nuk mund ta hapë këtë faqe.
* Agent nuk mund ta hapë këtë faqe.
* Admin mund ta hapë vetëm nëse vendosim ta lejojmë, por fokusi është CompanyAdmin.
* Faqja shfaq listën e agjentëve të kompanisë së vet.
* Shfaqen të dhëna bazike: first name, last name, email, status, created date nëse ekziston.
* Ka loading, error dhe empty state.
* Frontend build kalon.

## Technical Notes

* Përdor endpoint ekzistues nëse `GET /api/users` mund të filtrojë sipas role/company.
* Nëse backend nuk ka endpoint të përshtatshëm për “agents of my company”, mund të shtohet endpoint minimal si:

```http
GET /api/users/agents/my-company
```

ose të zgjerohet existing users endpoint me filter.

## Dependencies

* Auth/roles.
* Existing user management backend.
* CompanyUsers / company relationship.

## Risk Notes

* CompanyAdmin nuk duhet të shohë agjentët e kompanive të tjera.
* Ownership/scope duhet të kontrollohet në backend, jo vetëm në frontend.

---

# ES-151 — Add Create Agent Form for CompanyAdmin

**Labels:** `company-admin`, `user-management-ui`, `frontend`
**Priority:** High

## Description

CompanyAdmin duhet të mund të krijojë agjentë për kompaninë e vet nga UI. Backend tashmë ka endpoint:

```http
POST /api/users/agents
```

Në UI duhet të krijohet formë për shtim të agjentit.

## Form Fields

* First name
* Last name
* Email
* Temporary password ose password field, nëse backend e kërkon
* Active status nëse backend e mbështet

CompanyAdmin nuk duhet të zgjedhë company manualisht. Company duhet të merret nga context/backend sipas userit të kyçur.

## Acceptance Criteria

* CompanyAdmin mund të krijojë Agent nga `/company/agents`.
* CompanyAdmin nuk zgjedh kompani tjetër.
* Pas krijimit, agenti shfaqet në listë.
* Form validation ekziston për required fields dhe email.
* Error message shfaqet nëse backend refuzon request.
* Success message shfaqet pas krijimit.
* User normal nuk sheh formën.
* Frontend build kalon.

## Technical Notes

* Mos ekspozo field për role; role duhet të jetë Agent nga backend.
* Nëse backend kërkon companyId, sigurohu që CompanyAdmin nuk mund të manipulojë companyId për kompani tjetër.
* Prefero backend ta nxjerrë companyId nga logged-in user kur është CompanyAdmin.

## Dependencies

* ES-150.
* `POST /api/users/agents`.

## Risk Notes

* Rreziku kryesor është privilege escalation. CompanyAdmin nuk duhet të krijojë agent për kompani tjetër.

---

# ES-152 — Build Admin Users and CompanyAdmins Management Page

**Labels:** `admin`, `user-management-ui`, `frontend`
**Priority:** High

## Description

Admin duhet të ketë UI për menaxhim të users, sidomos CompanyAdmin-at. Krijo faqe administrative ku Admin sheh users dhe mund të fillojë menaxhimin bazik.

Route e propozuar:

```text
/admin/users
```

## Acceptance Criteria

* Vetëm Admin mund të hapë `/admin/users`.
* Faqja shfaq listë të users.
* Shfaqen të dhëna bazike: name, email, role, status, company nëse ekziston.
* Ka filter bazik sipas role ose search sipas email/name nëse është praktike.
* Ka loading, error dhe empty state.
* User normal, Agent dhe CompanyAdmin nuk mund të hapin këtë faqe.
* Frontend build kalon.

## Technical Notes

* Përdor `GET /api/users`.
* Nëse endpoint nuk kthen role/company info të mjaftueshme, shto ose përdor DTO të përshtatshëm.
* Mos bëj full admin panel në këtë ticket. Fokusi është listim dhe bazë për krijim CompanyAdmin.

## Dependencies

* Existing `GET /api/users`.
* Admin authorization.

## Risk Notes

* Mos shfaq të dhëna sensitive si password hash, token data, etj.
* Backend duhet ta mbrojë endpoint-in me Admin role/permission.

---

# ES-153 — Add Create CompanyAdmin Form for Admin

**Labels:** `admin`, `user-management-ui`, `frontend`
**Priority:** High

## Description

Admin duhet të mund të krijojë CompanyAdmin nga UI. Backend tashmë ka endpoint:

```http
POST /api/users/company-admins
```

Në këtë ticket shtohet formë në `/admin/users` ose në route të veçantë për krijimin e CompanyAdmin.

## Form Fields

* First name
* Last name
* Email
* Password ose temporary password
* Company dropdown
* Active status nëse backend e mbështet

## Acceptance Criteria

* Admin mund të krijojë CompanyAdmin.
* Admin zgjedh company nga dropdown.
* Company dropdown mbushet nga API.
* Pas krijimit, CompanyAdmin shfaqet në listë.
* Form validation ekziston.
* Error/success states janë të qarta.
* Vetëm Admin e sheh dhe e përdor këtë formë.
* Frontend build kalon.

## Technical Notes

* Përdor `GET /api/companies` për dropdown.
* Mos lejo krijim CompanyAdmin pa company.
* Nëse password është temporary, UI duhet ta tregojë qartë.
* Mos implemento email sending në këtë ticket.

## Dependencies

* ES-152.
* `POST /api/users/company-admins`.
* `GET /api/companies`.

## Risk Notes

* CompanyAdmin duhet të lidhet saktë me company.
* Mos lejo që CompanyAdmin të krijohet pa role ose pa company relationship.

---

# ES-154 — Add Activate and Deactivate User UI

**Labels:** `admin`, `company-admin`, `user-management-ui`, `frontend`
**Priority:** Medium

## Description

Backend tashmë mbështet aktivizim/deaktivizim useri dhe revokim sessions kur useri deactivated. Në UI duhet të shtohet mundësia që Admin ose CompanyAdmin, sipas autorizimit, ta aktivizojë/deaktivizojë userin.

Endpoint ekzistues:

```http
PATCH /api/users/{id}/status
```

## Acceptance Criteria

* Admin mund të aktivizojë/deaktivizojë users sipas rregullave të backend-it.
* CompanyAdmin mund të aktivizojë/deaktivizojë vetëm agjentët e kompanisë së vet, nëse backend e lejon.
* UI kërkon confirmation para deactivate.
* Statusi rifreskohet pas update.
* Error message shfaqet nëse backend e refuzon action.
* User nuk mund të deaktivizojë veten nëse kjo nuk lejohet nga backend.
* Frontend build kalon.

## Technical Notes

* Përdor existing PATCH endpoint.
* Mos implemento logic të ndërlikuar në frontend. Backend vendos çka lejohet.
* Button text duhet të ndryshojë sipas statusit: Activate / Deactivate.

## Dependencies

* ES-150.
* ES-152.
* `PATCH /api/users/{id}/status`.

## Risk Notes

* Kujdes të mos lejohet CompanyAdmin të deaktivizojë user jashtë kompanisë.
* Kujdes me self-deactivation të Admin.

---

# ES-155 — Implement Role-Based Dashboard Summary Endpoint

**Labels:** `dashboard`, `backend`, `role-based-ui`
**Priority:** High

## Description

Dashboard-i duhet të fillojë të ketë statistika reale sipas rolit. Krijo endpoint:

```http
GET /api/dashboard/me
```

Ky endpoint kthen dashboard summary të përshtatur sipas userit të kyçur.

## Role Behavior

Admin sheh statistika globale.

CompanyAdmin sheh statistika vetëm për kompaninë e vet.

Agent sheh statistika vetëm për pronat e veta.

User sheh statistika marketplace/browsing.

## Example Metrics

Admin:

* totalProperties
* availableProperties
* soldProperties
* rentedProperties
* totalUsers
* totalCompanies
* totalAgents
* recentProperties

CompanyAdmin:

* companyProperties
* companyAgents
* availableProperties
* soldProperties
* rentedProperties
* recentCompanyProperties

Agent:

* myProperties
* myAvailableProperties
* mySoldProperties
* myRentedProperties
* recentMyProperties

User:

* availableProperties
* latestProperties
* propertiesByType ose popularCities nëse është praktike

## Acceptance Criteria

* Endpoint është i mbrojtur me `[Authorize]`.
* Response ndryshon sipas rolit.
* Admin nuk kufizohet në company.
* CompanyAdmin sheh vetëm company-scoped statistics.
* Agent sheh vetëm agent-scoped statistics.
* User nuk sheh statistika administrative.
* Logjika nuk vendoset në controller.
* Ka DTO të veçanta për response.
* Backend tests shtohen për role kryesore.
* Backend tests kalojnë.

## Technical Notes

* Krijo `DashboardController`.
* Krijo `IDashboardService`.
* Krijo DTO si `DashboardSummaryDto`, `DashboardPropertyDto`.
* Nëse user ka më shumë role, vendos precedence të qartë:

```text
Admin > CompanyAdmin > Agent > User
```

## Dependencies

* Auth/role system.
* Properties/users/companies/agents data.
* CompanyUsers relationship.

## Risk Notes

* Scope leakage është rreziku më i madh. CompanyAdmin/Agent nuk duhet të shohin data jashtë scope-it.

---

# ES-156 — Implement Redis Caching for Role-Based Dashboard Statistics

**Labels:** `dashboard-redis`, `backend`, `redis`
**Priority:** High

## Description

Dashboard statistics duhet të cache-ohen në Redis për të treguar përdorim real të NoSQL/in-memory store dhe për të përmirësuar performancën. Cache duhet të ndahet sipas roleve dhe scope-it, që të mos ndodhin përzierje të statistikave.

Nuk duhet të përdoret një key e përgjithshme si:

```text
dashboard:me
```

## Redis Key Strategy

Admin:

```text
dashboard:admin:global
```

CompanyAdmin:

```text
dashboard:companyadmin:company:{companyId}
```

Agent:

```text
dashboard:agent:{agentId}
```

User marketplace:

```text
dashboard:user:marketplace
```

## Acceptance Criteria

* `GET /api/dashboard/me` kontrollon Redis para SQL.
* Në cache hit, response kthehet nga Redis.
* Në cache miss, statistikat llogariten nga SQL dhe ruhen në Redis.
* Cache key përfshin role dhe scope.
* Nuk ka cache key të përbashkët për të gjithë userat.
* Admin, CompanyAdmin, Agent dhe User kanë cache keys të ndara.
* TTL vendoset si backup, p.sh. 30 minuta.
* Serialization/deserialization punon saktë.
* Nëse Redis nuk është available, dashboard ende mund të kthejë data nga SQL ose të trajtojë gabimin në mënyrë të kontrolluar.
* Backend tests kalojnë.

## Technical Notes

* Krijo `IDashboardCacheService`.
* Redis cache service duhet të jetë reusable.
* Mos shpërnda Redis logic në controllers.
* TTL nuk duhet të jetë mekanizmi kryesor për freshness; invalidation përdoret për ndryshime.

## Dependencies

* ES-155.
* Redis configuration ekzistuese.

## Risk Notes

* Nëse key nuk ndahet mirë sipas rolit/scope-it, mund të shfaqen statistika të gabuara te useri i gabuar.
* Mos lejo që Redis failure ta rrëzojë gjithë dashboard-in nëse mund të bëhet fallback në SQL.

---

# ES-157 — Add Dashboard Cache Invalidation on Data Changes

**Labels:** `dashboard-redis`, `backend`, `redis`
**Priority:** High

## Description

Për të shmangur inkonsistencën, dashboard cache duhet të pastrohet menjëherë kur ndryshojnë të dhënat që ndikojnë në statistika.

Ky ticket shton cache invalidation në flows ku krijohen, editohen ose fshihen properties, dhe ku krijohen/deaktivizohen users/agents/company admins.

## Invalidation Rules

Kur krijohet/editohet/fshihet property:

```text
dashboard:admin:global
dashboard:companyadmin:company:{companyId}
dashboard:agent:{agentId}
dashboard:user:marketplace
```

Kur krijohet/deaktivizohet Agent:

```text
dashboard:admin:global
dashboard:companyadmin:company:{companyId}
```

Kur krijohet/deaktivizohet CompanyAdmin:

```text
dashboard:admin:global
dashboard:companyadmin:company:{companyId}
```

Kur krijohet/deaktivizohet User normal:

```text
dashboard:admin:global
```

## Acceptance Criteria

* Property create invalidon dashboard cache për scopes relevante.
* Property update invalidon dashboard cache për scopes relevante.
* Property delete invalidon dashboard cache për scopes relevante.
* Agent create/deactivate invalidon Admin dhe CompanyAdmin dashboard cache.
* CompanyAdmin create/deactivate invalidon Admin dashboard cache.
* Invalidation logic është në service layer, jo në controller.
* Nuk fshihen keys të panevojshme në mënyrë shumë agresive nëse mund të shmanget.
* Backend tests kalojnë.

## Technical Notes

Mund të krijohen methods:

```csharp
InvalidateDashboardsForPropertyChange(companyId, agentId)
InvalidateDashboardsForAgentChange(companyId)
InvalidateDashboardsForCompanyAdminChange(companyId)
```

* Nëse delete property ndodh, sigurohu që companyId/agentId të lexohen para delete.
* Për User marketplace cache, invalidimi bëhet kur ndryshojnë properties publike/available.
* Nëse property update ndryshon companyId ose agentId, invalidoni edhe old scope edhe new scope.

## Dependencies

* ES-156.
* Existing property services.
* Existing user management services.

## Risk Notes

* Invalidation e gabuar mund të lërë cache stale.
* Kujdes me update ku property ndryshon agent/company; mund të duhet invalidim për old dhe new scope.

---

# ES-158 — Build Role-Based Dashboard UI

**Labels:** `dashboard`, `role-based-ui`, `frontend`
**Priority:** High

## Description

Dashboard page ekziston, por është placeholder/basic. Në këtë ticket ndërtohet dashboard UI që përdor endpoint-in e ri `GET /api/dashboard/me` dhe shfaq përmbajtje sipas rolit.

Dashboard nuk ka nevojë të jetë shumë advanced në Sprint 5, por duhet të ketë metric cards dhe shortcuts të dobishëm.

## Acceptance Criteria

* Dashboard thërret `GET /api/dashboard/me`.
* Dashboard shfaq metric cards sipas response-it.
* Admin sheh shortcuts për user/company admin management.
* CompanyAdmin sheh shortcuts për company agents.
* Agent sheh shortcuts për properties/my properties.
* User sheh shortcuts për Browse Properties dhe Map Search.
* Recent properties shfaqen nëse API i kthen.
* Ka loading, error dhe empty states.
* Frontend build kalon.

## Technical Notes

* Krijo dashboard API client method, p.sh. `getMyDashboard()`.
* Krijo TypeScript interfaces për dashboard response.
* Mos implemento charts të avancuara në këtë ticket, përveç nëse bëhet shumë lehtë.
* Fokusi është role-based summary dhe navigation.

## Dependencies

* ES-155.
* ES-156.
* AuthContext.

## Risk Notes

* Mos shfaq metrics që nuk ekzistojnë për role të caktuar.
* Kujdes me response shape që mund të ndryshojë sipas rolit.

---

# ES-159 — Add Route Protection for Role-Specific Pages

**Labels:** `role-based-ui`, `security`, `frontend`
**Priority:** High

## Description

Faqet e reja në Sprint 5 duhet të jenë të mbrojtura sipas rolit/permission. Nuk mjafton që link-u të mos shfaqet në navigation. Nëse useri e shkruan URL manualisht, duhet të bllokohet.

## Routes to Protect

```text
/admin/users
/company/agents
/properties/new
/dashboard
```

## Acceptance Criteria

* `/admin/users` hapet vetëm nga Admin.
* `/company/agents` hapet vetëm nga CompanyAdmin ose role të lejuara.
* `/properties/new` hapet vetëm nga user me `CreateProperty`.
* `/dashboard` kërkon authenticated user.
* Unauthorized user merr redirect ose access denied page.
* Logged out user ridrejtohet te login.
* Frontend build kalon.

## Technical Notes

* Përmirëso `ProtectedRoute` nëse duhet.
* Mund të krijohen props si:

```tsx
requiredRoles
requiredPermissions
```

* Mbaje logic të centralizuar, jo checks të shpërndara në çdo component.

## Dependencies

* AuthContext.
* Role/permission data në frontend.

## Risk Notes

* Frontend route protection nuk zëvendëson backend authorization.
* Kujdes me user info që mund të jetë ende loading.

---

# ES-160 — Improve Shared Loading, Empty and Error States

**Labels:** `frontend`, `ux-cleanup`
**Priority:** Medium

## Description

Meqë Sprint 5 shton disa faqe të reja dhe ndryshon UI, duhet të kemi loading, empty dhe error states më konsistente.

Kjo ndihmon shumë që app-i të duket si produkt real, jo vetëm si test UI.

## Pages Covered

* Login/Register
* Properties
* Map
* Dashboard
* Company Agents
* Admin Users
* Property Details nëse preket

## Acceptance Criteria

* Ka loading state të qartë për API calls kryesore.
* Ka empty state kur nuk ka properties.
* Ka empty state kur CompanyAdmin nuk ka agents.
* Ka empty state kur Admin list nuk kthen users.
* Ka error state kur API dështon.
* Error messages janë të kuptueshme për userin.
* Nuk shfaqen faqe bosh pa shpjegim.
* Frontend build kalon.

## Technical Notes

Mund të krijohen shared components:

```text
LoadingState
EmptyState
ErrorState
```

* Mos e tepro me dizajn; bëje të thjeshtë dhe konsistent.

## Dependencies

* Faqet e reja nga Sprint 5.

## Risk Notes

* Mos e shndërro në redesign të madh. Ky ticket është cleanup/consistency.

---

# ES-161 — Update README and Sprint Documentation for Role-Based UI and Redis Dashboard

**Labels:** `documentation`, `sprint-5`
**Priority:** Medium

## Description

Pas ndryshimeve të Sprint 5, dokumentimi duhet të përditësohet që projekti të jetë i shpjegueshëm në prezantim. README duhet të përfshijë flow-in e roleve, përdorimin e Redis për dashboard statistics dhe routes të reja.

## Acceptance Criteria

* README përditësohet me routes kryesore.
* Dokumentohet login/register/verify email demo flow.
* Dokumentohet role-based navigation.
* Dokumentohet si Admin krijon CompanyAdmin.
* Dokumentohet si CompanyAdmin krijon Agent.
* Dokumentohet marketplace dhe map search experience.
* Dokumentohet Redis usage për dashboard statistics.
* Dokumentohet cache invalidation në nivel të shkurtër.
* Setup/run instructions mbeten të sakta.
* Nuk përfshihen secrets në dokumentim.

## Technical Notes

Shto seksione si:

```text
Sprint 5 Features
Role-Based Access Summary
Redis Usage
Dashboard Cache Invalidation
User Marketplace Flow
Admin and CompanyAdmin Management Flow
```

## Dependencies

* ES-142 deri ES-160.

## Risk Notes

* Mos e lini dokumentimin për fund fare. Është pjesë e vlerësimit në Lab 2.

---

# Suggested Work Order

## Phase 1 — Auth and Routing Cleanup

1. **ES-142 — Improve Unauthenticated Landing Flow**
2. **ES-143 — Redesign Login Page UI**
3. **ES-144 — Redesign Register and Verify Email UI**
4. **ES-145 — Implement Role-Based Navigation Cleanup**
5. **ES-159 — Add Route Protection for Role-Specific Pages**

## Phase 2 — User Marketplace and Map Search

6. **ES-146 — Convert Properties Page to Marketplace Card Grid**
7. **ES-147 — Separate Create Property Flow from Marketplace Page**
8. **ES-148 — Improve Full Map Search Page for Users**
9. **ES-149 — Connect Marketplace List View and Map Search View**

## Phase 3 — Management UI for Admin and CompanyAdmin

10. **ES-150 — Build CompanyAdmin Agents Management Page**
11. **ES-151 — Add Create Agent Form for CompanyAdmin**
12. **ES-152 — Build Admin Users and CompanyAdmins Management Page**
13. **ES-153 — Add Create CompanyAdmin Form for Admin**
14. **ES-154 — Add Activate and Deactivate User UI**

## Phase 4 — Dashboard and Redis

15. **ES-155 — Implement Role-Based Dashboard Summary Endpoint**
16. **ES-156 — Implement Redis Caching for Role-Based Dashboard Statistics**
17. **ES-157 — Add Dashboard Cache Invalidation on Data Changes**
18. **ES-158 — Build Role-Based Dashboard UI**

## Phase 5 — Cleanup and Documentation

19. **ES-160 — Improve Shared Loading, Empty and Error States**
20. **ES-161 — Update README and Sprint Documentation for Role-Based UI and Redis Dashboard**

---

# Sprint 5 Final Ticket List

```text
ES-142 Improve Unauthenticated Landing Flow
ES-143 Redesign Login Page UI
ES-144 Redesign Register and Verify Email UI
ES-145 Implement Role-Based Navigation Cleanup
ES-146 Convert Properties Page to Marketplace Card Grid
ES-147 Separate Create Property Flow from Marketplace Page
ES-148 Improve Full Map Search Page for Users
ES-149 Connect Marketplace List View and Map Search View
ES-150 Build CompanyAdmin Agents Management Page
ES-151 Add Create Agent Form for CompanyAdmin
ES-152 Build Admin Users and CompanyAdmins Management Page
ES-153 Add Create CompanyAdmin Form for Admin
ES-154 Add Activate and Deactivate User UI
ES-155 Implement Role-Based Dashboard Summary Endpoint
ES-156 Implement Redis Caching for Role-Based Dashboard Statistics
ES-157 Add Dashboard Cache Invalidation on Data Changes
ES-158 Build Role-Based Dashboard UI
ES-159 Add Route Protection for Role-Specific Pages
ES-160 Improve Shared Loading, Empty and Error States
ES-161 Update README and Sprint Documentation for Role-Based UI and Redis Dashboard
```

Kjo është paketa end-to-end për Sprint 5, me **20 tickets**, duke filluar saktë nga **ES-142**.
