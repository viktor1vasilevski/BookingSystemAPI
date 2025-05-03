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

#### Response
```json:
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
      ...
    ],
    "searchType": 1
  },
  "success": true,
  "message": null,
  "errors": null,
  "notificationType": 0
}


### 🔍 2. Book

**POST** `/api/book/book`  
**Content-Type:** `application/json`

{
  "OptionCode": "d9fffc2a-91bf-4ae3-9710-98d44bdd8569",
  "SearchReq": {
    "Destination": "SKP",
    "DepartureAirport": "CPH",
    "FromDate": "2025-06-10T00:00:00Z",
    "ToDate": "2025-06-15T00:00:00Z"
  }
}

