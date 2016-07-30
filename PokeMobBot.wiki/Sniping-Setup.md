## Sniping Setup

### Implement the sniping setup that best suits you

###Scheduled Intervals
#### Non-GPX
If `UseGPXPathing` is set to `false`,
Sniping will execute every Pokestop.

#### GPX
If `UseGPXPathing` is set to `true` and `GPXFile` is found,
Sniping will execute every second or based on the value in milliseconds for`MinDelayBetweenSnipes` (whichever is higher).

## [Manual Location Sniping Setup](https://github.com/NecronomiconCoding/NecroBot/wiki/Manual-Locations-Sniping-Guide)
####Description
Defined by the scheduled interval above, NecroBot will utilize PokeVision to scan each location listed and catch the Pokemon specified in the configuration. This feature is NOT intended for catching individual one-off Pokemon, but instead for teleporting to pre-defined nests that have been known to contain specific Pokemon spawns.

## [Automatic Location Sniping Setup](https://github.com/NecronomiconCoding/NecroBot/wiki/Automatic-Locations-Sniping-Guide)
#### Description
With this setup, Necrobot will rely on a seperate location feeder service instead of the config. This service will run on the same machine as Necrobot to supply dynamic Pokemon locations from the Discord server channels for sniping.


