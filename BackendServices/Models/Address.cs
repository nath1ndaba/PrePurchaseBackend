using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver.GeoJsonObjectModel;
using PrePurchase.Models.Converters;
using System.Text.Json.Serialization;

namespace BackendServices.Models;
[BsonNoId]
[BsonIgnoreExtraElements]
public record Address
{
    [JsonConverter(typeof(ObjectIdConverter))]
    [BsonId]
    [BsonRequired]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [JsonConverter(typeof(ObjectIdConverter))]
    public ObjectId AddressBelongsToId { get; set; }

    public string Street { get; set; }
    public string City { get; set; }
    public string Suburb { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }

    [BsonRepresentation(BsonType.Double)]
    public double Latitude { get; set; }

    [BsonRepresentation(BsonType.Double)]
    public double Longitude { get; set; }

    [JsonConverter(typeof(GeoJsonPointConverter))]
    public GeoJsonPoint<GeoJson2DGeographicCoordinates> Coordinates
    {
        get => new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
            new GeoJson2DGeographicCoordinates(Longitude, Latitude));
        set
        {
            Latitude = value.Coordinates.Latitude;
            Longitude = value.Coordinates.Longitude;
        }
    }
}
