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

MapNuke now allows you to add your own custom art styles. The sprites and materials used by the map generator are dictated by the art style. Here's a step by step guide for adding your own art style.

1. Install Unity Hub and fork this repository using SourceTree or something similar. Download the version of unity required by this repository then open the unity project.
2. Create the folders you'd like to store your art assets in. For example, the default art assets are stored in _Gfx/MapArt/Default_. Your folder will be named something like _Gfx/MapArt/MyArtStyle_. Import your desired sprite assets. Click on imported art assets to look at their import settings. You can copy the settings used by existing assets if you aren't sure what values to use here.

__Important note: Don't use a perfectly white color (ie. a color with 100% red, green, and blue) in your sprite. It will cause issues. Dominions 5 uses that color to track province positions. If you need to use white in your sprite, make it some off-white that looks close enough to white.__

![Step 2](https://cdn.discordapp.com/attachments/404681432238391307/767117175877402654/d2.png)

3. Create the folders you'd like to store your materials in. For example, the default materials are stored in _Gfx/Materials/Default_. Follow a similar naming convention to what you did in step 2 here. Import any textures being used for the province materials (if applicable) into this folder. You can examine the default materials which use a noise shader to see how those are done. You'll want to use a different shader if you're doing a texture-based material.

![Step 3](https://cdn.discordapp.com/attachments/404681432238391307/767117179451342928/d5.png)

4. Open the main scene (if it's not already open) and navigate to the ArtManager in the scene browser. Duplicate the existing default art style and rename it to your art style's name.

![Step 4](https://cdn.discordapp.com/attachments/404681432238391307/767117174317514803/d1.png)

5. Click on the newly made art style object and examine its properties. You'll see a collection of values you can modify.

![Step 5](https://cdn.discordapp.com/attachments/404681432238391307/767117176833441813/d3.png)

It's a bit complex so i'll try to describe what the values for each terrain type do:

* __Cull Chance__ - A value between 0 and 1. Higher values will result in less sprite density on the province. Try generating a map using a value of 0 and then with a value of 0.99 if you want to see the difference it makes.

* __Province Edge Threshold__ - How close to the edge of the province can sprites be? This helps stop sprites from overlapping. 

Each sprite has several parameters:

* __Sprite__ - The sprite to use in summer. Use transparent_pixel.png for your summer sprite if you don't want anything to render in summer.

* __Winter Sprite__ - The sprite to use in winter. Use transparent_pixel.png for your winter sprite if you don't want anything to render in winter.

* __Spawn Chance__ - Value between 0 and 1, 0 being nonexistent and 1 being very common.

* __Can Flip__ - Can the sprite be flipped on the X axis? If so, there is a 50% chance it will randomly flip a given sprite. Mostly used with grass sprites.

* __Can Flip Winter__ - Same as above but in winter.

* __Is Centerpiece__ - Some province types (ie. caves, mountains) have a large central sprite. These are centerpieces. Only one centerpiece is used and it's selected based on the size. Examine the cave province centerpieces in the default art style to understand this better.

* __Valid Colors__ - List of valid colors to tint the sprite with. Just use white with 100% alpha if you don't want the sprite to be tintable.

* __Valid Colors Winter__ - Same as above for winter.

* __Valid Terrain__ - What type of terrain flag is required for this sprite to get used? Certain provinces (ie. wasteland) use this to specify special sprites for hotter/colder province modifiers. Examine the default art style to understand this better.

6. Add your materials from step 3 to the material list.

![Step 6](https://cdn.discordapp.com/attachments/404681432238391307/767117177982550056/d4.png)

7. Add your new art style to the list of valid art styles in the ArtManager. Click the ArtManager in the scene view and examine its _Art Configurations_ field. Increase its capacity by 1 and click-drag your new art style into the new space. This is the collection of valid art styles that can be chosen at runtime.

![Step 7](https://cdn.discordapp.com/attachments/404681432238391307/767117180805709824/d6.png)

8. Click the play button and test your art style out. It should appear in the art style dropdown list next to the nation picker menu.

### How to make province shaders use textures.

A texture-based material probably looks a lot nicer than the materials with perlin noise shaders I used. I'm not much of an artist. You'll need to create a new material using the __Unlit/Texture__ shader. Import your texture and use settings similar to the ones in the image below. Make sure wrap mode is set to repeat or your texture will not tile correctly.

![Image](https://cdn.discordapp.com/attachments/668838745516015625/780931011683811348/screenshot-mangadex.org-2020.11.24-17_12_23.png)
