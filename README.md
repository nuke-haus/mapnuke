# Automated Improved Dominions Starts
## An open-source map generator for Dominions 5

### Foreword

I started this side project because I wanted a robust map generator for Dominions 5 that also allowed the user to tweak the results to their liking. I'm really happy with how it turned out but there is still polishing to be done.

### How to use

1. Run the program.
2. In the main menu you'll see a list of players on the left. Choose a player count and select which nations will be used.
3. Click the generate button. Depending on the player count the generation process could take up to 3 minutes.
4. Once the map is generated, you can move the camera around by clicking and dragging.
5. Right click a province node or connection node to open the editor panel for that node.
6. Once you are happy with the map, click the export button.

### The to-do list

Shortcomings I've acknowledged but lazily ignored.

- Support for all valid playercounts. Currently missing support for: 17, 19, 21, 22.
- Improve performance. Map generation process is kind of sluggish right now. The sprite placement code needs optimizing.
- Improve poor FPS when many sprites are being rendered on large maps. Not sure if this is doable.
- Improve sprite placement. Currently some sprites will overlap in places where they shouldn't. 

### The art to-do list

I'm a programmer first and foremost, so my sprite art is kind of lacking.

- Add more house sprites. My goal was to have small houses and large house sprites for each province type. Small provinces have no houses, normal ones have small houses, and large ones have large houses scattered around. Only wastelands have house sprites at the moment.
- Redo farmland sprites. They suck.
- Improve kelp forest sprites.
- Add specific sprites for trench/highland provinces to make it visually obvious they have a trench/highland.
- More sprite variety. Doesn't hurt to have more sprite variants.
- Improve province shaders. In general more visual polish is needed.

### The wish list

Some features I'd have liked to have that were ultimately too much work.

- Expose all relevant map generation parameters in a nice little UI dialog box so the user can tweak more parameters.
- Add a dialog that lets players give provinces custom names if they want.
- Support for non-wrapping maps.
