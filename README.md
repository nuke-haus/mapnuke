# MapNuke
## An open-source map generator for Dominions 5

### HOW TO USE

1. Run the program.
2. In the main menu you'll see a list of players on the left. Choose a player count and select which nations will be used.
3. Click the generate button. The generation process could take up to 3 minutes depending on the player count and your computer specs.
4. Once the map is generated, you can move the camera around by clicking and dragging.
5. Right click a province node or connection node to open the editor panel for that node.
6. Once you are happy with the map, click the export button. Maps are exported to the data folder.

### CREATING YOUR OWN ART STYLE

MapNuke now allows you to add your own custom art styles. The sprites and materials used by the map generator are dictated by the art style. Here's a step by step guide for adding your own.

1. Fork this repository on GitHub. 
2. Clone your forked repository using SourceTree or some other source control app. 
3. Install Unity Hub. 
4. Download Unity version __2019.2.11f1__ through the Unity Hub then open the MapNuke project using that Unity version.

⚠ __Only Unity 2019.2.11f1 is compatible with this project! Using any other version will cause issues.__ ⚠ 

5. Create the folders you'd like to store your art assets in. For example, the default art assets are stored in _Gfx/MapArt/Default_. Your folder will be named something like _Gfx/MapArt/MyArtStyle_. 

![Step 5](https://cdn.discordapp.com/attachments/404681432238391307/767117175877402654/d2.png)

6. Add your desired sprite assets in your new folder. Click on art assets to view and edit their import settings. 

⚠ __Make sure your sprites have the "Art" packing tag in their import settings or they will have rendering issues! If you aren't sure about something, just copy the settings used in one of the default sprites.__ ⚠ 

⚠ __Do not use a perfectly white color (ie. a color with 100% red, green, and blue) in your sprite. It will cause issues. Dominions 5 uses that color to track province positions. If you need to use white in your sprite, make it an off-white.__ ⚠ 

![Step 6](https://cdn.discordapp.com/attachments/668838745516015625/824808363518066719/Untitled.png)

7. Create the folders you'd like to store your materials in. For example, the default materials are stored in _Gfx/Materials/Default_. Follow a similar naming convention to what you did in step 2 here. Import any textures being used for the province materials (if applicable) into this folder. You can examine the default materials which use a noise shader to see how those are done. You'll want to use a different shader if you're doing a texture-based material.

![Step 7](https://cdn.discordapp.com/attachments/404681432238391307/767117179451342928/d5.png)

8. Open the main scene (File -> Open Scene -> Scenes -> Main) and navigate to the Resources/Prefabs/ArtStyles folder in the project browser. Duplicate the Default art style prefab and rename it (in this example, the newly created prefab is MyArtStyle). 

![Step 8](https://cdn.discordapp.com/attachments/404681432238391307/904876855335735337/Untitled.png)

9. Double click on the newly made art style prefab and examine its properties. You'll see a collection of art configuration properties you can modify.

![Step 9](https://cdn.discordapp.com/attachments/404681432238391307/873954882351149136/dfffff.png)

It's a bit complex so i'll try to describe what the values for each terrain type do:

* __Cull Chance__ - A value between 0 and 1. Higher values will result in less sprite density on the province. Try generating a map using a value of 0 and then with a value of 0.99 if you want to see the difference it makes.

* __Province Edge Threshold__ - How close to the edge of the province can sprites be? This helps stop sprites from overlapping over borders into other provinces. 

Each sprite has several parameters:

* __Sprite__ - The sprite to use in summer. Use transparent_pixel.png for your summer sprite if you don't want anything to render in summer.

* __Winter Sprite__ - The sprite to use in winter. Use transparent_pixel.png for your winter sprite if you don't want anything to render in winter.

* __Size__ - How large the sprite is. This value is used to determine if it's too close to the edge of the province or overlaps another sprite.

* __Culling Radius__ - This value is used to remove potential sprite placement positions near the sprite. This helps with reducing overlap between sprites.

* __Cull Sprite Positions__ - This boolean value determines whether a sprite should ever cull nearby sprite positions.

* __Spawn Chance__ - Value between 0 and 1, 0 being nonexistent and 1 being very common.

* __Can Flip__ - Can the sprite be flipped on the X axis? If so, there is a 50% chance it will randomly flip a given sprite. Mostly used with grass sprites.

* __Can Flip Winter__ - Same as above but in winter.

* __Is Centerpiece__ - Some province types (ie. caves, mountains) have a large central sprite. These are centerpieces. Only one centerpiece is used and it's selected based on the size. Examine the cave province centerpieces in the default art style to understand this better.

* __Place At Least One__ - If this is enabled, each province will have at least one of these sprites present. Disable this option if you want to make certain sprites more rare.

* __Valid Colors__ - List of valid colors to tint the sprite with. Just use white with 100% alpha if you don't want the sprite to be tintable.

* __Valid Colors Winter__ - Same as above for winter.

* __Valid Terrain__ - What type of terrain flag is required for this sprite to get used? Certain provinces (ie. wasteland) use this to specify special sprites for hotter/colder province modifiers. Examine the default art style to understand this better.

10. Add your materials from step 3 to the material list.

![Step 10](https://cdn.discordapp.com/attachments/404681432238391307/767117177982550056/d4.png)

11. Click the play button and test your art style out. It should appear in the art style dropdown list next to the nation picker menu.

---

### PROVINCE SHADERS

Custom art styles can specify which materials each province type uses. There are several custom shaders included with MapNuke you can make use of in your custom materials.

---

#### Custom/Perlin3, Custom/Perlin4, Custom/Perlin5

![Perlin Noise](https://cdn.discordapp.com/attachments/404681432238391307/793239502092894228/perlin.png)

The perlin noise shaders create a randomized set of colorful blobs. Perlin3 lets you choose 3 colors, Perlin4 is for 4 colors and Perlin5 is for 5 colors.

There are some special perlin noise shaders used in niche cases:

- Custom/Perlin5write
- Custom/Perlin6write
- Custom/Perlin5read

These use stencil buffers to create a shoreline effect in bodies of water (and rivers). The materials for sea provinces would use the Perlin5write shader and the shorelines would use the Perlin5read shader.

---

#### Custom/Texture1024, Custom/Texture2048, Custom/Texture4096

![Texture](https://cdn.discordapp.com/attachments/404681432238391307/793239505951129660/tex.png)

These shaders are useful for just basic textures. In many cases it's probably preferable to use this shader since handmade textures are always nicer than basic perlin noise. The size of your texture needs to match the shader, ie. Custom/Texture2048 should be used with a texture that is 2048x2048 pixels. 

I recommend using 2048x2048 as your texture resolution. 1024x1024 is a bit low and 4096x4096 is a bit overkill.

There are some special texture shaders used in niche cases:

- Custom/Texture1024read
- Custom/Texture1024write
- Custom/Texture2048write
- Custom/Texture4096write

These use stencil buffers to create a shoreline effect in bodies of water (and rivers). The materials for sea provinces and rivers would use the TextureXXXXwrite shader and the shorelines would use the Texture1024read shader.

Your texture import settings should look something like:

![Texture Import](https://cdn.discordapp.com/attachments/404681432238391307/793258263511695360/import.png)

Use point filtering if you want crisp pixel art and bilinear/trilinear filtering if you want antialiasing.

---

#### Custom/TexBlend2, Custom/TexBlend3, Custom/TexBlend4

![Texture Blend](https://cdn.discordapp.com/attachments/404681432238391307/793239504118480896/texblend.png)

These shaders combine perlin noise with textures. Could be useful for some niche cases.
