Each of the following files are populated at different stages of the bots runtime.

#Table of Contents
1. [Config.json](#config.json)
    * [Location](#location)
	* [Evolution](#evolution)
	* [Upgrading](#upgrading)
	* [Transferring](#transferring)
	* [Catching](#catching)
	* [Sniping](#sniping)
	* [Recycling/Items](#recyclingitems)
	* [Other](#other)
2. [Auth.json](#auth.json)

# Config.json

###Scheduled Intervals
#### Non-GPX
If `UseGPXPathing` is set to `false`,
Evolution, upgrading, transferring and recycling will execute every 5th Pokestop.
Sniping will execute every Pokestop.

#### GPX
If `UseGPXPathing` is set to `true` and `GPXFile` is found,
Evolution, updating, transferring, recycling and sniping will execute every second or based on the value in milliseconds for`MinDelayBetweenSnipes` (whichever is higher).

## Location
#### DefaultAltitude (Value)
Altitude that the bot starts at. Not advised to change this from the default as Niantic does not check altitude.

#### DefaultLatitude (Value)
Latitude that the bot starts at. Must be between -90 and 90 values.

#### Default Longitude (Value)
Longitude that the bot starts at. Must be between -180 and 180 values

#### WalkingSpeedInKilometerPerHour
Obvious walking speed in kilometers per hour

#### MaxTravelDistanceInMeters (Value)
In meters, how far the bot will travel in a radius from the original default location.

#### UseGPXPathing (Value)
When `UseGPXPathing` is set to `true`, the bot will walk to the first point in the GPX path defined by the value in `GPXFile` and traverse each point. By default the bot wanders around the radius defined in `MaxTravelDistanceInMeters`.

#### GPXFile
Only used when `UseGPXPathing` is set to TRUE
Path of the GPX file relative to the .exe file. Place the GPX file in the same directory as the exe

#### MaxSpawnLocationOffset (Value)
When NecroBot boots, the default location will be offset based on this multiplier. Most users will not need to modify this.

## Evolution
### Executes based on the [scheduled interval](#scheduled-intervals)
#### EvolveAllPokemonAboveIV (Value)
When `EvolveAllPokemonAboveIV` is set to `true`, any Pokemon that is above the **EvolveAboveIVValue** value will be evolved if there are enough candies.

#### EvolveAboveIVValue (Value)
Only when `EvolveAllPokemonAboveIV` is set to `true`
Value used to determine what IV Pokemon should be automatically evolved at.

#### EvolveAllPokemonWithEnoughCandy (Value)
When `EvolveAllPokemonWithEnoughCandy` is set to `true`, any Pokemon listed in the PokemonsToEvolve list will be evolved every 5 Pokestops limited by the number of Pokemon and candies you have. If this is not set, Pokemon will not be evolved based on this list criteria.

#### PokemonsToEvolve (List)
Only used if `EvolveAllPokemonWithEnoughCandy` is set to true.
Comma-seperated quote-wrapped list of Pokemon names that distinguish which Pokemon to automatically evolve if there are enough candies.

#### KeepPokemonsThatCanEvolve (Value)
When NecroBot determines how many Pokemon to transfer due to duplicates, it will create a count based on the number of candies the account has for that species compared to the number required to evolve.

For example, if the account has 5 Pidgeys (12 candies to evolve) and 24 candies, NecroBot will keep the top two Pidgeys and transfer the rest.

##Upgrading
### Executes based on the [scheduled interval](#scheduled-intervals)
#### AutomaticallyLevelUpPokemon (Value)
When `AutomaticallyLevelUpPokemon ` is set to `true`, a randomly selected Pokemon will be leveled up one step based on the scheduled interval. 

#### LevelUpByCPorIv (Value)
NOTE: Only if`AutomaticallyLevelUpPokemon ` is set to `true`
This option specifies whether to use `UpgradePokemonCpMinimum` or `UpgradePokemonIvMinimum` when populating the list of potential Pokemon to level up.

#### UpgradePokemonCpMinimum (Value)
NOTE: Only if`AutomaticallyLevelUpPokemon ` is set to `true` and `LevelUpByCPorIV` is set to `cp`
Any Pokemon above this threshold in CP could be randomly selected to be leveled up by one step.

#### UpgradePokemonIvMinimum (Value)
NOTE: Only if`AutomaticallyLevelUpPokemon ` is set to `true` and `LevelUpByCPorIV` is set to `iv`
Any Pokemon above this threshold in IV could be randomly selected to be leveled up by one step.

## Transferring
### Executes based on the [scheduled interval](#scheduled-intervals)
#### PrioritizeIVOverCP (Value)
When `PrioritizeIVOverCP` is set to `true`, the bot will sort by IV instead of CP when deciding which Pokemon to transfer out of a group of duplicate Pokemon. By default, the bot sorts by CP.

#### TransferDuplicatePokemon (Value)
When `TransferDuplicatePokemon` is set to `true`, any Pokemon that meets the below criteria is transferred:
* Total number of that Pokemon species in your inventory is greater than `KeepMinDuplicatePokemon`
* Below the `KeepMinIVPercentage` value
* Below the `KeepMinCP` value
* Is not listed in your `PokemonsNotToTransfer` list

Note that any of the first three criteria are global values and they CAN be overwritten by the Pokemon-specific settings in the `PokemonsTransferFilter` list.

#### KeepMinDuplicatePokemon (Value)
NOTE: Only used if `TransferDuplicatePokemon` is set to `true`
This is a global setting that applies to any Pokemon that has not been overridden by an entry in the `PokemonsTransferFilter` list.

This value specifies how many duplicates of each Pokemon to keep when deciding what Pokemon to transfer. This number can be Pokemon-specific by creating an entry in the `PokemonsTransferFilter` list. A Pokemon can also be excluded from transfer with the `PokemonsNotToTransfer` list.

#### KeepMinIVPercentage (Value)
NOTE: Only used if `TransferDuplicatePokemon` is set to `true`
This is a global setting that applies to any Pokemon that has not been overridden by an entry in the `PokemonsTransferFilter` list.

This value specifies that a Pokemon that has an IV higher than this value will not be transferred. This number can be Pokemon-specific by creating an entry in the `PokemonsTransferFilter` list. A Pokemon can also be excluded from transfer with the `PokemonsNotToTransfer` list.

#### KeepMinCP (Value)
NOTE: Only used if `TransferDuplicatePokemon` is set to `true`
This is a global setting that applies to any Pokemon that has not been overridden by an entry in the `PokemonsTransferFilter` list.

This value specifies that a Pokemon that has an CP higher than this value will not be transferred. This number can be Pokemon-specific by creating an entry in the `PokemonsTransferFilter` list. A Pokemon can also be excluded from transfer with the `PokemonsNotToTransfer` list.

#### PokemonsNotToTransfer (List)
Only used if `TransferDuplicatePokemon` is set to `true`
Comma-seperated quote-wrapped list of Pokemon names that distinguish what Pokemon to keep regardless of their IV or CP so that they are not transferred. Pokemon in this list should never be transferred by NecroBot.

#### PokemonTransferFilter (List)
Only used if `TransferDuplicatePokemon` is set to `true`
This list of Pokemon names lets you set the `KeepMinDuplicatePokemon`, `KeepMinIVPercentage`, and `KeepMinCP` values specific to each Pokemon. This lets you override the default global values for those.

Therefore, if you have an entry in `PokemonTransferFilter` like below:

```json
"KeepMinCp": 1000,
"KeepMinDuplicatePokemon": 1,
"KeepMinIvPercentage": 90.0,
"PokemonsTransferFilter": {
    "Pinsir": {
      "KeepMinCp": 500,
      "KeepMinIvPercentage": 70.0,
      "KeepMinDuplicatePokemon": 2
    }
}
```

then NecroBot will transfer any Pinsirs that are below 500 CP and below 70% IV while keeping 2 duplicates. 

## Catching
#### UsePokemonToNotCatchFilter (Value)
When `UsePokemonToNotCatchFilter` is set to `true`, any Pokemon listed in the `PokemonsToIgnore` list will be skipped when determining what Pokemon to catch.

#### PokemonsToIgnore (List)
Only used if `UsePokemonToNotCatchFilter` is set to `true`.
Comma-seperated quote-wrapped list of Pokemon names that distinguish what Pokemon to ignore when searching to catch.

#### MaxPokeballsPerPokemon (Value)
Specifies the number of pokeballs to use per pokemon before giving up the encounter.

#### PokemonToUseMasterball (List)
List of pokemon that Necrobot is forced to use Masterball on.

#### UseGreatBallAboveCp (Value)
If a Pokemon is above this CP, use a Great ball.

#### UseUltraBallAboveCp (Value)
If a Pokemon is above this CP, use an Ultra ball.

#### UseMasterBallAboveCp (Value)
If a Pokemon is above this CP, use a Master ball.

## Sniping
### Executes based on the [scheduled interval](#scheduled-intervals)
### Make sure to follow the [Sniping Guide](https://github.com/NecronomiconCoding/NecroBot/wiki/Sniping-Setup) for more details
#### SnipeAtPokestops (Value)
When set to 'true', NecroBot will go on a sniping journey to each of the coordinates listed in the `PokemonToSnipe` list and try to catch the Pokemon listed in that same `PokemonToSnipe` list. Pokemon are not mapped to a location. So if you have 2 coordinates and two Pokemon listed, it will catch for both Pokemon in both locations.

If `UseSnipeLocationServer` is set to `true`, Necrobot will use that for location and pokemon sniping.

#### PokemonToSnipe (List)
JSON formatted list of coordinate and Pokemon entries used by `SnipeAtPokestops` to hunt down specific locations for specific pokemon.

Example:

```JSON
"PokemonToSnipe": {
    "Locations": [
      {
        "Latitude": 38.556807486461118,
        "Longitude": -121.2383794784546
      },
      {
        "Latitude": -33.859019,
        "Longitude": 151.213098
      },
      {
        "Latitude": 47.5014969,
        "Longitude": -122.0959568
      },
      {
        "Latitude": 51.5025343,
        "Longitude": -0.2055027
      },
      {
        "Latitude": 35.668465050004,
        "Longitude": 139.70481913813
      }
    ],
    "Pokemon": [
      "Dratini",
      "Magikarp",
      "Eevee",
      "Charmander",
      "Snorlax",
      "Porygon"
    ]
  }
```

#### MinPokeballsToSnipe (Value)
When NecroBot goes on a sniping journey, it will only proceed if the number of pokeball-type items it has exceeds this value.

#### MinPokeballsWhileSnipe (Value)
During a sniping journey, NecroBot will end the journey if the number of pokeball-type items becomes less than this value when hopping between locations.

#### UseSnipeLocationServer (Value)
Toggle to use a locally hosted location feeder service that NecroBot can use to dynamically populate potential sniping locations.

See Sniping Guide for more info.

#### SnipeLocationServer (Value)
Used when `UseSnipeLocationServer ` is set to `true`
Hostname of the sniping location service. Defaults to `localhost`


#### SnipeIgnoreUnknownIv (Value)
Used when `UseSnipeLocationServer ` is set to `true`
If NecroBot cannot determine the IV value from the sniping location service and this value is set to `true`, it will snipe them anyways. If set to `false`, Necrobot will ignore the Pokemon.

#### SnipeLocationServerPort (Value)
Used when `UseSnipeLocationServer ` is set to `true`
Defines the specific port Necrobot should connect to for the `SnipeLocationServer` value.

#### UseTransferIvForSnipe (Value)
When `UseTransferIvForSnipe` is set to `true`, Necrobot will use the the global `KeepMinIvPercentage` value and `PokemonTransferFilter` list to determine if it should attempt to snipe a Pokemon.


## Recycling/Items
#### Executes based on the [scheduled interval](#scheduled-intervals)

#### UseEggIncubators (Value)
Automatically incubates eggs in order that they are listed in the inventory when the bot first runs.

#### UseLuckyEggsWhileEvolving (Value)
When `UseLuckyEggsWhileEvolving` is set to `true`, NecroBot will hold-off on evolving Pokemon until there are enough evolutions to meet the value in `useLuckyEggsMinPokemonAmount` and then proceed to use a Lucky Egg before evolving them.
Before triggering the evolve step, the bot will use a lucky egg if one has not been used in the past 30 minutes.

#### UseLuckyEggsMinPokemonAmount (Value)
Value used when evolving to determine the number of evolutions needed to trigger a Lucky Egg use.

#### ItemRecycleFilter (List)
List of max values mapped to each non-Pokeball, non-potion, and non-revive Pokemon Go item. During the recycling stage, any item that is over the max count is recycled to meet the value. This is to prevent full inventories.

#### TotalAmountOfPokebalsToKeep (Value)
Total amount of pokeball-type items to keep. Necrobot will try to intelligently prioritize higher pokeballs when farming Pokestops.

#### TotalAmountOfPotionsToKeep (Value)
Total amount of potion-type items to keep. Necrobot will try to intelligently prioritize higher potions when farming Pokestops.

#### TotalAmountOfRevivesToKeep (Value)
Total amount of revive-type items to keep. Necrobot will try to intelligently prioritize higher revives when farming Pokestops.

## Other
On startup, Necrobot displays a list of the top CP/IV Pokemon in your inventory, this setting will change how many Pokemon to display.

#### AutoUpdate (Value)
NecroBot will detect if it needs to update if set to `true`

#### DelayBetweenPokemonCatch (Value)
Delay in milliseconds between attempts to catch a Pokemon.

#### DelayBetweenPlayerActions (Value)
Delay in milliseconds between transfers/recycles/evolutions/etc.

#### DumpPokemonStats (Value)
When set to 'true', NecroBot will output a .txt file of the list of Pokemon in your inventory sorted by IV in a Dumps directory.

#### RenamePokemon (Value)
When `RenamePokemon` is set to `true`,  Pokemon are renamed based on the template defined in `RenameTemplate`.

#### RenameAboveIv (Value)
When `RenamePokemon` and `RenameAboveIv` are set to `true`, any Pokemon above the value in `KeepMinIvPercentage` is renamed.

#### RenameTemplate (Value)
Format used by NecroBot to rename Pokemon. No need to modify.

#### StartupWelcomeDelay (Value)
When NecroBot runs, if this is set to 'true', it will ask for you to verify the coordinates before proceeding. Setting it to 'false' will skip this prompt.

#### TransferConfigAndAuthOnUpdate (Value)
When Necrobot automatically updates, it will attempt to transfer any pre-existing config and auth values into the new config/auth structure. Any values that cannot be determined will be preset with the defaults.

#### TranslationLanguageCode (Value)
Translation code for Necrobot. Supported values can be found in the config/translations folder.

#### WebSocketPort (Value)
Port used by NecroBot for potential GUI implementations. Should not be modified.

# Auth.json
#### AuthType (Value)
Set to `google` for Google
Set to `ptc` for Pokemon Trainer Club

#### GoogleRefreshToken (Value)
Depreciated value not used anymore.

#### GoogleUsername (Value)
Google account username used when `AuthType` is set to `google`

#### GooglePassword
Google account password used when `AuthType` is set to `google`

#### PtcUsername (Value)
Pokemon Trainer Club username used when `AuthType` is set to `ptc`

#### PtcPassword (Value)
Pokemon Trainer Club username used when `AuthType` is set to `ptc`