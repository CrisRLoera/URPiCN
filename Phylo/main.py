import random
from Bio import Phylo
from Bio.Phylo.TreeConstruction import DistanceTreeConstructor, DistanceMatrix
import matplotlib.pyplot as plt

# Lista de especies de ejemplo
species = ["A", "B", "C", "D", "E"]

# Generar una matriz de distancias aleatoria
matrix = []
for i in range(len(species)):
    row = [0 if i == j else random.uniform(0.1, 1.0) for j in range(i+1)]
    matrix.append(row)

# Crear la matriz de distancia
distance_matrix = DistanceMatrix(species, matrix)

# Construir el árbol usando el método UPGMA
constructor = DistanceTreeConstructor()
tree = constructor.upgma(distance_matrix)

# Graficar el árbol
fig, ax = plt.subplots(figsize=(6, 6))
Phylo.draw(tree, axes=ax)
#plt.show()
fig.savefig("arbol_filogenetico.png")
