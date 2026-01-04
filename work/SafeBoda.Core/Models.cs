namespace SafeBoda.Core
{
    public record Location(double Latitude, double Longitude); 
    public record Rider(Guid Id, string Name, string PhoneNumber); 
    public record Driver(Guid Id, string Name, string PhoneNumber, string MotoPlateNumber); 
    public record Trip
    {
        public Guid Id { get; init; }
        public Guid RiderId { get; init; }
        public Guid DriverId { get; init; }
        public Location Start { get; init; }
        public Location End { get; init; }
        public decimal Fare { get; init; }
        public DateTime RequestTime { get; init; }
    }
     
    public record TripRequest(
        Location StartLocation,
        Location EndLocation,
        Guid RiderId
    );

}