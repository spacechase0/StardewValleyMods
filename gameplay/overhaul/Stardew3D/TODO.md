Some of these might depend on items from other sections.

No particular order of priority. Some of these definitely fall into "feature creep" despite not being in that section.

# Next milestone: VR test gameplay video (first week of spring)
Has some overlap with other sections
* Fix chest menu being repositioned each time you place an item (because that replaces the entire menu)
* Farmhouse interactions, and actually walking up the porch
* Motion controls for:
    * Hoe
    * Watering can
    * Fishing rod
* Make wall furniture properly visible (at least in initial farmhouse)
* For various placeholders, hide the shadows from 2d rendering
* Make sure I can get through the intro sequence
* Make monster hitboxes more lenient
* Sky
    * sun/moon
    * stars
    * weather
* Fix audio for NPC footsteps(?) playing even when they are in another location (ex. hearing things in town while in a shed)
* Temporary hacks for the video:
    * Increase player speed by 25% (since we go at like 80% speed of 2d, for some reason)
    * Make all layers show on the ground, not just back/building
    * Maybe basic placeholders for various buildings?

# Codebase Cleanup / Refactoring
* Refactor input to be more contextual based on what is currently going on in-game
* Possibly refactor input methods to be more abstract?
    * So that "pure hand tracking" or "no VR controller but do have a gamepad" could be added with minimal extra code
    * Possibly covered by "dynamic tag system for game handlers" thing
* Dehardcode extended qualified IDs
* Dynamic tag system for game handlers (so that they can be enabled/disabled from a config option changing)
* Better system for menu handling
* Migrate from OpenVR.Net to the official bindings at https://github.com/ValveSoftware/openvr
    * OpenVR.Net hasn't been updated in years, and I'm not even sure if it used the latest version for when it did.
    * I'm already ignoring most of what it provides on top and just use base OpenVR directly, since I couldn't get the added stuff working right
    * I remember having other problems with whatever binding I used before OpenVR.Net
* Make `TileSpot` match `Game1.(up|down|left|right)`
* Rework RenderBatcher instancing system to be less persistent
    * I think this would also let me flatten the render handler system, removing the RenderData stuff. Handlers are already per-instance anyways.
    * This might be less practical than I realized once lighting is a thing (if I don't want parts of shadow maps to be recalculated constantly)
    * If I have way of attaching code to objects and running code immediately when said object disappears, I can just remove them when necessary
        * ConditionalWeakTable will need to wait for the garbage collector, and we can't entirely rely on IDisposable
        * Maybe the 1.7 thing if it happens...
* Make SpriteBatchProxy sane

# Asset Workflow
* In-game editors for:
    * Models
        * Animation metadata
        * Texture overrides
    * Maps
        * Props
        * Shortcut tiles for props
        * Portals
        * Basic tile editing (for tweaking existing maps better, for ceilings, etc.)
    * Interaction data
    * Menus
        * Clickables
    * Support modded content for all of the above
    * Export formats: C#, JSON, TMX (for heightmap data)
* Map generation for mines (and maybe indoors in general?)
* Maybe a rough outdoor map generation, for rudimentary support of modded content?
* Rework editor to make more sense from a "adding content for various things and exporting to a single content pack" standpoint
    * Ex. base mod and content packs are different "projects" - you select one, make all the edits, and saving exports to it
* Either add C# exporting for things like Interaction data, or export to json and make the C# side load that instead of the current system

# General
* Full 3D collisions?

# "Game Mode" stuff (ie. the input / rendering combos)
* Merge first person and third person modes, and split off camera controller from them
* [VR] More input models, including mix and match capabilities:
    * Motion controls, including gestures
    * Point-and-click with controllers
    * Point-and-click with head + gamepad
    * If practical, full hand tracking?
    * If practical, voice recognition for some niche cases? (Unsure how effective the pre-generative-AI solutions are)
* [VR] Allow desktop window to show a flatscreen renderer (would be more performance intensive)?
* [VR] Make headset position actually move the farmer, not just the camera

# Graphics
* Animation support for models
* Lighting
    * Sun/moon directional light
    * Light sources like torches, TVs, etc.
* Sky environment
    * Actual sun/moon
        * Moon should include the shipping menu easter egg where you can click it when it's full to make it have the face
    * Weather
    * Stars
    * Maybe rare fancy stuff later, like falling stars and auroras
* Shadows
* Show indoor locations while outside, through windows/doors? (and vice versa)
* IK for player in VR first person
* The endless journey of optimization
    * Optimize building maps, especially rebuilding an existing one with not many changes. Currently, the editor lags on larger maps while doing edits.
    * Could I possibly make the heightmap into a Texture2D, and then have the vertex shader offset things based on that?
        * Almost everything being rendered would no longer need to get heightmap data and apply it, so those calculations could be skipped each time.
        * It would need to be per-game-location, which might interfere with batching performance.
        * Gameplay stuff could perhaps use the heightmap too to avoid the constant relevant calculations,
          however there'd be a higher risk of mismatch between gameplay and graphics.

# Movement / Traversal
* [VR] Snap turning, teleport locomotion
* "Regions" thing? For stretching/squishing certains portions of the map when in 3D
    * Allows matching up a seamless 3D world despite locations not perfectly matching up
    * Lets us make cliffsides line up correctly with how it looks in 2D
    * How do we handle the object placement grid for this? Especially for paths/floors
* Seamless 3d location traversal?
    * MP support would be tricky

# Interactions
* More point and click controls
* Add "virtual interactables" for things like in-world menus
* Interaction areas should have an explicit "facing" normal, rather than relying on the shape's rotation
* Add more collision modes
    * Currently have just have "Impact" activation (opposite normal of the other object) and "Object" target (the other object), and a single mode per thing
    * Target: "Ground"/"Ceiling"/"Water/"Walls" (ex. a hoe would be impacting the ground)
    * Action: "Slide" (ex. sliding a hoe along the ground to activate, shears along a sheep, ...)
* [VR] Reimagine controls specifically for VR, rather than be the same as 2d left click / right click
    * Ex. grip = grab
* [VR] Holster system for hotbar
    * Based on headset coordinate space?
    * Optional anchoring to trackers rather than direct headset coordinates (ex. for if you have body tracking)
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
* [VR] Reimagine various menus as in-world
    * Forge: Place things on it in the right spots and activate it
    * Pierre's shop: buy things off the actual shelves?

# Misc
* Crosshair for flatscreen first person
* Make as much audio in the game have proper 3d positioning

# Polish
* Make optional the highlight/outline for when a cursor is pointing at something
* Make the selection outline better (ie. not just the bounding box)
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
