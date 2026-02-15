Honor Code: 
    - I did not get assistant from other people.
    - Minimal usage of Gemini when debugging some errors in CollectPowerPelletHCFSM
Name: Muhammad Luqman Irfan Bin Ahmad Kamal Peong
ID: 1221102096
MAIN SCRIPT: CollectPowerPelletHCFSM.cs
Edited Script:
    1. Program.cs
        - Adjusted window size
    2. PacmanScene.cs
        - Added a private gamemap reference and an update function to call gamemap.update.
    3. GameMap.cs
        - Made it so the StartColumn and StartRow are set to the hometile coordinate automatically.
        - Loading in LuqmanMap.tmx for the map
    4. Ghost.cs
        - instatiating FSM as CollectPowerPelletHCFSM
Custom Map Name: LuqmanMap
    - Home tile in top right corner
    - Goal tile in bottom left corner
