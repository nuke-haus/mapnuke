# MapNuke
## An open-source map generator for Dominions 5

### How to use

1. Run the program.
2. In the main menu you'll see a list of players on the left. Choose a player count and select which nations will be used.
3. Click the generate button. The generation process could take up to 3 minutes depending on the player count and your computer specs.
4. Once the map is generated, you can move the camera around by clicking and dragging.
5. Right click a province node or connection node to open the editor panel for that node.
6. Once you are happy with the map, click the export button. Maps are exported to the data folder.

### How to add your own art style

A recent update to MapNuke now allows you to add your own custom art styles. The sprites and materials used by the map generator are dictated by the art style. Here's a step by step guide for adding your own art style.

1. Install Unity Hub and fork this repository using SourceTree or something similar. Download the version of unity required by this repository then open the unity project.
2. Create the folders you'd like to store your art assets in. For example, the default art assets are stored in _Gfx/MapArt/Default_. Your folder will be named something like _Gfx/MapArt/MyArtStyle_. Import your desired sprite assets. Click on imported art assets to look at their import settings. You can copy the settings used by existing assets if you aren't sure what values to use here.

![Step 2](https://cdn.discordapp.com/attachments/404681432238391307/767117175877402654/d2.png)

3. Create the folders you'd like to store your materials in. For example, the default materials are stored in _Gfx/Materials/Default_. Follow a similar naming convention to what you did in step 2 here. Import any textures being used for the province materials (if applicable) into this folder. You can examine the default materials which use a noise shader to see how those are done. You'll want to use a different shader if you're doing a texture-based material.

![Step 3](https://cdn.discordapp.com/attachments/404681432238391307/767117179451342928/d5.png)

4. Open the main scene (if it's not already open) and navigate to the ArtManager in the scene browser. Duplicate the existing default art style and rename it to your art style's name.

![Step 4](https://cdn.discordapp.com/attachments/404681432238391307/767117174317514803/d1.png)

5. Click on the newly made art style object and examine its properties. You'll see a collection of values you can modify.

![Step 5](https://cdn.discordapp.com/attachments/404681432238391307/767117176833441813/d3.png)

It's a bit complex so i'll try to describe what each value does.

__Cull Chance__ - A value between 0 and 1. Higher values will result in less sprite density on the province. Try using 0 and 0.99 if you want to see the difference it makes.
__Province Edge Threshold__ - How close to the edge of the province can sprites be? This helps stop sprites from overlapping. 

Each sprite has several parameters.

__Sprite__ - The sprite to use in summer.
__Winter Sprite__ - The sprite to use in winter.
__Spawn Chance__ - Value between 0 and 1. 1 means it will be very common.
__Can Flip__ - Can the sprite be flipped on the X axis?
__Can Flip Winter__ - Same as above but in winter.
__Is Centerpiece__ - Some province types (ie. caves, mountains) have a large central sprite. These are centerpieces.
__Valid Colors__ - List of valid colors to tint the sprite with. Just use white with 100% alpha if you don't want the sprite to be tintable.
__Valid Colors Winter__ - Same as above for winter.
__Valid Terrain__ - What type of terrain flag is required for this sprite to get used? Certain provinces (ie. wasteland) use this to specify special sprites. Examine the default art style to understand this better.

6. Add your materials from step 3 to the material list.

![Step 6](https://cdn.discordapp.com/attachments/404681432238391307/767117177982550056/d4.png)

7. Add your new art style to the list of valid art styles in the ArtManager. Click the ArtManager in the scene view and examine its _Art Configurations_ field. Increase its capacity by 1 and click-drag your new art style into the new space. This is the collection of valid art styles that can be chosen at runtime.

8. Click the play button and test your art style out. It should appear in the art style dropdown list next to the nation picker menu.

### The to-do list

Missing features and other known issues.

- Add unique sprites for hot and cold province modifiers. For example, a swamp with the colder province attribute should have some sort of unique sprite. Right now only wastelands are given hot/cold modifiers.
- Compact map layouts for 15, 17, 18, 19, 20 and 21 players.

### The wish list

Some features that would be nice to have.

- More sprite variety.
- Add a dialog box somewhere that lets players give provinces custom names if they want.
- Support for non-wrapping maps.

### Other info

Stay tuned for more cool stuff - www.nuke.haus
