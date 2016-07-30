###Scheduled Intervals
#### Non-GPX
If `UseGPXPathing` is set to `false`,
Sniping will execute every Pokestop.

#### GPX
If `UseGPXPathing` is set to `true` and `GPXFile` is found,
Sniping will execute every second or based on the value in milliseconds for `MinDelayBetweenSnipes` (whichever is higher).

### Description
With this setup, Necrobot will rely on a seperate location feeder service instead of the config. This service will run on the same machine as Necrobot to supply dynamic Pokemon locations from the Discord server channels for sniping.

###Configuration

####PokeFeeder Setup
1. Download the [newest PokeFeeder release](https://github.com/5andr0/PogoLocationFeeder/releases/latest)
2. Unzip the PokeFeeder release
3. Execute `PogoLocationFeeder.exe`
 * On first execution, it will immediately close and create a config.json file
4. Open the newly created `config.json` file.
5. Edit `DiscordUser` (Your email address) and `DiscordPassword` with user credentials of a Discord account
 * NOTE: The feeder service is only going to search for channels that the Discord account has previously joined. If the account is new, it will not search any servers.
 * The feeder service does support multiple servers and multiple channels.
6. Add `90_plus_iv` and `coordsbots` (for the NecroBot Discord server) entries such that it looks like below:

    ```JSON 
  "ServerChannels": [
    "90_plus_iv",
    "coordsbots"
  ],
```
7. Close and save config.json
8. Run `PogoLocationFeeder.exe` again
9.  You should see a "Listening..." notification.

### Necrobot Setup

1. Open Necrobot's Config.json
2. Make sure your values are set like below:

    ```JSON
  "SnipeAtPokestops": true,
  "SnipeIgnoreUnknownIv": true,
  "SnipeLocationServer": "localhost",
  "SnipeLocationServerPort": 16969,
  "UseSnipeLocationServer": true,
```
3. Run Necrobot
4. Pogofeeder should give you a new notification about a connection.
5. If you start getting notifications about Pokemon in PogoFeeder, you set it up correctly. You should see Necrobot attempt to catch some of the Pokemon, but it will not catch them all depending on how quick Pogofeeder finds the coordinates.