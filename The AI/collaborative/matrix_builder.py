import pandas as pd
import numpy as np
from sklearn.decomposition import TruncatedSVD
import os

def build_utility_matrix(filepath):
    ratings_data = pd.read_csv(filepath)
    utility_matrix = ratings_data.pivot(index='UserID', columns='ISBN', values='Rating').fillna(0)
    return utility_matrix

def apply_matrix_factorization(utility_matrix, n_components=10):
    svd = TruncatedSVD(n_components=n_components)
    user_features = svd.fit_transform(utility_matrix)
    item_features = svd.components_
    # Save the matrices
    np.save('collaborative/models/user_features.npy', user_features)
    np.save('collaborative/models/item_features.npy', item_features)
    return user_features, item_features  # Add this line to return the computed matrices


def main():
    # Ensure the correct directory structure exists
    matrix_directory = 'collaborative/models'
    if not os.path.exists(matrix_directory):
        os.makedirs(matrix_directory)
    # Adjust the file path to the training data if necessary
    utility_matrix = build_utility_matrix('data/split/train_data.csv')
    apply_matrix_factorization(utility_matrix)

if __name__ == '__main__':
    main()
