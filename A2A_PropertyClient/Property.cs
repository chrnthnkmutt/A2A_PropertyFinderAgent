using System.Text.Json.Serialization;

namespace PropertyFinder.Models
{
    public class Property
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("bedrooms")]
        public int Bedrooms { get; set; }

        [JsonPropertyName("bathrooms")]
        public decimal Bathrooms { get; set; }

        [JsonPropertyName("sqft")]
        public int Sqft { get; set; }

        [JsonPropertyName("yearBuilt")]
        public int YearBuilt { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("amenities")]
        public List<string> Amenities { get; set; } = new();

        [JsonPropertyName("petFriendly")]
        public bool PetFriendly { get; set; }

        [JsonPropertyName("available")]
        public bool Available { get; set; }

        public override string ToString()
        {
            return $"{Title} - ${Price:N0} | {Bedrooms}BR/{Bathrooms}BA | {Sqft} sqft | {Address}";
        }

        public string ToDetailedString()
        {
            var amenitiesStr = Amenities.Any() ? string.Join(", ", Amenities) : "None listed";
            return $@"
🏠 {Title}
📍 {Address}
💰 ${Price:N0}
🛏️ {Bedrooms} bedrooms, {Bathrooms} bathrooms
📐 {Sqft} square feet
🏗️ Built in {YearBuilt}
📝 {Description}
🎯 Amenities: {amenitiesStr}
🐕 Pet Friendly: {(PetFriendly ? "Yes" : "No")}
✅ Available: {(Available ? "Yes" : "No")}";
        }
    }
}
