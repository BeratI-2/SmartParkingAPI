using Microsoft.AspNetCore.Mvc;
using SmartParkingAPI;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SmartParkingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public AuthController(ParkingDbContext context)
        {
            _context = context;
        }

        public class UserDto
        {
            public string Emri { get; set; } = string.Empty;
            public string Email { get; set; }
            public string Fjalekalimi { get; set; }
            public string KodVerifikimi { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto request)
        {
            try
            {
                string[] allowedDomains = { "gmail.com", "hotmail.com", "seeu.edu.mk", "yahoo.com", "outlook.com" };
                string emailDomain = request.Email.Split('@').LastOrDefault()?.ToLower();

                if (!allowedDomains.Contains(emailDomain))
                    return BadRequest(new { mesazhi = "🛑 Regjistrimi lejohet vetëm me adresa zyrtare!" });

                if (_context.Perdoruesit.Any(p => p.Email == request.Email))
                    return BadRequest(new { mesazhi = "Ky email është i regjistruar tashmë!" });

                string kodiRastesor = new Random().Next(100000, 999999).ToString();

                var newUser = new Perdorues
                {
                    Emri = request.Emri,
                    Email = request.Email.ToLower(),
                    Fjalekalimi = request.Fjalekalimi,
                    Roli = "Klient",
                    Verifikuar = false,
                    KodVerifikimi = kodiRastesor
                };

                _context.Perdoruesit.Add(newUser);
                _context.SaveChanges(); // Nëse Supabase nuk i ka kolonat e reja, këtu ndodh Crash!

                // Kalojmë dërgimin e emailit në sfond që të mos presim
                _ = Task.Run(() => DergoEmailReal(newUser.Email, newUser.Emri, kodiRastesor));

                return Ok(new
                {
                    mesazhi = "Regjistrimi u krye me sukses!",
                    kodiPrezantimi = kodiRastesor
                });
            }
            catch (Exception)
            {
                // Nëse ndodh Crash, serveri ta tregon errorin, nuk rri duke u menduar
                return StatusCode(500, new { mesazhi = "GABIM NË DATABAZË: Sigurohuni që keni shtuar kolonat 'Verifikuar' dhe 'KodVerifikimi' në Supabase!" });
            }
        }

        [HttpPost("verify")]
        public IActionResult VerifyCode([FromBody] UserDto request)
        {
            var user = _context.Perdoruesit.FirstOrDefault(p => p.Email == request.Email.ToLower());
            if (user == null) return NotFound(new { mesazhi = "Përdoruesi nuk u gjet!" });

            if (user.KodVerifikimi == request.KodVerifikimi)
            {
                user.Verifikuar = true;
                user.KodVerifikimi = null;
                _context.SaveChanges();
                return Ok(new { mesazhi = "✅ Profil i Verifikuar! Tani mund të kyçeni." });
            }

            return BadRequest(new { mesazhi = "❌ Kodi i verifikimit është i pasaktë!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto request)
        {
            var user = _context.Perdoruesit.FirstOrDefault(p => p.Email == request.Email.ToLower() && p.Fjalekalimi == request.Fjalekalimi);
            if (user == null) return Unauthorized(new { mesazhi = "Email ose Fjalëkalimi është gabim!" });
            if (!user.Verifikuar) return BadRequest(new { mesazhi = "🔒 Llogaria nuk është e verifikuar ende!" });

            return Ok(new { id = user.Id, emri = user.Emri, email = user.Email, mesazhi = "Login i suksesshëm!" });
        }

        private void DergoEmailReal(string emailDestinacioni, string emriKlientit, string kodi)
        {
            try
            {
                string emailDergues = "beratidrizi4@gmail.com";
                string appPassword = "nvhhlfsmznrmhtyy";

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailDergues, "Smart Parking Tetovë");
                    mail.To.Add(new MailAddress(emailDestinacioni));
                    mail.Subject = "Kodi i Verifikimit (OTP) - Smart Parking";
                    mail.IsBodyHtml = true;
                    mail.Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px;'>
                        <h2 style='color: #2563eb; text-align: center;'>Smart Parking Tetovë</h2>
                        <p>Përshëndetje <b>{emriKlientit}</b>,</p>
                        <p>Kodi juaj i sigurisë është:</p>
                        <div style='background-color: #f8fafc; padding: 15px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                            <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #0f172a;'>{kodi}</span>
                        </div>
                    </div>";

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(emailDergues, appPassword);
                        smtp.EnableSsl = true;
                        smtp.Timeout = 10000; // Mbrojtja sekrete: S'lejon të ngrijë
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error SMTP: " + ex.Message);
            }
        }
    }
}