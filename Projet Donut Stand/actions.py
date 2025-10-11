import json
from typing import Any, Dict, List, Optional, Union
import copy

class Action : 
    def __init__(self, base_file ) :
        with open(base_file, 'r', encoding='utf-8') as f :
            self.base_data = json.load(f)
        self.objects = self.base_data['forme']
        self.world_state = {'arm' : {'holding' : False, 'object' : None}, 'forme' : self.objects}


    def pickup(self, objet : str) -> Dict:
        world_state = copy.deepcopy(self.world_state)
        #if objet not in self.objects :
            #raise ValueError(f'Objet '{objet}' non reconnu.')
        #print(world_state)

        
        
        if (world_state['arm']['holding']==False  and world_state['forme'][objet]['under'] == 'nothing' and world_state['forme'][objet]["poids"] < 5):

            world_state['arm']['object'] = objet
            world_state['arm']['holding'] = True

            on_top = world_state['forme'][objet]['on_Top_of']
            if on_top != 'table':
                world_state['forme'][on_top]['under'] = 'nothing'

            self.world_state = world_state
            
        else :
            return {}
        
        return world_state
    
    def drop(self, objet: str) -> Dict:
        world_state = copy.deepcopy(self.world_state)
        #if objet not in self.objects :
            #raise ValueError(f'Objet '{objet}' non reconnu.')
        
        if (world_state['arm']['holding'] == True and world_state['arm']['object'] == objet):
            world_state['arm']['object'] = None
            world_state['arm']['holding'] = False
            
            tmp = 'table'

            for o in self.objects :
                if(world_state['forme'][o]['position_occupe'] == world_state['forme'][objet]['position_occupe'] and o != objet and world_state['forme'][o]['under'] == 'nothing'):
                    tmp = o    
            
            world_state['forme'][objet]['on_Top_of'] = tmp
            world_state['forme'][objet]['under'] = 'nothing'

            if (tmp != "table") :
                world_state['forme'][tmp]['under'] = objet

            
            
            self.world_state = world_state
        else :
            return {} 
        return world_state
    
    def moveTo(self, position: tuple[int,int]) -> Dict:
        world_state = copy.deepcopy(self.world_state)
        #if (world_state['arm']['object'] not in self.objects) :
            #raise ValueError(f'Objet '{world_state['arm']['object']}' non reconnu.')
        
        if (world_state['arm']['holding']==True and world_state['arm']['object'] is not None):
            world_state['forme'][world_state['arm']['object']]['position_occupe'] = [list(position)]
            self.world_state = world_state
        else :
            return {}
        return world_state
    
    def push(self, objet : str) -> Dict:

        world_state = copy.deepcopy(self.world_state)
        #if objet not in self.objects :
            #raise ValueError(f'Objet '{objet}' non reconnu.')
        
        audessus = world_state['forme'][objet]['under'] == 'nothing'
        
        if (world_state['arm']['holding'] == False and world_state['forme'][objet] is not None and world_state['forme'][objet]['orientation'] == [0,1,0] and audessus):
            world_state['forme'][objet]['orientation'] = [0,0,1]
            self.world_state = world_state
      

        elif (world_state['arm']['holding'] == False and world_state['forme'][objet] is not None and world_state['forme'][objet]['orientation'] == [0,0,1] and audessus):
            world_state['forme'][objet]['orientation'] = [0,1,0]
            self.world_state = world_state
            
        else :
            return {}
        return world_state
    
    def roll(self, objet, position: tuple[int, int]) -> Dict:
        world_state = copy.deepcopy(self.world_state)

        # Vérifier si la position est déjà occupée par un autre objet
        for obj_name, obj_data in world_state['forme'].items():
            if obj_name != objet and obj_data['position_occupe'][0] == list(position):
                return {}  # Position occupée, on ne fait rien

        if (world_state['arm']['holding'] == False and
            world_state['forme'][objet]['orientation'] == [0,0,1] and
            objet == "cylindre"):
            world_state['forme'][objet]['position_occupe'] = [list(position)]
            self.world_state = world_state
        else:
            return {}
        return world_state