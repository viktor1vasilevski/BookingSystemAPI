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

