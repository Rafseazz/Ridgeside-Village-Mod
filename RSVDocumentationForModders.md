# RSV Extra Features
Ridgeside Village has a bunch of extra features that other mods can utilize for their own mods as long as Ridgeside Village is installed.

**List of features**
* Showing Images with Tile Action
* Adding new Trash Cans
* Editing the Ridgeside World Map
* Wedding Reception / Anniversary Picture
* New Event Precondition
* New Event Commands

***

## Showing Images with Tile Action

1. Create an Object Tile in the Buildings Layer in Tiled
2. Add a string custom property
3. Input this property: "Action | ShowImage "gamepath" [scale (float)] [string ID]"

## Adding new Trash Cans

1. Create an Object Tile in the Buildings Layer in Tiled
2. Add a string custom property
3. Input this property: "Action | RSV.TrashCan [ID]"
 * ID can be a string, just no spaces
4. Use content patcher to EditData the filepath: "RSV/TrashCanData"
    
    * Format is "ID": "weight itemID amount/weight itemID amount"
    * itemID -1 for nothing
    * Math: Item Weight/Summation of all Weight = Probability Chance of item.

Patch:

    {
        "LogName": "Edit Trash Can Data",
        "Action": "EditData",
        "Target": "RSV/TrashCanData",
        "Entries": {
            "ID" : "65 -1 0/10 72 2/30 770 5",
        }
    }

## Editing the Ridgeside World Map

You can edit the RSVworld maps by patching;
"Assets/LooseSprites/RSVMap.png" for the village map and/or
"Assets/LooseSprites/RSVWorldMap.png" for the RSV world map.

## Wedding Reception / Anniversary Picture

Mod authors can add their own NPC's wedding reception image with the simple CP patch

    {
        "Action": "EditImage",
        "Target": "Maps/z_RSVspousePic",
        "FromFile": "assets/Abigail.png", //Your spouse's picture! It should be 144x112 pixels w/ transparent background(If you want to use the default background)
        "When": {
            "Query: '{{Spouse}}' = 'Abigail'": true //Change spouse name to the name of your NPC
        },
        "Update": "OnLocationChange, OnTimeChange",
    },

They can edit existing images as well using the same patch.

## New Event Precondition

New event precondition: rsvRidingHorse
* Usage example: "75160321/rsvRidingHorse/t 900 1200": "sweet/120 50/farmer 120 50 2/skippable/pause 1000/end"
* Will trigger an event only if the player is riding their mount

## New Event Commands (Documentation WIP)

RSVAddSO: Adds a special order ID
RSVShowImage: Shows an image in the screen