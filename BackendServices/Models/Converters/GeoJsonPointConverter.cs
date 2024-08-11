using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver.GeoJsonObjectModel;

public class GeoJsonPointConverter : JsonConverter<GeoJsonPoint<GeoJson2DGeographicCoordinates>>
{
    public override GeoJsonPoint<GeoJson2DGeographicCoordinates> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        double latitude = 0;
        double longitude = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                    new GeoJson2DGeographicCoordinates(longitude, latitude));
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read();

                if (propertyName == "coordinates")
                {
                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        reader.Read();
                        longitude = reader.GetDouble();
                        reader.Read();
                        latitude = reader.GetDouble();
                        reader.Read();
                    }
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, GeoJsonPoint<GeoJson2DGeographicCoordinates> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("type");
        writer.WriteStringValue("Point");

        writer.WritePropertyName("coordinates");
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Coordinates.Longitude);
        writer.WriteNumberValue(value.Coordinates.Latitude);
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
