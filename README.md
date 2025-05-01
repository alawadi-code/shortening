# URL Shortener

A full-stack URL shortener application built with .NET 8.0 and React.

## Tech Stack

- Backend: .NET 8.0 Web API
- Frontend: React 18 with TypeScript
- Database: SQL Server with Entity Framework Core 8.0
- ORM: Entity Framework Core

## Project Structure

- `URLShortener.API/` - .NET 8.0 Web API backend
- `urlshortener-client/` - React TypeScript frontend

## API Endpoints

- `POST /api/url/shorten` - Create a new short URL
- `GET /api/url/{shortUrl}` - Redirect to the original URL

## Features

- URL shortening with custom URL support
- Click tracking for shortened URLs
- CORS enabled for frontend-backend communication
- RESTful API architecture
- SQL Server for persistent storage

## Database Management

```bash
# Create migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Reset database
dotnet ef database drop -f --context UrlContext
```
run 
cd URLShortener.API;

cd URLShortener.API; dotnet clean; dotnet restore; dotnet ef database update; dotnet run
 clint do


 cd urlshortener-client;

 cd urlshortener-client; npm install; npm start