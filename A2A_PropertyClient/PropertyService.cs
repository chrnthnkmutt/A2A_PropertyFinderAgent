using PropertyFinder.Models;
using System.Text.Json;

namespace PropertyFinder.Services
{
    public class PropertyService
    {
        private readonly List<Property> _properties;

        public PropertyService()
        {
            _properties = LoadProperties();
        }

        private List<Property> LoadProperties()
        {
            try
            {
                var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "properties.json");
                if (!File.Exists(jsonPath))
                {
                    // Try relative path
                    jsonPath = "properties.json";
                }

                var jsonContent = File.ReadAllText(jsonPath);
                var properties = JsonSerializer.Deserialize<List<Property>>(jsonContent) ?? new List<Property>();
                return properties;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading properties: {ex.Message}");
                return new List<Property>();
            }
        }

        public List<Property> SearchProperties(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _properties.Where(p => p.Available).ToList();

            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            return _properties.Where(p => p.Available && searchTerms.Any(term =>
                p.Title.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term) ||
                p.Address.ToLower().Contains(term) ||
                p.Type.ToLower().Contains(term) ||
                p.Amenities.Any(a => a.ToLower().Contains(term))
            )).ToList();
        }

        public List<Property> FilterByPrice(decimal minPrice, decimal maxPrice)
        {
            return _properties.Where(p => p.Available && p.Price >= minPrice && p.Price <= maxPrice).ToList();
        }

        public List<Property> FilterByBedrooms(int bedrooms)
        {
            return _properties.Where(p => p.Available && p.Bedrooms >= bedrooms).ToList();
        }

        public List<Property> FilterByType(string type)
        {
            return _properties.Where(p => p.Available && 
                p.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public Property? GetPropertyById(int id)
        {
            return _properties.FirstOrDefault(p => p.Id == id);
        }

        public List<Property> GetAllAvailableProperties()
        {
            return _properties.Where(p => p.Available).ToList();
        }

        public string ProcessPropertyQuery(string userQuery)
        {
            var query = userQuery.ToLower();

            // Check for specific property types
            if (query.Contains("apartment") || query.Contains("condo") || query.Contains("house") || 
                query.Contains("loft") || query.Contains("townhouse") || query.Contains("penthouse"))
            {
                var type = ExtractPropertyType(query);
                var properties = FilterByType(type);
                return FormatPropertyResults(properties, $"Properties of type '{type}'");
            }

            // Check for price range queries
            if (query.Contains("price") || query.Contains("budget") || query.Contains("$") || query.Contains("under") || query.Contains("between"))
            {
                var (minPrice, maxPrice) = ExtractPriceRange(query);
                if (minPrice > 0 || maxPrice > 0)
                {
                    var properties = FilterByPrice(minPrice, maxPrice == 0 ? decimal.MaxValue : maxPrice);
                    return FormatPropertyResults(properties, $"Properties in price range ${minPrice:N0} - ${(maxPrice == 0 ? "no limit" : maxPrice.ToString("N0"))}");
                }
            }

            // Check for bedroom requirements
            if (query.Contains("bedroom") || query.Contains("bed"))
            {
                var bedrooms = ExtractBedroomCount(query);
                if (bedrooms > 0)
                {
                    var properties = FilterByBedrooms(bedrooms);
                    return FormatPropertyResults(properties, $"Properties with {bedrooms}+ bedrooms");
                }
            }

            // Check for pet-friendly requests
            if (query.Contains("pet") || query.Contains("dog") || query.Contains("cat"))
            {
                var properties = _properties.Where(p => p.Available && p.PetFriendly).ToList();
                return FormatPropertyResults(properties, "Pet-friendly properties");
            }

            // General search
            var searchResults = SearchProperties(userQuery);
            return FormatPropertyResults(searchResults, "Search results");
        }

        private string ExtractPropertyType(string query)
        {
            if (query.Contains("apartment")) return "apartment";
            if (query.Contains("condo")) return "condo";
            if (query.Contains("house")) return "house";
            if (query.Contains("loft")) return "loft";
            if (query.Contains("townhouse")) return "townhouse";
            if (query.Contains("penthouse")) return "penthouse";
            return "house"; // default
        }

        private (decimal minPrice, decimal maxPrice) ExtractPriceRange(string query)
        {
            decimal minPrice = 0;
            decimal maxPrice = 0;

            // Look for price patterns like "under 500000", "between 400000 and 600000", "$500k"
            if (query.Contains("under"))
            {
                var parts = query.Split("under");
                if (parts.Length > 1)
                {
                    maxPrice = ExtractNumericValue(parts[1]);
                }
            }
            else if (query.Contains("between") && query.Contains("and"))
            {
                var betweenIndex = query.IndexOf("between");
                var andIndex = query.IndexOf("and", betweenIndex);
                if (betweenIndex >= 0 && andIndex > betweenIndex)
                {
                    var minPart = query.Substring(betweenIndex + 7, andIndex - betweenIndex - 7);
                    var maxPart = query.Substring(andIndex + 3);
                    minPrice = ExtractNumericValue(minPart);
                    maxPrice = ExtractNumericValue(maxPart);
                }
            }
            else
            {
                // Look for any price mentions
                maxPrice = ExtractNumericValue(query);
            }

            return (minPrice, maxPrice);
        }

        private decimal ExtractNumericValue(string text)
        {
            // Remove common non-numeric characters
            var cleaned = text.Replace("$", "").Replace(",", "").Replace("k", "000").Replace("K", "000");
            
            // Extract numbers
            var numbers = System.Text.RegularExpressions.Regex.Matches(cleaned, @"\d+");
            if (numbers.Count > 0 && decimal.TryParse(numbers[0].Value, out decimal value))
            {
                return value;
            }
            return 0;
        }

        private int ExtractBedroomCount(string query)
        {
            // Look for patterns like "2 bedroom", "3 bed", "studio" (0 bed)
            if (query.Contains("studio")) return 0;
            
            var numbers = System.Text.RegularExpressions.Regex.Matches(query, @"\d+");
            foreach (System.Text.RegularExpressions.Match match in numbers)
            {
                if (int.TryParse(match.Value, out int bedrooms) && bedrooms >= 0 && bedrooms <= 10)
                {
                    return bedrooms;
                }
            }
            return 0;
        }

        private string FormatPropertyResults(List<Property> properties, string title)
        {
            if (!properties.Any())
            {
                return $"🔍 {title}: No properties found matching your criteria. Try adjusting your search terms.";
            }

            var result = $"🔍 {title} ({properties.Count} found):\n\n";
            
            foreach (var property in properties.Take(5)) // Limit to 5 results
            {
                result += property.ToDetailedString() + "\n" + new string('-', 50) + "\n";
            }

            if (properties.Count > 5)
            {
                result += $"\n... and {properties.Count - 5} more properties. Please refine your search for more specific results.";
            }

            return result;
        }
    }
}
