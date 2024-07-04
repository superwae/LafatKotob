import pandas as pd
import requests
import json
import numpy as np  # For scaling

# Define the base URL of the Flask API
API_URL = 'http://127.0.0.1:5000/recommend'

# Load the test data and book data
test_data = pd.read_csv('split/test_data.csv')
book_data = pd.read_csv('Filtered_BX_Books.csv')

# Ensure the ISBN is a string to avoid any data type issues
book_data['ISBN'] = book_data['ISBN'].astype(str)
test_data['ISBN'] = test_data['ISBN'].astype(str)

# Join the test data with book data to get genres for the books each user rated
test_data_with_genres = pd.merge(test_data, book_data[['ISBN', 'Genres']], on='ISBN', how='left')

# Process each unique user in the test data
for user_id in test_data_with_genres['UserID'].unique():
    # Filter data for the current user
    user_data = test_data_with_genres[test_data_with_genres['UserID'] == user_id]
    
    # Initialize genre counts
    genre_counts = {genre: 0 for genre in set(','.join(book_data['Genres'].dropna()).split(','))}

    # Count genres from user data
    total_genres = 0
    for _, row in user_data.iterrows():
        genres = row['Genres'].split(',')
        for genre in genres:
            genre_counts[genre] += 1
            total_genres += 1

    # Convert genre counts to a list format expected by the API
    genre_list = [{"genres": [genre]} for genre, count in genre_counts.items() if count > 0]
    print(f"User {user_id} genres: {genre_list}")
    # Prepare the JSON payload for the POST request
    request_payload = json.dumps({
        "user_id": int(user_id),
        "books": genre_list
    })
    print(f"payload: {request_payload}")

    # Send the POST request and wait for response
    response = requests.post(API_URL, data=request_payload, headers={'Content-Type': 'application/json'})
    if response.status_code == 200:
        print(f"User {user_id} Recommendations:")
        print(response.json())
    else:
        print(f"Failed to fetch recommendations for user {user_id} with status {response.status_code}. Response content: {response.text}")
