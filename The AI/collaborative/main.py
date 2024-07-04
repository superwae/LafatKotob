from flask import Flask, request, jsonify
from test2 import recommend  # Directly import 'recommend'
import pandas as pd
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity
import random
from concurrent.futures import ThreadPoolExecutor
from flask_caching import Cache
import pandas as pd
import json
import hashlib
from flask_cors import CORS

app = Flask(__name__)
CORS(app)
cache = Cache(app, config={'CACHE_TYPE': 'SimpleCache', 'CACHE_DEFAULT_TIMEOUT': 3000})

def make_cache_key():
    body_data = json.dumps(request.get_json(), sort_keys=True).encode('utf-8')
    return hashlib.md5(body_data).hexdigest()

executor = ThreadPoolExecutor(max_workers=2)
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
def create_user_feature_matrix(book_data, ratings_data):
    combined_data = pd.merge(ratings_data, book_data, on='ISBN', how='inner')
    combined_data = pd.get_dummies(combined_data, columns=['Genres'], prefix='', prefix_sep='')
    for genre in combined_data.columns[combined_data.columns.str.contains('Genres_')]:
        combined_data[genre] = combined_data[genre] * combined_data['Rating']
    user_features = combined_data.groupby('UserID').sum().drop(columns=['Rating'])
    return user_features

def create_user_profile(user_data, all_genres):
    total_count = sum(user_data.values())
    user_profile = {genre: 0 for genre in all_genres}
    for genre, count in user_data.items():
        if genre in user_profile:
            user_profile[genre] = count / total_count if total_count > 0 else 0
    return user_profile


def compute_user_profile_similarity(new_user_profile, user_profiles):
    numeric_profiles = user_profiles.select_dtypes(include=[np.number])
    new_profile_vector = np.array([new_user_profile.get(genre, 0) for genre in numeric_profiles.columns]).reshape(1, -1)
    profiles_matrix = numeric_profiles.to_numpy()
    return cosine_similarity(new_profile_vector, profiles_matrix).flatten()

def get_similar_users(new_user_profile, user_profiles, top_n=10):
    similarities = compute_user_profile_similarity(new_user_profile, user_profiles)
    top_similar_indices = np.argsort(-similarities)[:top_n]
    return user_profiles.index[top_similar_indices]

def load_matrices():
    user_features = np.load('collaborative/models/user_features_50.npy')
    item_features = np.load('collaborative/models/item_features_50.npy')
    return user_features, item_features

def recommend_books(user_features, item_features, user_indices, book_data,genre_counts, top_n=10):
    book_scores = {}
    
    # Aggregate scores for all books across similar users
    for user_index in user_indices:
        test_user_features = user_features[user_index]  # User features for a specific user
        predicted_ratings = np.dot(test_user_features, item_features)
        
        for idx, score in enumerate(predicted_ratings):
            if idx in book_scores:
                book_scores[idx] = max(book_scores[idx], score)
            else:
                book_scores[idx] = score
    
    # Now select the top_n entries with the highest scores
    top_books_indices = sorted(book_scores, key=book_scores.get, reverse=True)[:5]
    recommendations = []
    for index in top_books_indices:
        book_info = book_data.loc[book_data.index == index]
        if not book_info.empty:
            recommendations.append(book_info.to_dict('records')[0])


    total_normalized_weight = sum(genre_counts.values())
    genre_book_counts = {}
    fractional_parts = {}
    if total_normalized_weight > 0:
        for genre, weight in genre_counts.items():
            fractional_share = (weight / total_normalized_weight) * top_n
            genre_book_counts[genre] = int(fractional_share)
            fractional_parts[genre] = fractional_share - genre_book_counts[genre]

    # Ensure the total books count is exactly top_n
    current_total = sum(genre_book_counts.values())
    if current_total < top_n:
        # Distribute remaining books starting from the genre with the highest fractional part
        for genre, fraction in sorted(fractional_parts.items(), key=lambda item: item[1], reverse=True):
            if current_total >= top_n:
                break
            if genre_book_counts[genre] > 0 or fractional_parts[genre] > 0:  # Only add to genres initially considered
                genre_book_counts[genre] += 1
                current_total += 1

    print("Genre book counts:", genre_book_counts)

    for genre, count in genre_book_counts.items():
        # Filter books by genre
        genre_books = book_data[book_data['Genres'].str.contains(genre, case=False, na=False)]

        # If the genre books available are less than the count needed, take as many as available
        selected_books = genre_books.sample(n=min(count, len(genre_books)), replace=False).to_dict('records')
        # Append the randomly selected books to the recommendations list
        recommendations.extend(selected_books)

    # Ensure the recommendations are unique in case there's any overlap in genre categorization
    recommendations = [dict(t) for t in {tuple(d.items()) for d in recommendations}]
    
    return recommendations
def normalize_book_data(books):
    normalized_books = []
    for book in books:
        # Ensure input can be converted to dictionary if it's a pd.Series
        if isinstance(book, pd.Series):
            book = book.to_dict()
        elif not isinstance(book, dict):
            # If book is neither a pd.Series nor a dict, log an error and skip it
            print(f"Skipping invalid book data: {book}")
            continue
        
        # Use .get() method safely by confirming the dictionary
        normalized_book = {
            'ISBN': book.get('ISBN', 'N/A'),
            'Book-Title': book.get('Book-Title', 'No Title'),
            'Book-Author': book.get('Book-Author', 'Unknown Author'),
            'Year-Of-Publication': book.get('Year-Of-Publication', 'N/A'),
            'Genres': book.get('Genres', 'No Genre'),
            'Page Count': book.get('Page Count', 0)
        }
        normalized_books.append(normalized_book)
    return normalized_books



@app.route('/recommend', methods=['POST'])
@cache.cached(key_prefix=make_cache_key)
def recommend_api():  # This function handles the API request
    try:
        data = request.get_json()
        cache_key = str(data)
        book_data = pd.read_csv('collaborative/Filtered_BX_Books.csv')
        ratings_data = pd.read_csv('collaborative/split/train_data.csv')
        user_profiles = create_user_feature_matrix(book_data, ratings_data)

     
        # Count genres from provided books
        new_user_genres = {}
        for book in data['books']:
            for genre in book['genres']:
                if genre in genre_mapping:
                    mapped_genres = genre_mapping[genre]
                    for mg in mapped_genres:
                        new_user_genres[mg] = new_user_genres.get(mg, 0) + 1
                else:
                    new_user_genres[genre] = new_user_genres.get(genre, 0) + 1

        print("Initial genre counts:", new_user_genres)  
        # Normalize the genre counts
        max_genre_count = max(new_user_genres.values(), default=1)
        if max_genre_count == 0:
            max_genre_count = 1 

        normalized_genres = {k: (v / max_genre_count) * 10 for k, v in new_user_genres.items()}
        all_genres = user_profiles.columns.tolist()
        new_user_profile = create_user_profile(normalized_genres, all_genres)
        
        similar_user_ids = get_similar_users(new_user_profile, user_profiles)
        similar_user_indices = [user_profiles.index.get_loc(id) for id in similar_user_ids if id in user_profiles.index]

        user_features, item_features = load_matrices()

        future1 = executor.submit(recommend_books, user_features, item_features, similar_user_indices, book_data, new_user_genres)
        future2 = executor.submit(recommend, normalized_genres)

        recommendations1 = future1.result()
        recommendations2 = future2.result()

        # Get recommendations based on the genre profile
        #pick 15 random books from the recommended_books1 recommended_books2 

        normalized_books1 = normalize_book_data(recommendations1)
        normalized_books2 = normalize_book_data(recommendations2)
        all_recommended_books = {frozenset(book.items()): book for book in normalized_books1 + normalized_books2}.values()
        unique_books = random.sample(list(all_recommended_books), min(15, len(all_recommended_books)))
        return jsonify(unique_books)
    except Exception as e:
        print("Error:", e)  # Print error to the console
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True)
