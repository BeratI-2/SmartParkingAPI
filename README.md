# Smart Parking System - SOA API 🚗☁️

Ky projekt është një implementim i plotë i Arkitekturës së Orientuar drejt Shërbimeve (SOA) për një sistem inteligjent parkimi. Sistemi lidh sensorë fizikë IoT (ESP32) me një Backend C# (.NET 8.0) dhe një ndërfaqe Web për përdoruesit.

## 🏗️ Arkitektura e Sistemit
Projekti është i ndarë në shërbime të lirshme (Loose Coupling):
- **Truri (Web API):** Ndërtuar me .NET 8.0, vepron si urë komunikimi RESTful.
- **Baza e të Dhënave:** PostgreSQL e hostuar në Supabase. Komunikimi bëhet përmes Entity Framework Core.
- **Front-End:** Ndërfaqe HTML/JS/CSS e hostuar në Netlify që konsumon API-në asinkronisht.
- **Terminali IoT:** Mikrokontrollues ESP32 me sensorë ultrasonikë (HC-SR04) që dërgon kërkesa HTTP POST drejt API-së.

## 📂 Struktura e Dosjeve (Folder Structure)
- `/Controllers/` - Përmban logjikën e biznesit dhe API Endpoints (`ParkingController.cs`, `AuthController.cs`). Këtu aplikohen rregullat e faturimit dhe sinkronizimit IoT.
- `/Models/` - Përmban klasat e domenit (Domain Models) dhe Data Transfer Objects (DTOs) për sigurinë e të dhënave gjatë transmetimit.
- `ParkingDbContext.cs` - Shtresa e persistencës (Repository Layer) që lidh kodin me bazën e të dhënave.
- `Program.cs` - Pika e nisjes, konfigurimi i CORS dhe injektimi i varësive (Dependency Injection).

## 🚀 Udhëzime për Kompilimin dhe Ekzekutimin (How to Run)

### Parakushtet (Prerequisites)
- SDK e instaluar: **.NET 8.0**
- Visual Studio 2022 (ose i ngjashëm)
- Lidhje interneti e hapur për të aksesuar databazën në Cloud (Supabase).

### Hapat:
1. Ekstraktoni dosjen e projektit.
2. Hapni skedarin `SmartParkingAPI.sln` (ose `.slnx`) në Visual Studio.
3. Prisni që të restaurohen paketat NuGet automatikisht.
4. Klikoni butonin e gjelbër **"Run" (IIS Express ose http)** në Visual Studio.
5. API do të ndizet në `localhost`. Për të parë funksionalitetin vizual, hapni skedarin `index.html` (Front-end) në çdo shfletues.

## 🔐 Siguria dhe Autentifikimi
Sistemi përdor 2FA (Two-Factor Authentication) me kode OTP të dërguara përmes Email-it për të verifikuar përdoruesit, duke zëvendësuar fjalëkalimet tradicionale të pasigurta.

## 📡 API Endpoints Kryesore
- `GET /api/Parking/status` - Kthen statusin Live të të gjitha vendeve.
- `POST /api/Parking/update` - Shërbimi që konsumohet nga pajisja ESP32 për të ndryshuar statusin fizik.
- `POST /api/Auth/login` - Shërbimi i autentifikimit.
