namespace SmartParkingAPI
{
    // 1. Modeli origjinal që kërkon Controlleri yt për ESP32 dhe InfluxDB
    public class ParkingStatus
    {
        public int Spot1 { get; set; }
        public int Spot2 { get; set; }
        public int Spot3 { get; set; }
        public int FreeSpots { get; set; }
    }

    // 2. Logjika e re e ngjyrave që do të na duhet për më vonë
    public enum SpotColor
    {
        Free = 0,       // E Gjelbër
        Reserved = 1,   // Portokalli
        Occupied = 2    // E Kuqe
    }
}