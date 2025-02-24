using FluentValidation;
using Main.Constants;
using Main.DTOs;
using Main.Enums;
using Main.Helpers;
using Main.Hubs;
using Main.Interfaces;
using Main.Requests;
using Main.Responses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Main.Services;

public class ManagerService : IManagerService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManagerService> _logger;
    private readonly IValidator<SearchRequest> _searchRequestValidator;
    private readonly IValidator<BookRequest> _bookRequestValidator;
    private readonly IHubContext<BookingHub> _hubContext;

    private static readonly Dictionary<string, BookingDTO> _bookings = new();
    private static readonly Random _random = new();

    public ManagerService(HttpClient httpClient, IConfiguration configuration, ILogger<ManagerService> logger, 
        IValidator<SearchRequest> searchRequestValidator, IValidator<BookRequest> bookRequestValidator, IHubContext<BookingHub> hubContext)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _searchRequestValidator = searchRequestValidator;
        _bookRequestValidator = bookRequestValidator;
        _hubContext = hubContext;
    }

    public async Task<ApiResponse<SearchResponse>> SearchAsync(SearchRequest request)
    {
        var validationResult = ValidationHelper.ValidateRequest<SearchRequest, SearchResponse>(request, _searchRequestValidator);

        if (validationResult != null)
            return validationResult;

        var searchType = DetermineSearchType(request);

        string hotelApiUrl = _configuration["ApiEndpoint:SearchHotels"].Replace("{destinationCode}", request.Destination);
        string flightApiUrl = _configuration["ApiEndpoint:SearchFlights"]
            .Replace("{departureAirport}", request.DepartureAirport ?? "")
            .Replace("{arrivalAirport}", request.Destination);

        try
        {
            var options = new List<Option>();

            if (searchType == SearchTypeEnum.HotelOnly.ToString() || searchType == SearchTypeEnum.LastMinuteHotels.ToString())
            {
                var hotels = await FetchHotelsAsync(hotelApiUrl);
                options.AddRange(hotels.Select(hotel => new Option
                {
                    OptionCode = Guid.NewGuid().ToString(),
                    HotelCode = hotel.HotelCode.ToString(),
                    FlightCode = "",
                    ArrivalAirport = hotel.DestinationCode,
                    Price = RandomHelper.GenerateRandomDouble(100, 500),
                    HotelName = hotel.HotelName,
                    City = hotel.City
                }));

                return new ApiResponse<SearchResponse>
                {
                    Success = true,
                    Data = new SearchResponse { Options = options, SearchType = SearchTypeEnum.HotelOnly },
                    NotificationType = NotificationTypeEnum.Success
                };
            }
            else if (searchType == SearchTypeEnum.HotelAndFlight.ToString())
            {
                var (hotels, flights) = await FetchHotelsAndFlightsAsync(hotelApiUrl, flightApiUrl);
                options = PopulateHotelsAndFlightsOptions(hotels, flights);
            }

            return new ApiResponse<SearchResponse>
            {
                Success = true,
                Data = new SearchResponse { Options = options, SearchType = SearchTypeEnum.HotelAndFlight },
                NotificationType = NotificationTypeEnum.Success
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the search request at {Timestamp}. Destination: {Destination}, Departure: {DepartureAirport}",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.Destination, request.DepartureAirport);

            return new ApiResponse<SearchResponse>
            {
                Success = false,
                Message = ManagerConstants.ERROR_SEARCH_REQUEST,
                NotificationType = NotificationTypeEnum.ServerError
            };
        }
    }

    public ApiResponse<BookResponse> Book(BookRequest request)
    {
        var validationResult = ValidationHelper.ValidateRequest<BookRequest, BookResponse>(request, _bookRequestValidator);

        if (validationResult != null)
            return validationResult;

        string bookingCode = GenerateBookingCode();
        int sleepTime = RandomHelper.GenerateRandomInt(5, 10);
        DateTime bookingTime = DateTime.UtcNow;

        var booking = new BookingDTO
        {
            BookingCode = bookingCode,
            SleepTime = sleepTime, 
            BookingTime = bookingTime,
            Status = BookingStatusEnum.Pending
        };

        if (request.SearchReq.FromDate >= DateTime.UtcNow && request.SearchReq.FromDate <= DateTime.UtcNow.AddDays(45))
            booking.SearchType = SearchTypeEnum.LastMinuteHotels.ToString();

        _bookings[bookingCode] = booking;

        return new ApiResponse<BookResponse>
        {
            Success = true,
            NotificationType = NotificationTypeEnum.Success,
            Data = new BookResponse
            {
                BookingCode = bookingCode,
                BookingTime = bookingTime
            }
        };
    }

    public async Task<ApiResponse<CheckStatusResponse>> CheckStatusAsync(CheckStatusRequest request)
    {
        try
        {
            if (_bookings.ContainsKey(request.BookingCode))
            {
                var booking = _bookings[request.BookingCode];
                _ = Task.Run(async () =>
                {
                    await Task.Delay(booking.SleepTime * 1000);

                    if (booking.SearchType == "LastMinuteHotels")
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", BookingStatusEnum.Failed, ManagerConstants.BOOKING_FAILED);
                    }
                    else
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", BookingStatusEnum.Success, ManagerConstants.BOOKING_COMPLETED);
                    }

                });
                return new ApiResponse<CheckStatusResponse>
                {
                    Message = ManagerConstants.PENDING,
                    NotificationType = NotificationTypeEnum.Info,
                    Data = new CheckStatusResponse { Status = BookingStatusEnum.Pending },
                };
            }

            return new ApiResponse<CheckStatusResponse>
            {
                Message = ManagerConstants.INVALID_BOOKING_CODE,
                NotificationType = NotificationTypeEnum.BadRequest,
                Data = new CheckStatusResponse { Status = BookingStatusEnum.Failed },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the check status of the booking code at {Timestamp}. BookingCode: {BookingCode}",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.BookingCode);

            return new ApiResponse<CheckStatusResponse>
            {
                Success = false,
                NotificationType = NotificationTypeEnum.ServerError,
                Message = ManagerConstants.ERROR_CHECKSTATUS
            };
        }
    }


    #region private methods 

    private string DetermineSearchType(SearchRequest request)
    {
        if (String.IsNullOrEmpty(request.DepartureAirport))
            return "HotelOnly";
        if (request.FromDate <= DateTime.Now.AddDays(45) && String.IsNullOrEmpty(request.DepartureAirport))
            return "LastMinuteHotels";
        return "HotelAndFlight";
    }
    private List<Option> PopulateHotelsAndFlightsOptions(List<HotelDTO> hotels, List<FlightDTO> flights)
    {
        List<Option> options = new();

        foreach (var hotel in hotels)
        {
            foreach (var flight in flights)
            {
                var option = new Option
                {
                    OptionCode = Guid.NewGuid().ToString(),
                    HotelCode = hotel.HotelCode.ToString(),
                    FlightCode = flight.FlightCode.ToString(),
                    FlightNumber = flight.FlightNumber,
                    ArrivalAirport = flight.ArrivalAirport,
                    DepartureAirport = flight.DepartureAirport,
                    Price = RandomHelper.GenerateRandomDouble(100, 500),
                    City = hotel.City,
                    HotelName = hotel.HotelName
                };

                options.Add(option);
            }
        }

        return options;
    }
    private async Task<List<HotelDTO>> FetchHotelsAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch hotels. API response: {errorMessage}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<HotelDTO>>(content) ?? new List<HotelDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching hotels from {Url} at {Timestamp}",
                url, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            throw;
        }
    }
    private async Task<List<FlightDTO>> FetchFlightsAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
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
                url, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            throw;
        }

    }
    private async Task<(List<HotelDTO>, List<FlightDTO>)> FetchHotelsAndFlightsAsync(string hotelUrl, string flightUrl)
    {
        var hotelTask = FetchHotelsAsync(hotelUrl);
        var flightTask = FetchFlightsAsync(flightUrl);

        await Task.WhenAll(hotelTask, flightTask);

        return (await hotelTask, await flightTask);
    }
    private static string GenerateBookingCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    #endregion
}
