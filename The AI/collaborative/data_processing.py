# data_processing.py
import pandas as pd
from sklearn.model_selection import train_test_split
import os


def load_book_data(filepath):
    return pd.read_csv(filepath)

def load_ratings_data(filepath):
    data = pd.read_csv(filepath)
    return data.rename(columns={'User-ID': 'UserID', 'Book-Rating': 'Rating'})

def split_data(data, test_size=0.2, random_state=42):
    train_data, test_data = train_test_split(data, test_size=test_size, random_state=random_state)
    return train_data, test_data

def split_and_save_data(data, test_size=0.2, random_state=42):
    train_data, test_data = train_test_split(data, test_size=test_size, random_state=random_state)
    # Create directory for split data if it doesn't exist
    if not os.path.exists('data/split'):
        os.makedirs('data/split')
    # Save the split data
    train_data.to_csv('data/split/train_data.csv', index=False)
    test_data.to_csv('data/split/test_data.csv', index=False)