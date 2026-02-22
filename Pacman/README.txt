Honor Code: 
    - I did not get assistant from other people.

Name: Muhammad Luqman Irfan Bin Ahmad Kamal Peong
ID: 1221102096
This project builds off of the Pacman Game.
VargasHCFSM.cs was based on GhostHCFSM.cs
Important scripts:
    - PacmanScene.cs
        - loads ghost/vargas, the player, triptile manager.
        - checks win and lose conditions and restarts the scene
    - Pacman.cs
        - Movement is altered to be more normal.
        - Heatmap create and update is handled here.
    - TripTileManager.cs
        - Sets up the triptiles and updates which ones are active depending on the heatmap
    - VargasHCFSM.cs
        - States (Roaming, Hot spots, Chase, Alert)
        Roaming - Vargas travels randomly

        Hot spots - Vargas travels to hot spots (Commonly stepped on tiles). He'll randomly pick out of the top 20 spots

        Chase - Upon spotting the player, Vargas will give chase. In this state, Vargas can block tiles in trip spots. He'll
        predict where the player will go and block paths.
        (Vargas tracks the players position after seeing them. If the player goes out of vision, Vargas tracks their
            position for a few seconds afterwards)

        Alert - If the player passes a trip spot, Vargas is alerted and will travel to the trip spot location

        GameOver - If the player wins or losses, Vargas enters this state and stays idle. 
    - ScoreUI.cs
        - Displaying the score ui.
