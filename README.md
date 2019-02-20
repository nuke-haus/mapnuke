# Automated Improved Dominions Starts
## An open-source map generator for Dominions 5

### Foreword

I started this side project because I wanted a robust map generator for Dominions 5 that also allowed the user to manually tweak the results to their liking. I'm really happy with how it turned out but there is still polishing to be done.

### How to use

1. Run the program.
2. In the main menu you'll see a list of players on the left. Choose a player count and select which nations will be used.
3. Click the generate button. Depending on the player count the generation process could take up to 3 minutes.
4. Once the map is generated, you can move the camera around by clicking and dragging.
5. Right click a province node or connection node to open the editor panel for that node.
6. Once you are happy with the map, click the export button. Maps are exported to the data folder.

### The to-do list

Shortcomings I've acknowledged but lazily ignored.

- Currently missing map layouts for the following player counts: 21, 22.
- Improve performance. Map generation process is kind of sluggish right now. The sprite placement code needs optimizing.
- Improve poor FPS when many sprites are being rendered on large maps.... If possible.

### The art to-do list

I'm a programmer first and foremost, so my sprite art is kind of lacking.

- More sprite variety. Every type of sprite (ie. trees, mountains) should have, at the very least, 5 variants.
- Add unique sprites for hot and cold province modifiers. For example, a swamp with the colder province attribute should have some sort of unique sprite.
- Improve province shaders. Sea shaders should have a nice shoreline but they don't. 

### The wish list

Some features I'd have liked to have that were ultimately too much work.

- Expose all relevant map generation parameters in a nice little UI dialog box so the user can tweak more parameters.
- Add a dialog box somewhere that lets players give provinces custom names if they want.
- Support for non-wrapping maps.

### Other info

You can contact me on twitter (terrible idea): https://twitter.com/nuke_makes_game
