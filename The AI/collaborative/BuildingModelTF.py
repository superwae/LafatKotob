import tensorflow as tf
import numpy as np
import pandas as pd
from sklearn.model_selection import KFold
import matplotlib.pyplot as plt

class NMFRegularized(tf.keras.Model):
    def __init__(self, num_users, num_items, n_components, l1_reg=0.1):
        super(NMFRegularized, self).__init__()
        self.W = tf.Variable(tf.random.normal([num_users, n_components], stddev=0.1), trainable=True)
        self.H = tf.Variable(tf.random.normal([n_components, num_items], stddev=0.1), trainable=True)
        self.l1_reg = l1_reg

    def call(self, inputs):
        prediction = tf.linalg.matmul(self.W, self.H)
        return prediction
    
def create_global_mappings(data):
    user_mapping = {id: i for i, id in enumerate(data['UserID'].unique())}
    item_mapping = {id: i for i, id in enumerate(data['ISBN'].unique())}
    return user_mapping, item_mapping

def create_utility_matrix(data, user_mapping, item_mapping):
    num_users = len(user_mapping)
    num_items = len(item_mapping)
    utility_matrix = np.zeros((num_users, num_items))

    # Map user IDs and ISBNs to their corresponding indices
    user_indices = data['UserID'].map(user_mapping)
    item_indices = data['ISBN'].map(item_mapping)

    # Check for any IDs that weren't in the mapping (i.e., NaN values after mapping)
    if user_indices.isnull().any() or item_indices.isnull().any():
        raise ValueError("Unseen UserID or ISBN found in the dataset.")

    utility_matrix[user_indices.values, item_indices.values] = data['Rating'].values
    return tf.convert_to_tensor(utility_matrix, dtype=tf.float32)

data = pd.read_csv('collaborative/split/train_data.csv')
user_mapping, item_mapping = create_global_mappings(data)

def cross_validate(data, n_splits, n_components_list, l1_reg_list, epochs):
    kf = KFold(n_splits=n_splits, shuffle=True, random_state=42)
    results = []
    for train_index, test_index in kf.split(data):
        train_data = data.iloc[train_index]
        test_data = data.iloc[test_index]
        
        train_matrix = create_utility_matrix(train_data, user_mapping, item_mapping)
        test_matrix = create_utility_matrix(test_data, user_mapping, item_mapping)

        for n_components in n_components_list:
            for l1_reg in l1_reg_list:
                model = NMFRegularized(num_users=train_matrix.shape[0], num_items=train_matrix.shape[1], n_components=n_components, l1_reg=l1_reg)
                optimizer = tf.keras.optimizers.Adam()
                loss_fn = tf.keras.losses.MeanSquaredError()

                for epoch in range(epochs):
                    with tf.GradientTape() as tape:
                        preds = model(train_matrix)
                        base_loss = loss_fn(train_matrix, preds)
                        l1_loss = l1_reg * (tf.reduce_sum(tf.abs(model.W)) + tf.reduce_sum(tf.abs(model.H)))
                        loss = base_loss + l1_loss

                    grads = tape.gradient(loss, model.trainable_variables)
                    if not grads or any(g is None or tf.reduce_any(tf.math.is_nan(g)) for g in grads):
                        print(f"Epoch {epoch+1}: Failed to compute gradients or found NaN.")
                        print(f"Predictions: {preds}")
                        print(f"Loss: {loss.numpy()}")
                        continue  # Skip applying gradients if they're None or NaN

                    optimizer.apply_gradients(zip(grads, model.trainable_variables))
                
                test_preds = model(test_matrix)
                test_loss = loss_fn(test_matrix, test_preds)
                
                results.append({'n_components': n_components, 'l1_reg': l1_reg, 'train_loss': loss.numpy(), 'test_loss': test_loss.numpy()})

    return results


def plot_results(results):
    components = [r['n_components'] for r in results]
    train_losses = [r['train_loss'] for r in results]
    test_losses = [r['test_loss'] for r in results]
    l1_regs = [r['l1_reg'] for r in results]

    plt.figure(figsize=(12, 6))
    for l1_reg in set(l1_regs):
        subset = [r for r in results if r['l1_reg'] == l1_reg]
        plt.plot([r['n_components'] for r in subset], [r['test_loss'] for r in subset], label=f'L1 Reg={l1_reg}')
    plt.xlabel('Number of Components')
    plt.ylabel('Test Loss')
    plt.title('Test Loss vs Number of Components for Different L1 Regularizations')
    plt.legend()
    plt.grid(True)
    plt.show()

# Example usage:
data = pd.read_csv('collaborative/split/train_data.csv')
results = cross_validate(data, n_splits=5, n_components_list=[5, 10], l1_reg_list=[0.01, 0.05], epochs=10)
plot_results(results)