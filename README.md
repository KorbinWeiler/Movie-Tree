# Movie Tree

Movie Tree is a social movie-rating and discovery web application that helps users explore films beyond their usual tastes. It combines user ratings, social feeds, and scheduled AI-powered discovery lists to surface both popular and under-known titles. The app focuses on transparent, user-driven discovery by letting people save, rate, review, and share movies, while providing an AI-assisted discovery stream.

### Link
https://white-desert-08300781e.6.azurestaticapps.net

## Tech Stack

- **Frontend:** Nuxt (Vue), TypeScript, Vuetify, TailwindCSS
- **Backend:** ASP.NET (C#), Entity Framework Core
- **Hosting:** AzureSQL, Azure Static Web Apps, Azure App Service
- **Third-party APIs:** TMDB for movie metadata; Google Gemini (via Google.GenAI) for AI-generated discovery

## Architecture

- **Web UI (Nuxt):** Client-side SPA communicating with the backend API for auth, data, and generation requests.
- **Backend API (ASP.NET):** REST API implementing authentication, movie CRUD, user actions (watch-later, reviews, friends), and AI orchestration.
- **Data Layer:** EF Core mapping to SQL Server, with migrations and seeded movie/genre data.
- **AI:**  The AI integration uses Google.GenAI (Gemini Developer API) for content generation with Azure Vision and Search for semantic search. 

## Core Features

- Browse and search movies by title with metadata and poster images.
- Track watched movies and add items to a Watch Later list.
- Rate movies on a 1–10 scale; ratings are always public.
- Leave text reviews with visibility controls: Public, Private, or Friends.
- Social features: add friends and view a feed with Public and Friends tabs.
- Generate discovery lists on-demand or view weekly AI-selected discovery movies.
- Generate page with input modes: full history, specific movies, genre, or fully automatic AI selection.

## Admin Features

- Manage movie visibility and moderate reviews (hide/remove inappropriate content).
- Seed and synchronize movie metadata from TMDB.

## Setup & Run (Development)

Prerequisites:
- .NET 10 SDK
- Node.js (for frontend)
- SQL Server or LocalDB

Backend (development):

1. Open a terminal in the `Backend` folder.
2. Restore and build:

```bash
dotnet restore
dotnet build
```

3. Configure environment variables (example `.env` in solution root):

```text
ConnectionStrings__Default=Server=(localdb)\\MSSQLLocalDB;Database=MovieTree;Trusted_Connection=True;TrustServerCertificate=True
Jwt__Key=your-dev-secret-min32chars
GOOGLE_API_KEY=your-gemini-api-key
```

4. Run the backend:

```bash
dotnet run
```

Frontend (development):

1. Open a terminal in the `Frontend` folder.
2. Install and run:

```bash
npm install
npm run dev
```

Notes:
- The backend expects the Google Gemini API key in `GOOGLE_API_KEY` or the `.env` loader used by the project.
- For production, store secrets in a secure store (Azure Key Vault, environment settings on the host).

