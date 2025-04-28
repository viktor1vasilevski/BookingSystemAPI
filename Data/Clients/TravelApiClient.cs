using Main.DTOs;
using Main.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Data.Clients;

public class TravelApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<TravelApiClient> logger) : ITravelApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<TravelApiClient> _logger = logger;

    public async Task<List<FlightDTO>> FetchFlightsAsync(string destination, string departureAirport)
    {
        string flightApiUrl = _configuration["ApiEndpoint:SearchFlights"]
            .Replace("{departureAirport}", departureAirport ?? "")
            .Replace("{arrivalAirport}", destination);
        try
        {


            var response = await _httpClient.GetAsync(flightApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch flights. API response: {errorMessage}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<FlightDTO>>(content) ?? new List<FlightDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching flights from {Url} at {Timestamp}",
                flightApiUrl, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            throw;
        }
    }

    public async Task<(List<HotelDTO> Hotels, List<FlightDTO> Flights)> FetchHotelsAndFlightsAsync(string destination, string departureAirport)
    {
        var hotelTask = FetchHotelsAsync(destination);
        var flightTask = FetchFlightsAsync(destination, departureAirport);

        await Task.WhenAll(hotelTask, flightTask);

        return (await hotelTask, await flightTask);
    }

    public async Task<List<HotelDTO>> FetchHotelsAsync(string destinationCode)
    {
        string hotelApiUrl = _configuration["ApiEndpoint:SearchHotels"].Replace("{destinationCode}", destinationCode);

        try
        {
            var response = await _httpClient.GetAsync(hotelApiUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<HotelDTO>>(content) ?? new List<HotelDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch hotels for destination {DestinationCode}.", destinationCode);
            throw;
        }
    }

}
