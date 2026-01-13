Create please Web and Desktop application with this requirements:

Kërkesa Funksionale - eProtokoll
Kërkesa Baze
1- Aplikimi duhet te jete desktop/Web me disa nivele perdorimi (Menaxher, Punonjes, Administrator)
2- Registrim te dhëna baze
a. Institucione te jashtëm
b. Lloj klasifikimi dokumenti (Publik, vetëm disa punonjes, Sekret vetëm menaxheri)
c. Numri i protokollit (fillimi & Mbyllja vjetore)
3- Procedura per shkresa te jashtme (pritje) me ngarkim dokumenti
4- Procedura për shkresa te jashtme (dërgese) me ngarkim dokumenti
5- Procedura për shkresa te brendshme me ngarkim dokumenti
6- Gjenerim & Printim Libër Protokolli
7- Shtimi i gjurmimit te nj shkrese (delegim kompetence per kthim pergjigje te punonjesit)
8- Shtimi i afateve per kthim pergjigje
9- Shtimi i punonjesve si perdorues sistemi
10- Skanim Dokumenti (mund te përdoret Librari e gatshme)
11- Raporte statistikor
Kërkesa te avancuara
1- Raport se bashku me vonesat per gjurmimin e një dokumenti
2- Alert për punonjësit për vonesa ose shkresa te reja
3- Raporte per prioritete e çështjeve
4- WebAPI për ngarkimin e dokumentave.
5- Enkriptim të dokumentit nëse klasifikimi shtë sekret.
6- Mbajja e dokumenteve jashtë bazes se te dhënave dhe shmangja e dublikatave te fileve edhe
pse ngarkohen nga nje ose disa perdorues me te njejtin emër apo te ndryshëm.


Like:
# Kërkesa Funksionale – eProtokoll (Backend vetëm) – .NET

> **Scope:** Vetëm Backend (API + DB + Storage integration).  
> **Platformë e sugjeruar:** .NET 8 (ASP.NET Core Web API), EF Core, SQL Server/PostgreSQL, Storage (File System/Azure Blob/S3).

---

## 1. Qëllimi i sistemit

Sistemi **eProtokoll** menaxhon shkresat/dokumentet zyrtare:
- Shkresa **të jashtme** (Pritje / Dërgesë)
- Shkresa **të brendshme**
- Protokollim (numër unik)
- Klasifikim (Publik / I kufizuar / Sekret)
- Delegim, gjurmim, raportim
- Ruajtje dokumentesh (jo domosdoshmërisht brenda DB)

---

## 2. Rolet dhe autorizimi (RBAC)

### 2.1 Role
- **Administrator**
  - menaxhon përdoruesit, rolet, konfigurimet bazë
- **Menaxher**
  - akses në dokumente “Sekret”
  - delegon shkresa
  - monitoron SLA/vonesa
- **Punonjës**
  - regjistron shkresa
  - ngarkon dokumente
  - përgjigjet kur i delegohet

### 2.2 Rregulla të aksesit (shembull)
- Dokument **Publik**: të gjithë përdoruesit e autorizuar.
- Dokument **I kufizuar**: vetëm përdorues të caktuar / departament / listë aksesesh.
- Dokument **Sekret**: vetëm **Menaxher** (dhe admin).

> Backend duhet të implementojë **policy-based authorization** (Claims/Policies).

---

## 3. Regjistrime bazë (Master Data)

### 3.1 Institucione të jashtme
- CRUD për lista institucionesh (emër, NUIS/ID, adresë, kontakt, etj.)

### 3.2 Klasifikimi i dokumenteve
- **Publik**
- **I kufizuar**
- **Sekret**

### 3.3 Konfigurimi i protokollit
- **Fillimi / Mbyllja e librit** (p.sh. vjetor)
- Numër protokolli **unik** sipas rregullit:
  - p.sh. `YYYY-000001` ose `NR/YY`
- **Atomic increment** (concurrency safe) — nuk lejohet dublim.

---

## 4. Menaxhimi i shkresave

### 4.1 Shkresa të jashtme – Pritje (Incoming)
- Regjistrim i shkresës
- Protokollim
- Ngarkim dokumenti (file)
- Status (p.sh. `E re`, `Në trajtim`, `Mbyllur`)

### 4.2 Shkresa të jashtme – Dërgesë (Outgoing)
- Regjistrim i shkresës dalëse
- Protokollim
- Ngarkim dokumenti
- Ruajtje e të dhënave të dërgesës (kanali, data, etj.)

### 4.3 Shkresa të brendshme
- Regjistrim + protokollim (nëse kërkohet)
- Ngarkim dokumenti
- Rrjedhë e brendshme (delegim / përgjigje / mbyllje)

---

## 5. Delegimi & Gjurmimi (Audit Trail)

### 5.1 Delegim kompetence
- Menaxheri delegon një shkresë tek një punonjës
- Punonjësi ka të drejtë:
  - ta shikojë
  - të shtojë komente / përgjigje
  - të ndryshojë statusin sipas rregullave

### 5.2 Gjurmimi i veprimeve
Çdo veprim duhet të log-ohet:
- kush e krijoi / modifikoi
- kush e delegoi
- kush e hapi / shkarkoi dokumentin
- ndryshime statusi
- afate & vonesa

> Kjo realizohet me një tabelë `AuditLogs` + `DocumentHistory`.

---

## 6. Dokumentet (Storage & Deduplication)

### 6.1 Ruajtja e dokumenteve
Opsione:
- FileSystem (on-prem)
- Azure Blob Storage
- AWS S3 / kompatibël

Në DB ruhen vetëm:
- metadata e dokumentit
- `StoragePath` / `BlobKey`
- `ContentType`, `Size`, `Hash`

### 6.2 Parandalim dublikatesh
- Për çdo dokument të ngarkuar llogaritet `SHA-256 hash`
- Nëse ekziston i njëjti hash:
  - ose refuzohet ngarkimi
  - ose lidhet si “i njëjti file” (re-use)

---

## 7. Raporte & statistika

Raporte bazë:
- numri i shkresave sipas periudhe
- sipas klasifikimit
- sipas statusit
- sipas përdoruesit/departamentit

Raporte të avancuara:
- shkresa në vonesë (SLA)
- punonjësit me ngarkesë më të lartë
- prioritetet e çështjeve

---

## 8. Kërkesat e avancuara (Phase 2)

1. **Alert/Notifications**
   - shkresa të reja
   - shkresa në vonesë
2. **Prioritetet e çështjeve**
   - `Low/Medium/High/Urgent`
3. **WebAPI për ngarkim dokumentesh**
   - endpoint i dedikuar për integrime
4. **Enkriptim për dokumentet “Sekret”**
   - encryption at rest (p.sh. envelope encryption)
5. **Ruajtje jashtë DB + shmangie dublikatesh**
   - hashing + storage provider

---

## 9. Modeli i të dhënave (Propozim DB)

### 9.1 Entitete kryesore
- `Users` (ose Identity tables)
- `Roles`
- `ExternalInstitutions`
- `ProtocolBooks` (p.sh. viti, status open/closed)
- `ProtocolCounters` (counter atomic)
- `Documents` (metadata)
- `Letters` / `Correspondences` (shkresa)
- `Assignments` (delegime)
- `Responses` (përgjigje / komente)
- `AuditLogs`

### 9.2 Letter / Correspondence (fusha minimum)
- `Id`
- `Type` (`Incoming`, `Outgoing`, `Internal`)
- `Classification` (`Public`, `Restricted`, `Secret`)
- `ProtocolNumber`
- `Subject`
- `ExternalInstitutionId` (nullable)
- `CreatedByUserId`
- `AssignedToUserId` (nullable)
- `Priority` (Phase 2)
- `DueDate` (Phase 2 / SLA)
- `Status`
- `CreatedAt`, `UpdatedAt`

### 9.3 Document (fusha minimum)
- `Id`
- `LetterId`
- `FileName`
- `ContentType`
- `SizeBytes`
- `HashSha256`
- `StorageProvider`
- `StorageKey`
- `IsEncrypted`
- `CreatedAt`

---

## 10. API Endpoints (Propozim)

> Prefix: `/api/v1`

### 10.1 Auth
- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/logout`

### 10.2 Users (Admin)
- `GET /users`
- `POST /users`
- `PUT /users/{id}`
- `DELETE /users/{id}`
- `POST /users/{id}/roles`

### 10.3 Master Data
- `GET /institutions`
- `POST /institutions`
- `PUT /institutions/{id}`
- `DELETE /institutions/{id}`

- `GET /classifications`
- `GET /protocol-books`
- `POST /protocol-books/open`
- `POST /protocol-books/close`

### 10.4 Letters / Shkresa
- `GET /letters?type=&status=&from=&to=&classification=&assignedTo=`
- `GET /letters/{id}`
- `POST /letters` (krijon shkresë + opsional protokoll)
- `PUT /letters/{id}`
- `POST /letters/{id}/assign` (delegim)
- `POST /letters/{id}/status` (ndryshim statusi)

### 10.5 Documents
- `POST /letters/{id}/documents` (upload)
- `GET /letters/{id}/documents`
- `GET /documents/{id}/download`
- `DELETE /documents/{id}` (sipas policy)

### 10.6 Reports
- `GET /reports/summary?from=&to=`
- `GET /reports/overdue?from=&to=`
- `GET /reports/by-user?from=&to=`

---

## 11. Non-Functional Requirements

- **Audit**: çdo veprim i rëndësishëm log-ohet
- **Security**:
  - JWT + role/policy authorization
  - dokumentet “Sekret” me akses strikt
  - encryption at rest për “Sekret” (Phase 2)
- **Performance**:
  - pagination & filtering në listime
  - indexing (ProtocolNumber, Status, Type, CreatedAt, AssignedTo)
- **Observability**:
  - logs (Serilog)
  - metrics / health checks
- **Data integrity**:
  - protokollimi duhet të jetë atomic dhe pa dublime

---

## 12. Teknologji të sugjeruara (.NET)

- ASP.NET Core Web API (.NET 8)
- EF Core 8
- DB: SQL Server ose PostgreSQL
- Auth: ASP.NET Core Identity + JWT
- Storage: Azure Blob / S3 / FileSystem abstraction
- Background jobs (Phase 2 Alerts): Hangfire / Quartz
- Swagger/OpenAPI

---

## 13. Deliverables (Backend)

1. REST API + Swagger
2. Database schema + migrations
3. Storage provider abstraction
4. RBAC + policies për klasifikim
5. Audit trail
6. Raporte bazë
7. (Opsionale) Alerts + SLA + Encryption (Phase 2)

---
