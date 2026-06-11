using Microsoft.EntityFrameworkCore;
using System;

namespace SmartParkingAPI
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options) { }

        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<ParkingLog> ParkingLogs { get; set; }
        public DbSet<Perdorues> Perdoruesit { get; set; }
        public DbSet<Transaksion> Transaksionet { get; set; }
        public DbSet<Raportim> Raportimet { get; set; }
    }

    public class ParkingSpot
    {
        public int Id { get; set; }
        public string Emri { get; set; }
        public int Statusi { get; set; }
        public int? RezervuarNga_Id { get; set; }
        public string? Targat { get; set; } // <--- Targat e Tetovës
    }

    public class ParkingLog
    {
        public int Id { get; set; }
        public int ParkingSpotId { get; set; }
        public DateTime Koha_Zonies { get; set; }
    }

    public class Perdorues
    {
        public int Id { get; set; }
        public string Emri { get; set; }
        public string Email { get; set; }
        public string Fjalekalimi { get; set; }
        public string? Roli { get; set; }
        public bool Verifikuar { get; set; }
        public string? KodVerifikimi { get; set; }
        public decimal Balanca { get; set; } // <--- Portofoli Digjital
    }

    public class Transaksion
    {
        public int Id { get; set; }
        public int PerdoruesId { get; set; }
        public decimal Shuma { get; set; }
        public string Pershkrimi { get; set; }
        public DateTime Data { get; set; }
    }

    public class Raportim
    {
        public int Id { get; set; }
        public int PerdoruesId { get; set; }
        public string Tipi { get; set; }
        public string Pershkrimi { get; set; }
        public DateTime Data { get; set; }
    }
}