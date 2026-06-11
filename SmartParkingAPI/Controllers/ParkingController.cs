using Microsoft.AspNetCore.Mvc;
using SmartParkingAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace SmartParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public ParkingController(ParkingDbContext context)
        {
            _context = context;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(_context.ParkingSpots.ToList());
        }

        // ==========================================
        // 1. REZERVIMI (LLOGARITJA INTELIGJENTE E ORËVE)
        // ==========================================
        public class RezervoRequest
        {
            public int UserId { get; set; }
            public string Targat { get; set; }
            public string DataRezervimit { get; set; }
            public string OraRezervimit { get; set; }  // Ora Hyrjes
            public string OraDaljes { get; set; }      // Ora Daljes
        }

        [HttpPost("rezervo/{id}")]
        public IActionResult RezervoVendin(int id, [FromBody] RezervoRequest request)
        {
            var spot = _context.ParkingSpots.FirstOrDefault(s => s.Id == id);
            var user = _context.Perdoruesit.FirstOrDefault(u => u.Id == request.UserId);

            if (spot == null || user == null) return NotFound(new { mesazhi = "Gabim në sistem!" });
            if (spot.Statusi != 0) return BadRequest(new { mesazhi = "Ky vend nuk është i lirë!" });

            int startHour = 12, endHour = 13;
            if (!string.IsNullOrEmpty(request.OraRezervimit) && !string.IsNullOrEmpty(request.OraDaljes))
            {
                int.TryParse(request.OraRezervimit.Split(':')[0], out startHour);
                int.TryParse(request.OraDaljes.Split(':')[0], out endHour);
            }

            int paidHours = 0;
            int freeHours = 0;
            int totalHours = 0;
            int current = startHour;

            // Ndarja perfekte e orëve (Ditë vs Natë)
            while (current != endHour)
            {
                if (current >= 7 && current < 21) paidHours++;
                else freeHours++;

                current = (current + 1) % 24;
                totalHours++;
                if (totalHours > 24) break;
            }

            decimal cmimi = paidHours * 40.0m;
            string faturaMsg = "";
            string cmimiFatures = "FALAS";

            if (cmimi > 0)
            {
                if (user.Balanca < cmimi)
                    return BadRequest(new { mesazhi = $"💳 Balancë e pamjaftueshme! ({paidHours} orë me pagesë x 40 MKD = {cmimi} MKD)." });

                user.Balanca -= cmimi;
                _context.Transaksionet.Add(new Transaksion
                {
                    PerdoruesId = user.Id,
                    Shuma = -cmimi,
                    Pershkrimi = $"Rezervim: {spot.Emri} ({startHour}:00 - {endHour}:00)",
                    Data = DateTime.UtcNow.AddHours(2)
                });

                faturaMsg = $"U paguan {cmimi} MKD për {paidHours} orë ({freeHours} orë ishin falas). Fatura u dërgua!";
                cmimiFatures = $"{cmimi}.00 MKD";
            }
            else
            {
                faturaMsg = $"Rezervimi për {totalHours} orë u krye FALAS (Të gjitha orët ishin natën).";
            }

            spot.Statusi = 1;
            spot.RezervuarNga_Id = user.Id;
            spot.Targat = $"{request.Targat.ToUpper()} ({request.OraRezervimit} - {request.OraDaljes})";

            _context.SaveChanges();

            // DËRGIMI I FATURËS NË EMAIL ME QR CODE
            string qrData = Uri.EscapeDataString($"Smart Parking\nKlienti: {user.Emri}\nVendi: {spot.Emri}\nTarga: {request.Targat.ToUpper()}\nOrari: {request.DataRezervimit} ({request.OraRezervimit}-{request.OraDaljes})");
            string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={qrData}";

            _ = Task.Run(() => DergoFatureEmail(user.Email, user.Emri, request.Targat.ToUpper(), spot.Emri, request.DataRezervimit, request.OraRezervimit, request.OraDaljes, cmimiFatures, qrUrl));

            return Ok(new { mesazhi = faturaMsg });
        }

        private void DergoFatureEmail(string emailDestinacioni, string emri, string targa, string vendi, string data, string oraHyrjes, string oraDaljes, string cmimi, string qrUrl)
        {
            try
            {
                string emailDergues = "beratidrizi4@gmail.com"; string appPassword = "nvhhlfsmznrmhtyy";
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailDergues, "Smart Parking Tetovë");
                    mail.To.Add(new MailAddress(emailDestinacioni));
                    mail.Subject = "Fatura e Rezervimit (Invoice) & QR Code";
                    mail.IsBodyHtml = true;
                    mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);'>
                        <div style='background-color: #1e293b; padding: 20px; text-align: center; color: white;'><h2 style='margin: 0; color: #3b82f6;'>Smart Parking <span style='color: white;'>Tetovë</span></h2><p style='margin: 5px 0 0 0; opacity: 0.8;'>Pagesa e suksesshme ✅</p></div>
                        <div style='padding: 25px; background-color: #f8fafc;'>
                            <p>Përshëndetje <b>{emri}</b>, faleminderit që përdorni shërbimet tona.</p>
                            <div style='background: white; border: 1px dashed #cbd5e1; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                                <p style='margin: 5px 0; color: #64748b;'>Vendi i rezervuar: <b style='color: #0f172a; float: right;'>{vendi}</b></p>
                                <p style='margin: 5px 0; color: #64748b;'>Targa: <b style='color: #0f172a; float: right;'>{targa}</b></p>
                                <p style='margin: 5px 0; color: #64748b;'>Data: <b style='color: #0f172a; float: right;'>{data}</b></p>
                                <p style='margin: 5px 0; color: #64748b;'>Orari: <b style='color: #0f172a; float: right;'>{oraHyrjes} - {oraDaljes}</b></p>
                                <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 15px 0;'>
                                <p style='margin: 5px 0; font-size: 18px; color: #0f172a;'><b>Totali:</b> <b style='color: #10b981; float: right;'>{cmimi}</b></p>
                            </div>
                            <div style='text-align: center; margin-top: 25px;'><p style='font-size: 12px; color: #64748b; margin-bottom: 10px;'>Skanoni këtë kod në hyrje të parkingut:</p><img src='{qrUrl}' alt='QR Code' width='150' height='150' style='border-radius: 8px; border: 3px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.1);'></div>
                        </div>
                    </div>";
                    using (var smtp = new SmtpClient("smtp.gmail.com", 587)) { smtp.Credentials = new NetworkCredential(emailDergues, appPassword); smtp.EnableSsl = true; smtp.Timeout = 10000; smtp.Send(mail); }
                }
            }
            catch (Exception) { }
        }

        // ==========================================
        // 2. ANULIMI
        // ==========================================
        [HttpPost("anulo/{id}/{userId}")]
        public IActionResult AnuloRezervimin(int id, int userId)
        {
            var spot = _context.ParkingSpots.FirstOrDefault(s => s.Id == id);
            if (spot == null) return NotFound(new { mesazhi = "Vendi nuk ekziston!" });

            if (spot.RezervuarNga_Id != userId)
                return BadRequest(new { mesazhi = "Nuk mund të anulosh rezervimin e dikujt tjetër!" });

            spot.Statusi = 0; spot.RezervuarNga_Id = null; spot.Targat = null;
            _context.SaveChanges();
            return Ok(new { mesazhi = "Vendi u lirua me sukses!" });
        }

        // ==========================================
        // 3. E-WALLET, I-REPORT & STATISTIKAT
        // ==========================================
        public class RaportRequest { public int UserId { get; set; } public string Tipi { get; set; } public string Pershkrimi { get; set; } }

        [HttpPost("raporto")]
        public IActionResult RaportoProblem([FromBody] RaportRequest req)
        {
            _context.Raportimet.Add(new Raportim { PerdoruesId = req.UserId, Tipi = req.Tipi, Pershkrimi = req.Pershkrimi, Data = DateTime.UtcNow.AddHours(2) });
            _context.SaveChanges(); return Ok(new { mesazhi = "🚨 Raportimi u regjistrua me sukses!" });
        }

        public class FondeRequest { public int UserId { get; set; } public decimal Shuma { get; set; } public string Kartela { get; set; } }

        [HttpPost("shto-fonde")]
        public IActionResult ShtoFonde([FromBody] FondeRequest req)
        {
            var user = _context.Perdoruesit.Find(req.UserId);
            if (user == null) return NotFound();
            user.Balanca += req.Shuma;
            _context.Transaksionet.Add(new Transaksion { PerdoruesId = user.Id, Shuma = req.Shuma, Pershkrimi = $"Depozitim me kartelë (**** {req.Kartela.Substring(Math.Max(0, req.Kartela.Length - 4))})", Data = DateTime.UtcNow.AddHours(2) });
            _context.SaveChanges(); return Ok(new { mesazhi = $"💳 U shtuan {req.Shuma} MKD!", balancaERe = user.Balanca });
        }

        [HttpGet("profili/{userId}")]
        public IActionResult GetProfili(int userId)
        {
            var user = _context.Perdoruesit.Find(userId);
            if (user == null) return NotFound();
            var historiku = _context.Transaksionet.Where(t => t.PerdoruesId == userId).OrderByDescending(t => t.Data).Take(10).ToList();
            return Ok(new { balanca = user.Balanca, historiku = historiku });
        }

        [HttpGet("statistikat")]
        public IActionResult GetStatistikat()
        {
            var allSpots = _context.ParkingSpots.ToList(); var allLogs = _context.ParkingLogs.ToList();
            var fluksiSot = new int[24]; var sot = DateTime.UtcNow.Date;
            foreach (var log in allLogs.Where(l => l.Koha_Zonies >= sot)) fluksiSot[log.Koha_Zonies.AddHours(2).Hour]++;
            return Ok(new { TotaliVendeve = allSpots.Count, TeLira = allSpots.Count(s => s.Statusi == 0), TeRezervuara = allSpots.Count(s => s.Statusi == 1), TeZena = allSpots.Count(s => s.Statusi == 2), VendiMeIPerdorur = allLogs.Any() ? allSpots.FirstOrDefault(s => s.Id == allLogs.GroupBy(l => l.ParkingSpotId).OrderByDescending(g => g.Count()).First().Key)?.Emri ?? "Nuk ka" : "Nuk ka", FluksiOreve = fluksiSot });
        }

        [HttpPost("update")]
        public IActionResult UpdateStatus([FromBody] HardwareUpdate request)
        {
            var spot = _context.ParkingSpots.Find(request.SpotId);
            if (spot == null) return NotFound();
            if (request.Statusi == 0) { spot.RezervuarNga_Id = null; spot.Targat = null; }
            if (spot.Statusi != 2 && request.Statusi == 2) _context.ParkingLogs.Add(new ParkingLog { ParkingSpotId = spot.Id, Koha_Zonies = DateTime.UtcNow });
            spot.Statusi = request.Statusi; _context.SaveChanges(); return Ok();
        }
    }
    public class HardwareUpdate { public int SpotId { get; set; } public int Statusi { get; set; } }
}