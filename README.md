# 🧳 Mock Booking System API


This is a mock travel booking system backend built in .NET 10, designed to simulate a hotel and flight search, booking, and status-checking process. It supports asynchronous operations and real-time status updates via SignalR.

---

## ✨ Features

- Search for hotels or combined hotel+flight packages
- Book selected options with simulated processing time
- Real-time status updates using SignalR
- In-memory storage (no external DB)
- Clean architecture and SOLID principles

---

## 🚀 Technologies Used

- .NET 10 Web API
- SignalR (for real-time booking status)
- C#
- In-memory caching
- RESTful principles
- Asynchronous programming

---

## 📬 API Endpoints & Example Requests

### 🔍 1. Search

**POST** `/api/search/search`  
**Content-Type:** `application/json`

#### Request:
```json
{
  "Destination": "SKP",
  "DepartureAirport": "CPH",
  "FromDate": "2025-06-10T00:00:00Z",
  "ToDate": "2025-06-15T00:00:00Z"
}
```
#### Response:
```json
{
  "data": {
    "options": [
      {
        "optionCode": "6c3fde2c-4032-4ece-aebb-3f5afe798998",
        "hotelCode": "8626",
        "flightCode": "306",
        "flightNumber": "OS 306",
        "arrivalAirport": "SKP",
        "departureAirport": "CPH",
        "price": 442.56,
        "hotelName": "Alexandar Square Boutique Hotel",
        "city": "Skopje"
      },
      {
        "optionCode": "f8c9051c-6ae5-4a81-bf7b-89e55b2dfa2d",
        "hotelCode": "8627",
        "flightCode": "306",
        "flightNumber": "OS 306",
        "arrivalAirport": "SKP",
        "departureAirport": "CPH",
        "price": 337.25,
        "hotelName": "Skopje Marriott Hotel",
        "city": "Skopje"
      }
    ],
    "searchType": 1
  },
  "success": true,
  "message": null,
  "errors": null,
  "notificationType": 0
}
```


### 🔍 2. Book

**POST** `/api/book/book`  
**Content-Type:** `application/json`
```json
{
  "OptionCode": "d9fffc2a-91bf-4ae3-9710-98d44bdd8569",
  "SearchReq": {
    "Destination": "SKP",
    "DepartureAirport": "CPH",
    "FromDate": "2025-06-10T00:00:00Z",
    "ToDate": "2025-06-15T00:00:00Z"
  }
}
```

#### Response:
```json
{
  "Data": {
    "BookingCode": "A1b2C3",
    "BookingTime": "2025-02-24T12:34:56Z"
  },
  "Success": true,
  "Message": null,
  "Errors": null,
  "NotificationType": 0
}
```


### 🔍 3. Check Status

**POST** `/api/checkstatus/checkstatus?BookingCode=A1b2C3`  
**Content-Type:** `application/json`
#### Response (Before SleepTime Elapses):
```json
{
  "Data": {
    "Status": 2
  },
  "Message": "Booking is pending, please wait while we confirm your details.",
  "Errors": null,
  "NotificationType": 4
}
```

#### Response (After SleepTime Elapses):
```json
{
  "Status": 0,
  "Message": "Booking completed successfully!"
}
```

📌 Note: The booking status is sent to the frontend via SignalR. These examples show how the data flows internally, but the client receives real-time updates using WebSockets.

🛫 How to Use (Search Tips)
Destination is a 3-letter airport/city code like:

SKP → Skopje

BCN → Barcelona

DXB → Dubai

DepartureAirport is optional and used for flight searches:

CPH → Copenhagen

OSL → Oslo

STO → Stockholm

If you include DepartureAirport, the system performs a HotelAndFlight search. If not, it's a HotelOnly search. If FromDate is within the next 45 days, it becomes a LastMinuteHotel search (and those bookings always return Failed).
