import pandas as pd
from sqlalchemy import create_engine
import numpy as np
import random
from datetime import datetime

# Database connection string
server = '.'
database = 'Book'
driver = 'ODBC Driver 17 for SQL Server'

# Construct the connection string
connection_string = f'mssql+pyodbc://@{server}/{database}?driver={driver}&trusted_connection=yes'

# Create a SQLAlchemy engine
engine = create_engine(connection_string)

# Read the CSV file
csv_file = 'C:/Users/Wael/Desktop/The AI/newdata/150kBooksDefaultValues_6.csv' 

df = pd.read_csv(csv_file)

# Rename columns to match SQL table fields
df = df.rename(columns={
    'Book-Title': 'Title',
    'Book-Author': 'Author',
    'Year-Of-Publication': 'PublicationDate',
    'Publisher': 'Description',
    'Image-URL-L': 'CoverImage',
    'Genres': 'Type',
    'Page Count': 'PageCount',
    'Language': 'Language'
})

# Fill in missing columns with default or placeholder values
df['Title'] = df['Title'].fillna('Untitled')
df['Author'] = df['Author'].fillna('Unknown')
df['CoverImage'] = df['CoverImage'].fillna('https://example.com/default_cover.jpg')  # Default cover image URL
df['ISBN'] = df['ISBN'].fillna('Unknown')
df['Status'] = 'Available'
df['Language'] = df['Language'].map({'eng': 'English', 'fre': 'French', 'ger': 'German', 'spa': 'Spanish', 'ita': 'Italian'}).fillna('Other')  # Mapping language codes to full names

# Set the Type column to 'sell' or 'buy' randomly
df['Type'] = [random.choice(['sell', 'buy']) for _ in range(len(df))]

# Define userId and historyId values
users = [
    {"id": "e75e44de-d2e6-4843-8489-522d80a0279d", "historyId": 1},
    {"id": "76b83be6-b473-4a34-8477-5884470eb71c", "historyId": 2},
    {"id": "13287e51-6f90-44f1-be3a-65320de49ff1", "historyId": 3},
    {"id": "fca2af6e-98f0-4711-8bca-1ec9714c34a2", "historyId": 4},
    {"id": "01b11d53-5e45-4b01-a714-40546d72eb08", "historyId": 5},
    {"id": "49f5c4ed-cb27-4e9c-bdef-dc122b80f27c", "historyId": 6},
    {"id": "af71063a-810e-4cf4-9885-1e3e68e473de", "historyId": 7},
    {"id": "dce11590-5b19-4b76-99d3-c3e4746fa304", "historyId": 8},
    {"id": "1bd9eee5-f6e1-41d0-a12a-cd622847c5a4", "historyId": 9},

]

# Randomly assign userId and historyId values to 'UserId' and 'HistoryId' columns
random_users = [random.choice(users) for _ in range(len(df))]
df['UserId'] = [user['id'] for user in random_users]
df['HistoryId'] = [user['historyId'] for user in random_users]

# Randomly assign Condition values
conditions = ["Great", "Good", "Bad"]
df['Condition'] = [random.choice(conditions) for _ in range(len(df))]

# Set AddedDate to the current date and time
df['AddedDate'] = datetime.now()

# Assign PartnerUserId
df['PartnerUserId'] = "c98dd230-e629-4325-8194-5157a5d39423"

# Convert data types to match SQL table schema
df['PublicationDate'] = pd.to_datetime(df['PublicationDate'], errors='coerce')
df['PageCount'] = df['PageCount'].astype(float)

# Select columns to match the SQL table structure
df_books = df[['Title', 'Author', 'Description', 'CoverImage', 'UserId', 'HistoryId', 'PublicationDate', 'ISBN', 'PageCount', 'Condition', 'Status', 'Type', 'Language', 'AddedDate', 'PartnerUserId']]

# Insert data into SQL table
df_books.to_sql('Books', con=engine, if_exists='append', index=False)

print("Books data inserted successfully")
