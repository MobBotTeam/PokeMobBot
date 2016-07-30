# Getting Started - Compiled Release Setup
Note: You will need some basic Computer Experience.
Need help? Join the [Chat](https://github.com/NecronomiconCoding/NecroBot/wiki/Chat-&-Rules#chatting-using-discord)! The Issue Tracker is not for help!

***
## Installation & Configuration

### Compiled release steps are recommended for any end-user who has no intention of modifying the source code.


### Using compiled release
1. Download the latest release [Release.zip](https://github.com/NecronomiconCoding/NecroBot/releases).
2. Unzip the downloaded files, run the program (NecroBot.exe)
3. A console window will appear and then soon close. This is generating the config/auth JSON files.
4. Navigate to the Config/auth.json file
5. Change AuthType to `google` or `ptc` based on your login type
6. Enter your username and password with quotes around them. You can find an example below.
 * For Google logins, use `GoogleUsername` and `GooglePassword`.
   - If the Google login uses 2-factor authentication, you will be prompted when Necrobot attempts to login.
 * For PTC logins, use `PTCUsername` and `PTCPassword`.

     ```JSON
{
  "AuthType": "google",
  "GoogleRefreshToken": null,
  "PtcUsername": null,
  "PtcPassword": null,
  "GoogleUsername": "ReachForTheSkies@gmail.com",
  "GooglePassword": "howdyhowdyhowdy"
}
```

7. Save the auth.json file
8. Edit Config/config.json with your desired settings
 * The defaults settings are generic so you WILL have to modify these to match what you expect from NecroBot.
 * More details on these settings can be found [here](https://github.com/NecronomiconCoding/NecroBot/wiki/Config)
 * For GPX Path Setup, follow the guide at [GPX Pathing Setup](https://github.com/NecronomiconCoding/NecroBot/wiki/Getting-Started#gpx-pathing-setup)
 * For Sniping Setup, follow the guide at [Sniping Setup](https://github.com/NecronomiconCoding/NecroBot/wiki/Sniping-Setup)
9. Put your latitude and longitude values in the `DefaultLatitude` and `DefaultLongitude` variables 
 * You can find GPS coordinates [here](http://mondeca.com/index.php/en/any-place-en) to fit your desired location.
10. Save the config.json file
11. Run `NecroBot` again
 * If you are using a Google account to login which is setup with 2-factor authentication, NecroBot will open a Google page in your default browswer and request for your 2-factor token.
11. Enjoy!

## Changing the Location of the Bot
1. Get a new latitude and longitude.
2. If your Bot is running, close it.
3. Change the value of `DefaultLatitude` and `DefaultLongitude` in your `Config/config.json` file
4. Run the bot