using MongoDB.Bson.Serialization.Attributes;

namespace PrePurchase.Models
{
    /// <summary>
    /// Value Type containing the position data (Latitude and Longitude) information
    /// </summary>

    [BsonNoId]
    public struct Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Position(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
