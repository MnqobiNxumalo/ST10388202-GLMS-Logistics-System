# Global Logistics Management System (GLMS)

## Overview
Enterprise-grade logistics management system for contract and service request handling.

## Features
- Contract Management Hub with PDF upload/download
- Service Request Processing with real-time currency conversion (USD → ZAR)
- LINQ-powered search and filtering
- Automated validation (no requests on expired/on-hold contracts)
- Unit testing with xUnit (23+ tests passing)

## Technologies
- ASP.NET Core MVC 8.0
- Entity Framework Core with SQL Server
- xUnit & Moq for testing
- Bootstrap 5 for UI

## Setup Instructions

### Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK
- SQL Server LocalDB

### Run the Application
1. Clone the repository
2. Open `GLMS.sln` in Visual Studio
3. Set `GLMS.Web` as startup project
4. Run `Update-Database` in Package Manager Console
5. Press `F5` to run

## Test Results
All 23 unit tests passing successfully.

## Author
[Your Name]
