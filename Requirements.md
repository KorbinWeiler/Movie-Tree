# Movie Tree

## Elevator Pitch

Movie Tree is a movie rating app for _branching_ out, and finding new movies that might be outside of what you usually watch. The intent is to fix the two main problems with traditional movie recommending servies. First is that you tend to fall into a trend with movies that are recommended to you (if you watch a lot of action movies that is all you will see). The second issue is that newer movies get more attention which covers up older gems.

## Audience

This app is for movie lovers, or general audiences that want to find a hidden gem of a movie.

## Use Cases

- Keeping track of movies you have watched and how much you liked them
- Seeing what movies are popular with people right now
- Finding new movies that you might like that may or may not be inline with what you have previously watched

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
- TMDB (The Movie DataBase for getting movie data)

## Technical Requirements

- Server should periodically get an updated movie list from TMDB
- The home page should display a list of current movies that are popular with users, and another list that is random movies picked by AI.
- AI picked movies should be updated periodically (maybe every week?).
- If the user is logged in, the home page should also display a list containing the users "watch later" list.
- Users should be able to search for movies based on title.
- Once a user finds a movie, they should be able to either place it into a watch later list, or rate it out of 10 and leave a review.
- Users should be able to set their reviews to public, private, or friends. The rated score should always be public
- Users should be able to add friends.
- There should be a feed page where recent ratings can be viewed.
- Feed page should have a tab for public rating which should include the ratings of all users, and a friends tab which only displays reviews from the users friends
- There should be a generate movies page where users can generate a list of 10 movies recommended to them.
- When generating recommended movies, the user should be able to choose between using all their movies as data, selecting which movies they want to be used as data, picking a genre, and letting the AI have full reign on deciding.