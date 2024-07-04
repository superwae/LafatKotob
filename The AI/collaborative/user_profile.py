import pandas as pd
import numpy as np  
from sklearn.metrics.pairwise import cosine_similarity


def create_user_feature_matrix(book_data, ratings_data):
    # Correctly rename columns to standardize them across your script
    ratings_data = ratings_data.rename(columns={'User-ID': 'UserID', 'Book-Rating': 'Rating'})

    # Ensure the ISBN columns are of the same type to avoid merging issues
    book_data['ISBN'] = book_data['ISBN'].astype(str)
    ratings_data['ISBN'] = ratings_data['ISBN'].astype(str)

    # Merge the ratings and book data
    combined_data = pd.merge(ratings_data, book_data, on='ISBN', how='inner')

    # Create dummy variables for genres
    # Include UserID in the dummies to ensure it is not lost
    combined_data.set_index('UserID', inplace=True)
    features = pd.get_dummies(combined_data['Genres'])

    # Weight the features by the rating
    weighted_features = features.multiply(combined_data['Rating'], axis=0)

    # Group by UserID and sum to consolidate user preferences
    user_features = weighted_features.groupby('UserID').sum()
    
    return user_features
def create_user_profile(new_user_data, all_genres):
    # Initialize the profile with all genres set to 0
    user_profile = {genre: 0 for genre in all_genres}
    # Update the profile with the new user data
    user_profile.update(new_user_data)
    return user_profile


def compute_user_profile_similarity(new_user_profile, user_profiles):
    # Assuming `user_profiles` is a DataFrame, not a dictionary.
    # Convert the new_user_profile dictionary to a numpy array
    new_profile_vector = np.array(list(new_user_profile.values())).reshape(1, -1)
    
    # Convert the DataFrame to a numpy array. Assuming user_profiles is already suitable for this operation.
    profiles_matrix = user_profiles.to_numpy()  # Use to_numpy() if user_profiles is a DataFrame

    # Compute cosine similarity and return
    return cosine_similarity(new_profile_vector, profiles_matrix).flatten()
