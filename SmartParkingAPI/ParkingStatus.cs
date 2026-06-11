namespace SmartParkingAPI
{
    
    public class ParkingStatus
    {
        public int Spot1 { get; set; }
        public int Spot2 { get; set; }
        public int Spot3 { get; set; }
        public int FreeSpots { get; set; }
    }

    
    public enum SpotColor
    {
        Free = 0,       
        Reserved = 1,   
        Occupied = 2    
    }
}