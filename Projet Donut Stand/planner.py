import json
from collections import deque
from copy import deepcopy
import pprint

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
    objets = list(state["grid"].keys())


    # drop
    for obj in objets:
        
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.drop(obj)
            if new_state:
                actions.append((deepcopy(new_state), f"drop({obj})"))
        except Exception:
            pass

    # moveTo
    positions = [(0,0), (1,1), (-1,1), (1,-1), (0,-1), (0,1), (-1,-1), (-1,0), (1,0)]
    for pos in positions:
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.moveTo(pos)
            if new_state:
                actions.append((deepcopy(new_state), f"moveTo{pos}"))
        except Exception:
            pass

    # pickup
    for obj in objets:
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.pickup(obj)

            if new_state:   # filtrer {}
                
                actions.append((deepcopy(new_state), f"pickup({obj})"))
        except Exception:
            pass
    
    

    '''# push
    for obj in objets:
        try:
            action_obj.world_state = deepcopy(state)
            new_state = action_obj.push(obj)
            if new_state:
                actions.append((deepcopy(new_state), f"push({obj})"))
        except Exception:
            pass

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

    action_obj = Action(base_file)
    init_state = action_obj.world_state

    #print("init_state",init_state)

    final_a_obj = Action(final_file)
    final_state = final_a_obj.world_state

    #pprint.pprint(init_state)
    #pprint.pprint(final_state)


    visited = set()
    queue = deque([(init_state, [])])  # (state, actions_chain)
    comp=0


    while queue:
        comp+=1
        print("comp",comp)
        
        
        state, chain = queue.popleft()
        print("taille de chain",len(chain))



        if is_goal(state, final_state):
            return chain

        for new_state, act_name in generate_actions(action_obj, state):
            queue.append((new_state, chain + [act_name]))

    return None

if __name__ == "__main__":
    plan = bfs_planner("test_base.json", "test_final.json")
    if plan:
        print("Plan trouvé !")
        for i, step in enumerate(plan, 1):
            print(f"{i}. {step}")
    else:
        print("Aucun plan trouvé.")