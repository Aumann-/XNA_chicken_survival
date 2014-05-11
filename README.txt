This is a 3D survival game created in Windows XNA Studio for my Game Programming group project.

Game Description:

You play as a chicken running away from Colonel Sanders. Score is based on how long you can survive. Score increases over time and decreases as you stare at Sanders. You can throw eggs to force Sanders to despawn and relocate. An egg and a 500 point time bonus is given every 30 seconds. Each time Sanders is hit with an egg, his speed increases by 1.5 times its previous value. As the player stares at, his speed increases by 0.3 units per second.

Controls:

Space to begin, F10 to toggle between window and fullscreen, ESC to exit.

WASD to move, click to throw eggs.


Asset description:

The game contains a simple tree model made in Blender using Vertex colors instead of textures. 100 trees are spawned in random locations each time the game starts.

The chicken is a model obtained from BlendSwap (with credit given), changed to use vertex color instead of textures. After the initial cinematic camera, this model is removed from play.

The eggs are a model made in blender with a custom shader. Upon impact, the egg explodes into particles that are actually tiny egg models.

Upon death, a roasted chicken model (obtained from BlendSwap) is spawned.

The play camera follows the height of the terrain.

The game contains 2 cameras, an overhead cinematic camera for the beginning of the game and the end, and a walk camera to act as the player.

Sanders is a image file coded to act as a billboard.

Terrain is a RAW file made in Terragen with a texture image applied (found on google images).

Game contains multiple sounds (found...somewhere).