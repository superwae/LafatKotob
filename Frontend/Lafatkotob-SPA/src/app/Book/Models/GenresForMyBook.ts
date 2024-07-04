export interface GetGenreForUserModel {
  user_id: string;
  books: BookGenres[];
}

export interface BookGenres {
  genres: string[];
}
