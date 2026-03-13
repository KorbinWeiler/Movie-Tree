# Movie Tree — SQL Schema Design

> **Database:** SQL Server  
> **ORM:** Entity Framework Core (code-first, but schema documented here for reference)  
> Last updated: 2026-03-03

---

## Table of Contents

1. [AspNetUsers](#1-aspnetusers-identity-managed)
2. [Friendships](#2-friendships)
3. [Movies](#3-movies)
4. [Genres](#4-genres)
5. [MovieGenres](#5-moviegenres)
6. [WatchLater](#6-watchlater)
7. [Reviews](#7-reviews)
8. [AiPickList](#8-aipicklist)
9. [AiPickListItems](#9-aipicklistitems)
10. [Entity Relationship Overview](#10-entity-relationship-overview)
11. [Enums & Lookup Values](#11-enums--lookup-values)
12. [Indexes](#12-indexes)

---

## 1. AspNetUsers (Identity-managed)

Managed by ASP.NET Identity. Extended with app-specific columns.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `nvarchar(450)` | PK | GUID string (Identity default) |
| `UserName` | `nvarchar(256)` | NOT NULL, UNIQUE | Used as display name |
| `NormalizedUserName` | `nvarchar(256)` | UNIQUE INDEX | Created by Identity |
| `Email` | `nvarchar(256)` | NOT NULL, UNIQUE | |
| `NormalizedEmail` | `nvarchar(256)` | UNIQUE INDEX | Created by Identity |
| `PasswordHash` | `nvarchar(max)` | | Bcrypt via Identity |
| `SecurityStamp` | `nvarchar(max)` | | |
| `ConcurrencyStamp` | `nvarchar(max)` | | |
| `ProfilePictureUrl` | `nvarchar(1024)` | NULL | URL to avatar image |
| `Role` | `int` | NOT NULL, DEFAULT 0 | 0=User, 1=Moderator, 2=Admin |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | |

---

## 2. Friendships

Self-referencing join table. A friendship is always stored once with the lower-ID user as `RequesterId` to simplify duplicate checks.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | PK, IDENTITY | |
| `RequesterId` | `nvarchar(450)` | FK → AspNetUsers.Id, NOT NULL | User who sent the request |
| `AddresseeId` | `nvarchar(450)` | FK → AspNetUsers.Id, NOT NULL | User who received the request |
| `Status` | `int` | NOT NULL, DEFAULT 0 | 0=Pending, 1=Accepted, 2=Declined |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | |
| `UpdatedAt` | `datetime2` | NULL | Set when Status changes |

**Constraints:**
- `UNIQUE (RequesterId, AddresseeId)`
- `CHECK (RequesterId <> AddresseeId)`

---

## 3. Movies

Populated and kept in sync from TMDB by the background sync job. `TmdbId` is the authoritative external key.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | PK, IDENTITY | Internal PK |
| `Title` | `nvarchar(512)` | NOT NULL | |
| `OriginalTitle` | `nvarchar(512)` | NULL | Original language title |
| `Description` | `nvarchar(max)` | NULL | TMDB overview |
| `ReleaseDate` | `date` | NULL | |
| `RuntimeMinutes` | `int` | NULL | |
| `PosterUrl` | `nvarchar(1024)` | NULL | TMDB poster path |

---

## 4. Genres

Populated from TMDB genre list.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | PK | Matches TMDB genre ID |
| `Name` | `nvarchar(64)` | NOT NULL, UNIQUE | e.g. "Action", "Drama" |

---

## 5. MovieGenres

Many-to-many between Movies and Genres.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `MovieId` | `int` | PK (composite), FK → Movies.Id | |
| `GenreId` | `int` | PK (composite), FK → Genres.Id | |

---

## 6. WatchLater

Each row represents one movie on a user's Watch Later list.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | PK, IDENTITY | |
| `UserId` | `nvarchar(450)` | FK → AspNetUsers.Id, NOT NULL | |
| `MovieId` | `int` | FK → Movies.Id, NOT NULL | |
| `AddedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | |

**Constraints:**
- `UNIQUE (UserId, MovieId)` — a movie can only be on the list once per user

---

## 7. Reviews

A user can only review a given movie once. `ReviewText` is nullable so a user can leave a score without writing a review. `ReviewVisibility` applies only to `ReviewText`; `Rating` is always effectively public.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | PK, IDENTITY | |
| `UserId` | `nvarchar(450)` | FK → AspNetUsers.Id, NOT NULL | |
| `MovieId` | `int` | FK → Movies.Id, NOT NULL | |
| `Rating` | `tinyint` | NOT NULL, CHECK (Rating BETWEEN 1 AND 10) | Always public |
| `ReviewText` | `nvarchar(max)` | NULL | Optional written review |
| `ReviewVisibility` | `int` | NOT NULL, DEFAULT 0 | 0=Public, 1=Friends, 2=Private |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | |
| `UpdatedAt` | `datetime2` | NULL | Set on edit |

**Constraints:**
- `UNIQUE (UserId, MovieId)` — one review per user per movie

---

## 8. AiPickList

Stores each generated list of AI-picked discovery movies. A "global" list (for the home page) has `UserId = NULL` and `GenerationMode = 0`. User-generated lists have a `UserId`.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | PK, IDENTITY | |
| `UserId` | `nvarchar(450)` | FK → AspNetUsers.Id, NULL | NULL = global home-page list |
| `GenerationMode` | `int` | NOT NULL | 0=Global, 1=AllHistory, 2=Selected, 3=Genre, 4=FullAI |
| `GenreId` | `int` | FK → Genres.Id, NULL | Populated when Mode=3 |
| `CreatedAt` | `datetime2` | NOT NULL, DEFAULT GETUTCDATE() | |

---

## 9. AiPickListItems

The 10 movies for each `AiPickList` entry.

| Column | Type | Constraints | Notes |
|---|---|---|---|
| `Id` | `int` | PK, IDENTITY | |
| `AiPickListId` | `int` | FK → AiPickList.Id, NOT NULL | |
| `MovieId` | `int` | FK → Movies.Id, NOT NULL | |
| `Position` | `tinyint` | NOT NULL, CHECK (Position BETWEEN 1 AND 10) | Order in the list |

**Constraints:**
- `UNIQUE (AiPickListId, MovieId)`
- `UNIQUE (AiPickListId, Position)`

---

## 10. Entity Relationship Overview

```
AspNetUsers ──< Friendships >── AspNetUsers
AspNetUsers ──< WatchLater >── Movies
AspNetUsers ──< Reviews >── Movies
AspNetUsers ──< AiPickList

Movies ──< MovieGenres >── Genres
Movies ──< AiPickListItems

AiPickList ──< AiPickListItems
AiPickList >── Genres   (nullable, for mode=Genre)
```

---

## 11. Enums & Lookup Values

### ReviewVisibility

| Value | Name | Description |
|---|---|---|
| `0` | Public | Visible to all users |
| `1` | Friends | Visible only to the user's accepted friends |
| `2` | Private | Visible only to the user themselves |

### FriendshipStatus

| Value | Name | Description |
|---|---|---|
| `0` | Pending | Request sent, not yet accepted |
| `1` | Accepted | Both users are friends |
| `2` | Declined | Addressee declined the request |

### UserRole

| Value | Name |
|---|---|
| `0` | User |
| `1` | Moderator |
| `2` | Admin |

### AiGenerationMode

| Value | Name | Description |
|---|---|---|
| `0` | Global | Home-page AI list, no user context |
| `1` | AllHistory | Based on user's full review history |
| `2` | Selected | User picks specific movies as seed data |
| `3` | Genre | User picks a genre |
| `4` | FullAI | AI has full autonomy |

---

## 12. Indexes

| Table | Columns | Type | Reason |
|---|---|---|---|
| `Reviews` | `(MovieId, ReviewVisibility)` | Non-clustered | Feed queries filter by visibility |
| `Reviews` | `(UserId)` | Non-clustered | User profile / your-movies page |
| `Reviews` | `(CreatedAt DESC)` | Non-clustered | Feed sorted by newest |
| `WatchLater` | `(UserId)` | Non-clustered | Home page watch-later section |
| `Movies` | `(TmdbId)` | Unique | TMDB sync lookups |
| `Movies` | `(ReleaseDate)` | Non-clustered | Sort/filter by year |
| `Friendships` | `(AddresseeId, Status)` | Non-clustered | Friends tab feed filter |
| `AiPickList` | `(UserId, CreatedAt DESC)` | Non-clustered | Fetch latest list per user |
