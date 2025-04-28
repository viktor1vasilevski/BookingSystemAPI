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

public class ManagerService(HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ManagerService> logger,
    IHubContext<BookingHub> hubContext, ITravelApiClient travelApiClient) : IManagerService
{
    private static readonly Dictionary<string, BookingDTO> _bookings = new();
    private static readonly Random _random = new();

    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ManagerService> _logger = logger;
    private readonly IHubContext<BookingHub> _hubContext = hubContext;
    private readonly ITravelApiClient _travelApiClient = travelApiClient;


    public async Task<ApiResponse<SearchResponse>> SearchAsync(SearchRequest request)
    {
        try
        {
            var options = new List<Option>();
            var searchType = DetermineSearchType(request);

            if (searchType == SearchTypeEnum.HotelOnly.ToString() || searchType == SearchTypeEnum.LastMinuteHotels.ToString())
            {
                var hotels = await _travelApiClient.FetchHotelsAsync(request.Destination);
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
                var (hotels, flights) = await _travelApiClient.FetchHotelsAndFlightsAsync(request.Destination, request.DepartureAirport);
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

        if (request.SearchRequest.FromDate >= DateTime.UtcNow && request.SearchRequest.FromDate <= DateTime.UtcNow.AddDays(45))
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
    private static string GenerateBookingCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    #endregion
}
