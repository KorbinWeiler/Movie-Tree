# Movie Tree

## Elevator Pitch

Movie Tree is a movie rating app designed to help users _branch_ out and discover films outside their usual preferences. It addresses two common issues with traditional recommendation systems: users can get stuck in repetitive recommendation loops (for example, watching mostly action movies leads to mostly action recommendations), and newer movies often receive more visibility than older ones.

## Audience

This app is for movie lovers and general audiences who want to expand their range of movies.

## Use Cases

- Track movies you have watched and how much you liked them
- View movies that are currently popular with users
- Discover new movies that may or may not align with your usual viewing history

## Tech Stack

### Frontend

- Nuxt
- Vue
- Vuetify
- Typescript
- TailwindCSS

### Backend

- ASP.net
- EntityFramework

### Other

- SQL Server
- TMDB (The Movie Database, used as the movie metadata source)

## Technical Requirements

- The server shall synchronize movie data from TMDB on a recurring schedule.
- The home page shall display:
	- a list of movies currently popular with users, and
	- a separate list of AI-selected discovery movies.
- AI-selected discovery movies shall refresh on a recurring schedule (target: weekly).
- If a user is logged in, the home page shall also display that user's Watch Later list.
- Users shall be able to search for movies by title.
- After finding a movie, users shall be able to either:
	- add it to their Watch Later list, or
	- rate it on a 1-10 scale and leave a review.
- Users shall be able to set review visibility to Public, Private, or Friends.
- Numeric rating scores shall always be public, regardless of review visibility.
- Users shall be able to add friends.
- The app shall include a Feed page for viewing recent ratings.
- The Feed page shall include:
	- a Public tab with ratings from all users, and
	- a Friends tab that only displays reviews from a user's friends.
- The app shall include a Generate Movies page where users can generate a list of 10 recommended movies.
- When generating recommendations, users shall be able to choose one of the following input modes:
	- use all of their movie history,
	- select specific movies to use as input,
	- select a genre, or
	- use fully automatic AI selection.