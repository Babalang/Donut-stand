import json
import sys

def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)

def state_to_key(state):
    return json.dumps(state, sort_keys=True, ensure_ascii=False)

def build_child_map(arbre):
    child_map = {}
    for node in arbre:
        parent = node["parent"]
        if parent is not None:
            child_map.setdefault(parent, []).append(node["id"])
    return child_map

def find_leaf_nodes(arbre, child_map):
    all_ids = set(node["id"] for node in arbre)
    non_leaf = set(child_map.keys())
    return list(all_ids - non_leaf)

def build_id_map(arbre):
    return {node["id"]: node for node in arbre}

def dicts_equivalent(d1, d2):
    """Vérifie que d1 est inclus dans d2 (récursif, pour états partiels)."""
    for k, v in d1.items():
        if k not in d2:
            return False
        if isinstance(v, dict) and isinstance(d2[k], dict):
            if not dicts_equivalent(v, d2[k]):
                return False
        elif isinstance(v, list) and isinstance(d2[k], list):
            if v != d2[k]:
                return False
        else:
            if v != d2[k]:
                return False
    return True

def states_equivalent(state_arbre, state_start):
    # Vérifie que le bras ne tient rien dans l'état de l'arbre
    if "arm" in state_arbre and state_arbre["arm"].get("holding", True):
        return False
    # Vérifie que la section "forme" de l'état de départ est incluse dans celle de l'arbre
    if "forme" in state_start and "forme" in state_arbre:
        return dicts_equivalent(state_start["forme"], state_arbre["forme"])
    return False

def find_node_by_state(arbre, start_state):
    for i in range(len(arbre)):
        node = arbre[i]
        if states_equivalent(node["state"], start_state):
            return node["id"]
    return None

def path_to_root(arbre, start_id):
    id_map = build_id_map(arbre)
    path = []
    current = start_id
    while current is not None:
        path.append(current)
        current = id_map[current]["parent"]
    path = path[::-1]  # Pour avoir de la racine à start_id
    return path


def find_path(arbre, start_id, goal_id):
    id_map = build_id_map(arbre)
    path = []
    current = goal_id
    while current is not None:
        path.append(current)
        if current == start_id:
            break
        current = id_map[current]["parent"]
    path = path[::-1]
    if path and path[0] == start_id:
        return path
    return None

def inverse_action(action):
    if action is None:
        return None
    if action.startswith("pickup("):
        obj = action[len("pickup("):-1]
        return f"drop({obj})"
    if action.startswith("drop("):
        obj = action[len("drop("):-1]
        return f"pickup({obj})"
    # Ajoute ici d'autres inversions si besoin (moveTo, push, roll, etc.)
    return action

def invert_path(path, id_map):
    """Prend une liste d'IDs de noeuds (path) et retourne la liste inversée des actions."""
    inverted_actions = []
    for node_id in reversed(path):
        action = id_map[node_id]["action"]
        if action:
            inverted_actions.append(inverse_action(action))
    return inverted_actions

def main():
    if len(sys.argv) < 3:
        print("Usage: python search.py arbre.json start_state.json")
        sys.exit(1)
    arbre_file = sys.argv[1]
    start_state_file = sys.argv[2]

    arbre = load_json(arbre_file)
    start_state = load_json(start_state_file)
    
    goal_id = max(node["id"] for node in arbre)  # le but est la feuille avec l'ID le plus élevé

    child_map = build_child_map(arbre)
    leaf_nodes = find_leaf_nodes(arbre, child_map)
    id_map = build_id_map(arbre)

    start_id = find_node_by_state(arbre, start_state)
    if start_id is None:
        print("Position de départ non trouvée dans l'arbre.")
        sys.exit(1)
    else:
        print("Position de départ trouvée avec l'ID :", start_id)
    
    finalpath = []

    # 1. Chemin de start_id à la racine (inversé)
    path_to_root_ids = path_to_root(arbre, start_id)
    finalpath.extend(invert_path(path_to_root_ids, id_map))

    # 2. Chemin de la racine à l'objectif (dans l'ordre)
    root_id = path_to_root_ids[0]
    path_root_to_goal = find_path(arbre, root_id, goal_id)
    if path_root_to_goal:
        # On saute le premier (racine) car déjà inclus dans le chemin précédent
        for node_id in path_root_to_goal[1:]:
            action = id_map[node_id]["action"]
            if action:
                finalpath.append(action)
        print("Séquence complète d'actions :")
        for act in finalpath:
            print(act)
    else:
        print("Aucun chemin trouvé de la racine à l'objectif.")

if __name__ == "__main__":
    main()