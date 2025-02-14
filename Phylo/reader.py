from Bio import Phylo
import matplotlib.pyplot as plt


tree = Phylo.read("tree.nwk", "newick")


fig, ax = plt.subplots(figsize=(12, 12))

Phylo.draw(tree, do_show=False, axes=ax)


fig.savefig("arbol_filogenetico.png", dpi=300)
