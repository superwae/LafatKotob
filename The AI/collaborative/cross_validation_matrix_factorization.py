import pandas as pd
import numpy as np
from sklearn.decomposition import NMF
from numpy.linalg import norm
from sklearn.model_selection import KFold
import matplotlib.pyplot as plt
import os
from scipy.sparse import csr_matrix
from pandas.api.types import CategoricalDtype

def build_utility_matrix(data, all_users, all_isbns):
    data = data.copy()
    data['UserID'] = data['UserID'].astype('category')
    data['ISBN'] = data['ISBN'].astype('category')
    row_ind = data['UserID'].cat.codes
    col_ind = data['ISBN'].cat.codes
    data_array = data['Rating'].values
    utility_matrix = csr_matrix((data_array, (row_ind, col_ind)), shape=(len(all_users), len(all_isbns)))
    return utility_matrix

def apply_matrix_factorization(utility_matrix, n_components, beta_loss='frobenius'):
    # Initialize the NMF model
    model = NMF(n_components=n_components, init='nndsvd', beta_loss=beta_loss, random_state=0, max_iter=500)
    # Fit the model and transform the data
    W = model.fit_transform(utility_matrix)
    H = model.components_
    # Calculate the training reconstruction error manually if needed
    train_reconstruction_error = model.reconstruction_err_
    return W, H, model, train_reconstruction_error



def calculate_reconstruction_error(utility_matrix, W, H):
    reconstructed_matrix = np.dot(W, H)
    error = norm(utility_matrix.toarray() - reconstructed_matrix)
    return error

def cross_validate(filepath, n_splits, n_components_list, save_dir='plots'):
    ratings_data = pd.read_csv(filepath)
    all_isbns = ratings_data['ISBN'].unique()
    all_users = ratings_data['UserID'].unique()

    kf = KFold(n_splits=n_splits, shuffle=True, random_state=42)

    if not os.path.exists(save_dir):
        os.makedirs(save_dir)

    results = []

    for n_components in n_components_list:
        train_errors, test_errors, variances = [], [], []

        for train_index, test_index in kf.split(all_users):
            train_users = all_users[train_index]
            test_users = all_users[test_index]

            train_data = ratings_data[ratings_data['UserID'].isin(train_users)]
            test_data = ratings_data[ratings_data['UserID'].isin(test_users)]

            train_matrix = build_utility_matrix(train_data, all_users, all_isbns)
            test_matrix = build_utility_matrix(test_data, all_users, all_isbns)

            W, H, model, train_reconstruction_error = apply_matrix_factorization(train_matrix, n_components)
            test_error = calculate_reconstruction_error(test_matrix, W, H)

            train_errors.append(train_reconstruction_error)
            test_errors.append(test_error)
            variances.append(model.reconstruction_err_)

        avg_train_error = np.mean(train_errors)
        avg_test_error = np.mean(test_errors)
        avg_variance = np.mean(variances)
        results.append((n_components, avg_train_error, avg_test_error, avg_variance))

        print(f"n_components={n_components}, Avg Train Error: {avg_train_error}, Avg Test Error: {avg_test_error}, Avg Variance: {avg_variance}")

    # Plotting
    plt.figure(figsize=(14, 7))
    components, train_errs, test_errs, _ = zip(*results)
    plt.plot(components, train_errs, label='Train Error', marker='o')
    plt.plot(components, test_errs, label='Test Error', marker='o')
    plt.title('Training vs. Testing Reconstruction Error Across Different Components')
    plt.xlabel('Number of Components')
    plt.ylabel('Reconstruction Error')
    plt.legend()
    plt.savefig(os.path.join(save_dir, 'reconstruction_errors.png'))
    plt.close()

if __name__ == "__main__":
    filepath = 'collaborative/split/train_data.csv'
    cross_validate(filepath, n_splits=5, n_components_list=[1,2, 3,4, 5, 10, 15,25,50,75,100])
