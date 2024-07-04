import pandas as pd
from sqlalchemy import create_engine, text

# Database connection string
server = '.'
database = 'Book'
driver = 'ODBC Driver 17 for SQL Server'

# Construct the connection string
connection_string = f'mssql+pyodbc://@{server}/{database}?driver={driver}&trusted_connection=yes'

# Create a SQLAlchemy engine
engine = create_engine(connection_string)

# Read the CSV file again to get the genres
csv_file = 'C:/Users/Wael/Desktop/The AI/newdata/150kBooksDefaultValues_6.csv'
df = pd.read_csv(csv_file)

# Retrieve the inserted books with their IDs
query = text("SELECT Id, ISBN FROM Books")
with engine.connect() as conn:
    result = conn.execute(query)
    books = result.fetchall()

# Mapping genres to predefined genre IDs
genre_mapping = {
    'History': ['historical', 'american history', 'world war historical accounts','historical and educational','historical romance'],
    'Romance': ['historical romance', 'contemporary romance', 'romantic fiction'],
    'Science Fiction': ['science fiction and fantasy', 'science fiction'],
    'Fantasy': ['fantasy and magic', 'general fiction and fantasy'],
    'Thriller': ['thrillers and suspense', 'mystery and detective stories'],
    'Young Adult': ['young adult fiction', 'children’s literature and animals'],
    'Children': ['children’s adventure and friendship', 'juvenile fiction'],
    'Science': ['science', 'psychology and self-help'],
    'Horror': ['horror and supernatural'],
    'Nonfiction': ['nonfiction', 'biography and autobiography'],
    'Health': ['health'],
    'Travel': ['travel and exploration'],
    'Cooking': ['cooking and food'],
    'Art': ['art'],
    'Religion': ['religion and spirituality', 'christian literature'],
    'Philosophy': ['philosophy'],
    'Education': ['language and education', 'educational literature'],
    'Politics': ['politics'],
    'Business': ['business and management'],
    'Technology': ['technology'],
    'True Crime': ['true crime and murder'],
    'Drama': ['drama and plays'],
    'Adventure': ['adventure stories'],
    'Nature': ['nature'],
    'Humor': ['humor and cartoons'],
    'Lifestyle': ['cooking and food','games & activities'],
    'Economics': ['economics'],
    'Astronomy': ['astronomy'],
    'Linguistics': ['linguistics'],
    'Literature': ['literature', 'general fiction and literature'],
    'Short Story': ['short stories'],
    'Novel': ['comics and graphic novels'],
    'Medicine': ['medicine'],
    'Psychology': ['psychology'],
    'Anime': ['comics and graphic novels'],
    'Poetry': ['poetry and poetic works'],
    'Sports': ['sports'],
    'Comics': ['comics and graphic novels']
}

genre_ids = {
    'History': 1,
    'Romance': 2,
    'Science Fiction': 3,
    'Fantasy': 4,
    'Thriller': 5,
    'Young Adult': 6,
    'Children': 7,
    'Science': 8,
    'Horror': 9,
    'Nonfiction': 10,
    'Health': 11,
    'Travel': 12,
    'Cooking': 13,
    'Art': 14,
    'Comics': 38,
    'Religion': 15,
    'Philosophy': 16,
    'Education': 17,
    'Politics': 18,
    'Business': 19,
    'Technology': 20,
    'True Crime': 21,
    'Poetry': 36,
    'Drama': 22,
    'Adventure': 23,
    'Nature': 24,
    'Humor': 25,
    'Lifestyle': 26,
    'Economics': 27,
    'Astronomy': 28,
    'Linguistics': 29,
    'Literature': 30,
    'Short Story': 31,
    'Novel': 32,
    'Medicine': 33,
    'Psychology': 34,
    'Anime': 35,
    'Sports': 37
}

# Prepare data for BookGenres table
book_genres_data = []

# Create a dictionary from the CSV for quick lookup
isbn_genre_dict = df.set_index('ISBN')['Genres'].to_dict()

for book in books:
    book_id = book[0]
    isbn = book[1]
    if isbn in isbn_genre_dict:
        book_genres = isbn_genre_dict[isbn].split(',')
        for genre in book_genres:
            for main_genre, sub_genres in genre_mapping.items():
                if genre.strip().lower() in sub_genres:
                    book_genres_data.append({'BookId': book_id, 'GenreId': genre_ids[main_genre]})

# Insert data into BookGenres table
df_book_genres = pd.DataFrame(book_genres_data)
df_book_genres.to_sql('BookGenres', con=engine, if_exists='append', index=False)

print("BookGenres data inserted successfully")
