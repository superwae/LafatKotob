import pandas as pd
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity

def create_user_feature_matrix(book_data, ratings_data):
    # Merge the ratings and book data on ISBN
    combined_data = pd.merge(ratings_data, book_data, on='ISBN', how='inner')

    # Create dummy variables for genres
    combined_data = pd.get_dummies(combined_data, columns=['Genres'], prefix='', prefix_sep='')

    # Weight the features by the rating
    for genre in combined_data.columns[combined_data.columns.str.contains('Genres_')]:
        combined_data[genre] = combined_data[genre] * combined_data['Rating']

    # Aggregate data by UserID, summing up the genres
    user_features = combined_data.groupby('UserID').sum().drop(columns=['Rating'])

    return user_features

def create_user_profile(user_data, all_genres):
    # Initialize user profile with zeros for all known genres
    user_profile = {genre: 0 for genre in all_genres}
    # Update the profile based on the user's data
    user_profile.update({k: user_data[k] for k in user_data if k in user_profile})
    return user_profile

def compute_user_profile_similarity(new_user_profile, user_profiles):
    # Ensure only numeric columns are used for similarity
    numeric_profiles = user_profiles.select_dtypes(include=[np.number])
    
    # Convert the new_user_profile dictionary to a numpy array
    new_profile_vector = np.array([new_user_profile.get(genre, 0) for genre in numeric_profiles.columns]).reshape(1, -1)
    
    # Convert the DataFrame to a numpy array
    profiles_matrix = numeric_profiles.to_numpy()
    
    # Compute cosine similarity and return
    return cosine_similarity(new_profile_vector, profiles_matrix).flatten()

def get_similar_users(new_user_data, user_profiles, top_n=10):
    new_user_profile = create_user_profile(new_user_data, user_profiles.columns.tolist())
    similarities = compute_user_profile_similarity(new_user_profile, user_profiles)
    top_similar_indices = np.argsort(-similarities)[:top_n]
    return user_profiles.index[top_similar_indices]

def recommend_books(similar_user_ids, ratings_data, books_data, max_results=10, min_rating=4.0, random_samples=5):
    # Filter ratings data for similar user IDs
    filtered_ratings = ratings_data[ratings_data['UserID'].isin(similar_user_ids)]
    
    # Group by ISBN and calculate mean ratings
    recommended_books = filtered_ratings.groupby('ISBN')['Rating'].mean().reset_index()
    
    # Merge with books data for additional details
    recommended_books = recommended_books.merge(books_data, on='ISBN', how='left')
    
    # Filter books with at least the minimum rating
    highly_rated_books = recommended_books[recommended_books['Rating'] >= min_rating]
    
    # Sort books by rating in descending order
    highly_rated_books = highly_rated_books.sort_values(by='Rating', ascending=False).head(max_results)
    
    # Randomly pick books to add diversity
    random_books = recommended_books.sample(n=random_samples, replace=True)

    # Combine both sets of books
    final_recommendations = pd.concat([highly_rated_books, random_books]).drop_duplicates().head(max_results + random_samples)

    return final_recommendations

def recommend(new_user_data):
    book_data = pd.read_csv('collaborative/Filtered_BX_Books.csv')
    ratings_data = pd.read_csv('collaborative/split/train_data.csv')
    
    user_profiles = create_user_feature_matrix(book_data, ratings_data)
    
    similar_user_ids = get_similar_users(new_user_data, user_profiles)

    # Customize the number of results and the minimum rating, including random samples
    top_books = recommend_books(similar_user_ids, ratings_data, book_data, max_results=5, min_rating=4.0, random_samples=5)
    return top_books


