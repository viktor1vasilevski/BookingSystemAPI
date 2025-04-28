using Main.DTOs;

namespace Main.Interfaces;

public interface ITravelApiClient
{
    Task<List<HotelDTO>> FetchHotelsAsync(string destinationCode);
    Task<List<FlightDTO>> FetchFlightsAsync(string destination, string departureAirport);
    Task<(List<HotelDTO> Hotels, List<FlightDTO> Flights)> FetchHotelsAndFlightsAsync(string destination, string departureAirport);
}
