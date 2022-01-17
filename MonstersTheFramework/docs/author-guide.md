﻿[← back to readme](README.md)

# Monsters - the Framework

This documentation is for making mods; for using Monsters the Framework as a user, please check the [Nexus](https://www.nexusmods.com/stardewvalley/mods/10673) page.

You can edit these files with Content Patcher, or with your C# mod.

## Data types
A `bool` is true or false.

A `string` is just text.

An `int` is a whole number that is positive, negative, or 0.

A `double` is a number that can have a fractional component, like 3.14.

A `Vector2` is an object with two `double` components, `X` and `Y`.

An `Enum<X,Y,Z>` can have the values in between the angle brackets, comma separated. For this example, the values would be `X`, `Y`, or `Z`.

If a type has `?` after it, it can be null.

### Weighted<T>
A `Weighted<T>` is an object with a `Weight` value of type `double`, as well as a `Value` of type `T`. So, a `Weighted<string>` would look like this:

```json
{
    "Weight": 3.5,
    "Value": "meow"
}
```

These will often come in an array, with the notation `Weighted<T>[]`, meaning (for example, `Weighted<string>[]`):

```json
[
    // Noises my cat is making while I write this
    {
        "Weight": 3.9,
        "Value": "meow",
    },
    {
        "Weight": 5.1,
        "Value": "mrow",
    },
    {
        "Weight": 25.0,
        "Value": "MROOOOOW",
    }
]
```

When this is the case, it is used to determine a single value in the array with random chance. A `Weighted<t>` with a smaller weight is less likely to be chosen than one with a larger weight. Weights are relative to other weights in the same array.

### ItemDrop
An item drop is what a monster can drop on death. It takes two values, a `Drop` (which can be a vanilla item ID, name, or DGA item ID), and a `Quantity` (which defaults to 1).

```json
{
    "Drop": "Prismatic Shard",
    "Quantity": 4,
}
```


### Dictionary<string, T>
A `Dictionary<string, T>` is, for the purposes of this documentation, a normal JSON object containing values of a particular type.

Here is a `Dictionary<string, int>`:

```json
{
    "SomeField": 123,
    "AnotherField": 456,
    "LastField": 789
}
```

## Monsters

The monsters data file is `spacechase0.MonstersTheFramework/Monsters`.

Custom monsters for this mod are controlled via state machine. Essentially, there a bunch of states the monster can be in, but they can only be in one at a time. The current state dictates its behavior. Different triggers can be set to make it change from one state to another.

| Field | Type | Description |
| --- | --- | --- |
| `Name` | `string` | The display name of the monster. This may not be used in vanilla much, but other mods may make use for it. |
| `CorrespondingMonsterGoal` | `string` | The monster goal that this monster will contribute towards on death. Custom monster goals are coming in the future. |
| `MaxHealth` | `int`| The maximum health of this monster. |
| `Drops` | `Weighted<ItemDrop?>[][]` | The potential items a monster should drop on death. This is an array of an array, meaning an array of `Weighted<ItemDrop[]`. See note below table. |
| `StartingState` | `string` | Which state in the `States` dictionary the monster should start in. |
| `States` | `Dictionary<string, StateData>` | The potential states that this monster may use. |

Example for array of array of `Weighted<ItemDrop?>`, which means `Weighted<ItemDrop?>[][]`:

```json
[
    // First drop - 50% chance of bat wing, 50% chance of 3 slime
    [
        {
            "Weight": 1,
            "Value": {
                "Drop": "Bat Wing",
            }
        },
        {
            "Weight": 1,
            "Value": {
                "Drop": "Slime",
                "Quantity": 3
            }
        }
    ],
    // Second drop - 25% chance of a prismatic shard, 75% chance of nothing
    [
        {
            "Weight": 1,
            "Value": {
                "Drop": "Prismatic Shard",
            }
        },
        {
            "Weight": 3,
            "Value": null
        }
    ]
]
```

### StateData
A state defines a monster's current behavior and appearance.

All fields describe their behavior *during this state*. If the state changes to a different one, they no longer apply.

| Field | Type | Description |
| --- | --- | --- |
| `InheritsFrom` | `string?` | The other state that this state inherits from. For more information, check the 'Inheritance' section. |
| `BoundingBoxSize` | `Vector2` | The size of the bounding box of this monster. |
| `Defense` | `int` | The defense of the monster. Defense is subtracted from incoming damage. |
| `Movement` | `MovementData` | Controls how the monster moves. See the `MovementData` section for more information. |
| `IsGlider` | `bool` | Whether or not the monster "glides". A gliding monster goes over obstacles. |
| `CanReceiveDamageFromPlayer` | `bool` or `string` | If true, the monster can be hurt by player attacks. If false, they cannot. If a `string`, the weapon must have the given enchantment for damage to take effect. |
| `CanReceiveDamageFromBomb` | `bool` | Whether or not the monster can be hurt from bombs. |
| `IncomingDamageMultiplier` | `double` <!-- technically a float but whatever --> | A multiplier for incoming damage. |
| `HitSound` | `string` | The ID of the sound to make when taking damage. |
| `Animation` | `AnimationData` | Controls the animation and display of the monster. See the `AnimationData` section for more information. |
| `ContactDamage` | `int?` | The damage that should be done to  the player upon contact, if any. |
| `ApplyDebuff` | `int?` | The vanilla ID of the debuff that should be applied to players on contact. |
| `Events` | `EventData[]` | Events that can trigger state changes, among other things. See the `EventData` section for more information. |

#### MovementData
This controls how the monster moves during the current state.

| Field | Type | Description |
| --- | --- | --- |
| `Direction` | `Enum<Fixed, TowardPlayer, Pathfind>` | Which way the monster should move. |
| `Style` | `Enum<Constant, Dash>` | If `Direction` is not `Pathfind`, controls how the monster should move. |
| `FixedDirection` | `Vector` | If `Direction` is `Fixed`, what direction the monster should move. |
| `MovementSpeed` | `double` <!-- technically a float but whatever --> | The movement speed of the monster. |
| `DashFriction` | `double` <!-- technically a float but whatever --> | If `Style` is `Dash`, the rate the movement speed should be moved after starting a dash. (A new dash starts when speed reaches 0.) |

#### AnimationData
This controls how the monster looks during the current state.

| Field | Type | Description |
| --- | --- | --- |
| `SpriteSheet` | `string` | The path to the spritesheet to pull from. Must include mod-unique ID, ie. `spacechase0.ExampleContentPack/monster.png` |
| `FrameSize` | `Vector2` | The size of each frame on the spritesheet. |
| `StartingIndex` | `int` | The frame on the spritesheet to start the animation. |
| `AnimationLength` | `int` | The length of the animation, in frames. Default is `1`. |
| `TicksPerFrame` | `int` | The length of each frame, in ticks. There are 60 ticks per second. Default is `1`. |
| `Loops` | `bool` | Whether or not the animation loops at the end. |
| `Origin` | `Vector2` | The origin of the sprite relative to the top left of the bounding box. (This means that the sprite will be offset from the bounding box's top-left corner by this amount.) |
| `DeathChunkStartCoordinates` | `Vector2` | The pixel coordinates of the 4 deatch chunks. They must all be in a 16 by 16 pixel area, each taking up 4 by 4 pixels. |

#### EventData
Events are what drive a mob. They dictate when a mob should do what, like transition to another state.

| Field | Type | Description |
| --- | --- | --- |
| `Event` | `EventType` | The type of event for these actions. See table below this one. |
| `When` | `string` | Controls if these actions should trigger at all on this event. It must be an equation that results in a `bool` (or just true if you want it to always run). See the 'Variables' section for more information. |
| ... | string... | The rest of this object should be the actions you want to take on this event when the `When` equation is true. See the 'Actions' section for more information. |

Note that these are also per-state, meaning they will only trigger when the current state is the containing one (or some inheriting these values, see 'Inheritance').

`EventType` values:
| EventType value | Description |
| --- | --- |
| `OnStart` | When this state first starts. |
| `OnTick` | Triggers every tick during this state. |
| `OnHurtByBomb` | When the monster is hurt by a bomb. |
| `OnHurtByPlayer` | When the monster is hurt by a player. |
| `OnHitPlayer` | When the monster hits a player via contact damage. |
| `OnCollision` | When the monster collides with something solid (not a player). |
| `OnAnimationEnd` | When the current animation ends. |

An example `EventData`, which makes the mob explode with a radius of 5 on death (see 'Actions').

```json
{
    "Event": "OnDeath",
    "Explosion": 5
}
```

##### Actions
These are the available action types:

| Action key | Parameters | Description |
| --- | --- | --- |
| `State` | `string newState` | Changes the current state to the one specified by `newState`. When this action happens, no further `EventData` will be processed, even if they have the same `EventType` and `When` that would otherwise occur. |
| `Heal` | `int amount` | Heals the monster by `amount`. |
| `Explosion` | `int radius` | Causes an explosion centered on the mob wtih a radius of `radius`. |
| `Sound` | `string soundId` | Play a sound with the ID `soundId`. See the modding wiki for a list of vanilla sound IDs. |
| `Break` | n/a | Break any obstacles inside the monster's bounding box. |
| `Spawn` | `string monsterId` | Spawn a custom monster with the given `monsterId` at the current position. Spawning vanilla monsters is currently not implemented. |
| `Shoot` | ... | (not implemented yet) |
| `Variable_xyz` | `string eq` | Set variable `xyz` to a value given by the equation `eq`. The equation must result in an `int` or `double`. <!-- technically float but whatever --> See the 'Variables' section for more information. |

#### Inheritance

The first thing you should know about inheritance is it is controlled by the `InheritsFrom` field. For this mod, when state A inherits from state B, the values from state B are copied into state A, then state A's values are put in. For example:

Note that some values can be null that aren't normally indicated as such, like `BoundingBoxSize`, as long as they are filled in by inheritance.

```json
{
    // First state
    "B": {
        "Defense": 10,
        "HitSound": "skeletonHit",
        "ContactDamage": 5
    },
    "A": {
        "InheritsFrom": "B",
        "Defense": 20
    }
}
```

The above will result in the following:

```json
{
    // First state
    "B": {
        "Defense": 10,
        "HitSound": "skeletonHit",
        "ContactDamage": 5
    },
    "A": {
        "Defense": 20,
        "HitSound": "skeletonHit",
        "ContactDamage": 5
    }
}
```

You can chain inheritance, like `A <- B <- C`. Please don't do circular inheritance (ie. `A <- B <- C <- A`) or the mod will break.

Most values are overriden with inheritance, but the `Events` field is not. Instead, the `EventData` entries are applied together, with the inheritee coming first, then the inheritor. (ie. if `A <- B`, then `B` will run first, then `A`).

#### Variables
Some fields may ask for an equation.

If the equation must result in a `bool`, it needs to be a comparison, such as: `x < y` or `a = b`: `"$RANDOM < 0.05"`

If the equation must result in an `int` or `float`, it needs to be a computation, such as `x * y` or `a - b`: `"$$healCooldown + 1"`

The following built in variables exist:

| Variable | Description |
| --- | --- |
| `$HEALTH` | The amount of health the monster currently has. |
| `$STATE_TIMER` | A timer indicating how long the current state has been active, in ticks. |
| `$CLOSEST_PLAYER` | The distance to the closest player. |
| `$RANDOM` | A random number between `[0, 1)` (including 0, excluding 1). If used multiple times in the same equation, they will all have the same value. |

To use a custom variable, use `$$` and then the variable name, such as `$$cooldown`. Note that variables are case-sensitive.

See the 'Actions' section for how to set a variable.

If a variable is used that does not have a value yet, the equation may become invalid. To set your variables on monster spawn, you may want a `start` state that immediately sets the variables and goes to a "real" state, ie.
```json
"start": {
    "InheritsFrom": "base", // Sets a bunch of other fields required for states, see 'Inheritance' for more information
    "Events": [
        {
            "Event": "OnStart", // When this state starts...
            "Variable_healCooldown": 0, // Set our variable to its initial value.
            "State": "turn_random" // Immediately go to a state that we want to use normally.
        }
    ]
},
```
