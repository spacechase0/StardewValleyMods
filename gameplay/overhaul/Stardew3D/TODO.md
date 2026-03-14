Some of these might depend on items from other sections.

No particular order of priority. Some of these definitely fall into "feature creep" despite not being in that section.

# Codebase Cleanup / Refactoring
* Refactor input to be more contextual based on what is currently going on in-game
* Possibly refactor input methods to be more abstract?
    * So that "pure hand tracking" or "no VR controller but do have a gamepad" could be added with minimal extra code
    * Possibly covered by "dynamic tag system for game handlers" thing
* Dehardcode extended qualified IDs
* Dynamic tag system for game handlers (so that they can be enabled/disabled from a config option changing)
* Rework RenderBatcher:
    * Instanced -> InstancedModel
    * Generic -> InstancedVertices
    * NonInstanced -> Direct
    * + Dedicated billboarding system (for proper optimized use of tilesheets)
* Better system for menu handling

# Asset Workflow
* In-game editors for:
    * Models
        * Animation metadata
        * Texture overrides
    * Maps
        * Heightmap
        * Walls
        * Props
        * Shortcut tiles for props
        * Portals
    * Interaction data
    * Menus
        * Clickables
    * Support modded content for all of the above
    * Export formats: C#, JSON, TMX (for heightmap data)
* Map generation for mines (and maybe indoors in general?)
* Maybe a rough outdoor map generation, for rudimentary support of modded content?

# MVP
* Finish implementing debris rendering
* Crosshair for flatscreen first person
* Point and click controls
* [VR] Allow headset position to move player (and ideally, the camera)

# "Game Mode" stuff (ie. the input / rendering combos)
* Third person mode
* [VR] More motion controls
    * Hoe
    * Watering Can
    * Fishing Rod
    * Pan
    * Clubs (how to differentiate from sword besides hitbox?)
    * Daggers
    * Shears
    * Milk pail
    * Return scepter
* [VR] Remove floaty HUD as much as possible and change to in-world alternatives where possible
    * Day time money box -> watch
    * Chat -> carrier pigeons (or bats in the mines, or doves if you have doved children)
        * because it's funny
* [VR] More input models, including mix and match capabilities:
    * Motion controls, including gestures
    * Point-and-click with controllers
    * Point-and-click with head + gamepad
    * If practical, full hand tracking?
    * If practical, voice recognition for some niche cases? (Unsure how effective the pre-generative-AI solutions are)
* [VR] Holster system for selecting items
* [VR] Allow desktop window to show any of the other renderers (3d or even 2d)

# Graphics
* Much better (potential) idea to replace placeholder system: Reverse patch the draw methods and redirect SpriteBatch into a custom thing
* Animation support for models
* Lighting
    * Ambient light
    * Light sources like torches, TVs, etc.
* Shadows
* Show indoor locations while outside, through windows/doors? (and vice versa)
* IK for player in first person
* The endless journey of optimization

# Movement / Traversal
* "Regions" thing? For stretching/squishing certains portions of the map when in 3D
    * Allows matching up a seamless 3D world despite locations not perfectly matching up
    * Lets us make cliffsides line up correctly with how it looks in 2D
    * How do we handle the object placement grid for this? Especially for paths/floors
* Seamless 3d location traversal?
    * MP support would be tricky

# Polish
* Make optional the highlight/outline for when a cursor is pointing at something
* [VR] Swipe sound when swinging weapons
* Button on title screen for changing current mode (including turning off VR with your PC mouse, even while VR is active)
* [VR] A full scene for the title menu
    * On the Summit, can see the entire valley - exact layout based on your last loaded save
    * Title graphic floating in the air
    * A bookshelf or something with a book per save that you can browse and select to load the game
    * A mirror and table with documents, for customizing and selecting for a new save
    * A Rosetta Stone-type monolith for selecting language
    * An area you can go into to test/configure various VR things before actually loading a save
    * A way of seeing the credits too - TBD how
    * If GMCM is installed, some way of accessing that

# Feature creep
* Proximity voice chat
* In VR, dual wielding support
* Full 3D collisions (jumping off cliffs or on your bed, for example)
