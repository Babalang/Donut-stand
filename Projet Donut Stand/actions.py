import json
from typing import Any, Dict, List, Optional, Union

class Action : 
    def __init__(self, base_file : str = "base.json") :
        with open(base_file, 'r', encoding='utf-8') as f :
            self.base_data = json.load(f)
        self.objects = self.base_data["forme"]
        self.world_state = {"arm" : {"holding" : False, "object" : None}, "grid" : self.objects}


    def pickup(self, objet : str) -> Dict:
        world_state = self.world_state.copy()
        if objet not in self.objects :
            raise ValueError(f"Objet '{objet}' non reconnu.")
        
        if (world_state["arm"]["holding"]==False and world_state["arm"]["object"] is None):
            world_state["arm"]["object"] = objet
            world_state["arm"]["holding"] = True
        
        return world_state
    
    def drop(self, objet: str) -> Dict:
        world_state = self.world_state.copy()
        if objet not in self.objects :
            raise ValueError(f"Objet '{objet}' non reconnu.")
        
        if (world_state["arm"]["holding"]==True and world_state["arm"]["object"]==objet):
            world_state["arm"]["object"] = None
            world_state["arm"]["holding"] = False
        
        return world_state
    
    def moveTo(self, position: tuple[int,int]) -> Dict:
        world_state = self.world_state.copy()
        if world_state["arm"]["object"] not in self.objects :
            raise ValueError(f"Objet '{world_state["arm"]["object"]}' non reconnu.")
        
        if (world_state["arm"]["holding"]==True and world_state["arm"]["object"] is not None):
            world_state["grid"][world_state["arm"]["object"]]["position_occupe"] = [list(position)]
            world_state["arm"]["object"] = None
            world_state["arm"]["holding"] = False
            self.world_state = world_state
        return world_state
    



# Programme pour tester les actions
def main():
    print("=== Test des actions ===")
    
    # Créer une instance d'Action
    action = Action()
    
    print("État initial:")
    print(json.dumps(action.world_state, indent=2, ensure_ascii=False))
    print("\n" + "="*50 + "\n")
    
    try:
        # Test 1: Pickup d'un cylindre
        print("1. Pickup du cylindre...")
        new_state = action.pickup("cylindre")
        print("Nouvel état après pickup:")
        print(json.dumps(new_state, indent=2, ensure_ascii=False))
        print("\n" + "="*50 + "\n")
        
        # Test 2: MoveTo pour déplacer l'objet
        print("2. Déplacement du cylindre vers la position (5, 5)...")
        final_state = action.moveTo((5, 5))
        print("État final après déplacement:")
        print(json.dumps(final_state, indent=2, ensure_ascii=False))
        print("\n" + "="*50 + "\n")
        
        # Test 3: Pickup d'un cube petit
        print("3. Pickup du cube petit...")
        action.pickup("cube")  # Référence au niveau cube
        print("État après pickup du cube:")
        print(json.dumps(action.world_state, indent=2, ensure_ascii=False))
        
    except ValueError as e:
        print(f"Erreur: {e}")
    except Exception as e:
        print(f"Erreur inattendue: {e}")

if __name__ == "__main__":
    main()