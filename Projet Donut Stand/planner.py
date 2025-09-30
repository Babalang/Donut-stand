import json
from collections import deque
from copy import deepcopy
import pprint
import time

from actions import Action

def load_json(path):
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)
    
def state_to_key(state):
    """Convertir un état en clé sérialisée pour visited set"""
    return json.dumps(state, sort_keys=True, ensure_ascii=False)

def is_goal(state, final_state):
    return json.dumps(state, sort_keys=True) == json.dumps(final_state, sort_keys=True)

def generate_actions(action_obj, state):
    actions = []
    objets = list(state["forme"].keys())


    # drop
    for obj in objets:
        
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.drop(obj)
            if new_state:
                actions.append((new_state, f"drop({obj})"))
        except Exception:
            pass

    # moveTo ,(0,-1), (0,1), (-1,0), (1,0) Les coins : (1,1),(-1,1),(-1,-1)
    positions = [(0,0),(1,-1)]
    for pos in positions:
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.moveTo(pos)
            if new_state:
                actions.append((new_state, f"moveTo{pos}"))
        except Exception:
            pass

    # pickup
    for obj in objets:
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.pickup(obj)

            if new_state:   # filtrer {}
                
                actions.append((new_state, f"pickup({obj})"))
        except Exception:
            pass

    # push
    for obj in objets:
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.push(obj)
            if new_state:
                actions.append((new_state, f"push({obj})"))
        except Exception:
            pass
    '''
    # roll
    for obj in objets:
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.roll(obj, 1)
            if new_state:
                actions.append((deepcopy(new_state), f"roll({obj},1)"))
        except Exception:
            pass
        
    # print(len(actions))'''

    return actions



def bfs_planner(base_file="test_base.json", final_file="test_base.json"):
    arbre_file = "arbre.json"
    action_obj = Action(base_file)
    init_state = action_obj.world_state

    #print("init_state",init_state)

    final_a_obj = Action(final_file)
    final_state = final_a_obj.world_state

    #pprint.pprint(init_state)
    #pprint.pprint(final_state)


    visited = set()
    queue = deque([(init_state, [],None)])  # (state, actions_chain)
    comp=0

    arbre = []
    node_id = 0
    state_to_id = {}

    prev_len_chen = 0

    start_time = time.time()
    while queue:
        comp+=1
        #print("comp",comp)
        
        
        state, chain, parent = queue.popleft()
        if(len(chain) != prev_len_chen):
            print("taille de chain",len(chain))
            prev_len_chen = len(chain)
        state_key = state_to_key(state)

        # Ajout du nœud à l'arbre
        current_id = node_id
        state_to_id[state_key] = current_id
        arbre.append({
            "id": current_id,
            "parent": parent,
            "action": chain[-1] if chain else None,
            "state": state
        })
        node_id += 1

        if is_goal(state, final_state):
            with open(arbre_file, "w", encoding="utf-8") as f:
                json.dump(arbre, f, indent=2, ensure_ascii=False)
            end_time = time.time()
            print(f"Temps d'exécution : {end_time - start_time:.2f} secondes")
            return chain

        for new_state, act_name in generate_actions(action_obj, state):
            new_state_key = state_to_key(new_state)
            if new_state_key not in visited:
                queue.append((new_state, chain + [act_name], current_id))
                visited.add(new_state_key)
    with open(arbre_file, "w", encoding="utf-8") as f:
            json.dump(arbre, f, indent=2, ensure_ascii=False)
    end_time = time.time()
    print(f"Temps d'exécution : {end_time - start_time:.2f} secondes")
    return None


if __name__ == "__main__":
    plan = bfs_planner("base.json", "final.json")
    if plan:
        print("Plan trouvé !")
        for i, step in enumerate(plan, 1):
            print(f"{i}. {step}")
    else:
        print("Aucun plan trouvé.")