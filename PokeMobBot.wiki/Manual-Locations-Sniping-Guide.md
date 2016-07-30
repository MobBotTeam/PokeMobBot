###Scheduled Intervals
#### Non-GPX
If `UseGPXPathing` is set to `false`,
Sniping will execute every Pokestop.

#### GPX
If `UseGPXPathing` is set to `true` and `GPXFile` is found,
Sniping will execute every second or based on the value in milliseconds for`MinDelayBetweenSnipes` (whichever is higher).

### Description
Defined by the scheduled interval above, NecroBot will utilize PokeVision to scan each location listed and catch the Pokemon specified in the configuration. This feature is NOT intended for catching individual one-off Pokemon, but instead for teleporting to pre-defined nests that have been known to contain specific Pokemon spawns.

###Configuration
Each configuration will not be applied until NecroBot is restarted.

1. Enable sniping by setting `SnipeAtPokestops` to `true`
2. Navigate to the config.json located in the Config folder and locate the  `PokemonToSnipe` list
3. Locate the `Locations` section in the `PokemonToSnipe` list. This is where we begin setting up our sniping points. Check the [Resources](https://github.com/NecronomiconCoding/NecroBot/wiki/Resources) section for locations. You can list multiple locations for the NecroBot to check.
An example of this would be:

   ```JSON
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
  }
],
```

4. Locate the `Pokemon` section in the `PokemonToSnipe` list. This is where we are able to set our list of Pokemon we would like the NecroBot to search for at any of the above locations before returning to the default location. 
An example of this would be:

   ```JSON
    "Pokemon": [
      "Pikachu",
      "Venusaur",
      "Magnemite",
      "Abra"
    ]
```

5. Once you are done fine tuning all of your settings, save the config.json and run NecroBot.exe. If Necrobot crashes on startup after configuring sniping, you need to verify that there are no typos in your spelling or syntax. If Necrobot is running, remember it must be restarted to push any config updates. 

#### Common Mistakes

Do not add too many Pokemon or locations to the `PokemonToSnipe` list. When NecroBot teleports, it will not claim any Pokestops and since nests tend to be full of Pokemon, NecroBot will quickly run out of Pokeballs. Intelligently filter the Pokemon to catch or keep your locations minimal.

An example of a syntax mistake would be:

A) Typing Mr. Mime or Mr Mime where the correct syntax would be MrMime.

B) Typing a name incorrectly, Pikochu as opposed to the correct Pikachu.

Both of these will force NecroBot to crash.