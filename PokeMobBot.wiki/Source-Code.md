# Getting Started - Source Code Setup
Note: You will need some basic Computer Experience.
Need help? Join the [Chat](https://github.com/NecronomiconCoding/NecroBot/wiki/Chat-&-Rules#chatting-using-discord)! The Issue Tracker is not for help!

## These instructions are only for individuals who wish to modify the source code. If you want to just run the bot, please go [here](https://github.com/NecronomiconCoding/NecroBot/wiki/Compiled-Releases).

### Using GitHub pull and repository
#### **Downloading necessary files**
Download and Install [Visual Studio 2015](https://go.microsoft.com/fwlink/?LinkId=691978&clcid=0x409).

#### Source code as zip-files
1. Get the current source code from [Code](https://github.com/NecronomiconCoding/NecroBot/archive/master.zip).
2. Get the current version of the RocketAPI we use from [RocketAPI](https://github.com/FeroxRev/Pokemon-Go-Rocket-API/archive/master.zip)
3. Unzip the source code for NecroBot
4. Unzip the RocketAPI so the folder structure matches `NecroBot\FeroxRev\PokemonGo.RocketAPI\`
	
#### Source code via git (faster, always up2date)
1. open your terminal (you should have [git for windows](https://git-for-windows.github.io) installed)
2. change to the folder where you want to clone the files to and type into terminal
3. `git clone --recursive https://github.com/NecronomiconCoding/NecroBot.git`

#### Update source code to latest
1. Ensure you are in NecroBot root directory
2. Run `git submodule update --force`

#### Pull specific commit or pull request
1. Ensure you are in Necrobot root directory
2. Run `git fetch origin pull/ID/head` 

#### Setup
1. Open `NecroBot for Pokemon Go.sln`.
2. Right click on `PoGo.NecroBot.CLI` and click on `Set as Startup Project`.
3. Press `CTRL + F5`
 * If this is the first execution of NecroBot in this directory, it will generate the config/auth JSON files and then automatically close.
4. Navigate to the bin/debug/Config/auth.json file
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
8. Edit bin/debug/Config/config.json with your desired settings
 * The defaults settings are generic so you WILL have to modify these to match what you expect from NecroBot.
 * More details on these settings can be found [here](https://github.com/NecronomiconCoding/NecroBot/wiki/Config)
 * For GPX Path Setup, follow the guide at [GPX Pathing Setup](https://github.com/NecronomiconCoding/NecroBot/wiki/Getting-Started#gpx-pathing-setup)
 * For Sniping Setup, follow the guide at [Sniping Setup](https://github.com/NecronomiconCoding/NecroBot/wiki/Sniping-Setup)
9. Put your latitude and longitude values in the `DefaultLatitude` and `DefaultLongitude` variables 
 * You can find GPS coordinates [here](http://mondeca.com/index.php/en/any-place-en) to fit your desired location.
10. Save the config.json file
11. Press `CTRL + F5` and follow the instructions.
 * If you are using a Google account to login which is setup with 2-factor authentication, NecroBot will open a Google page in your default browswer and request for your 2-factor token.


**NOTE:** If PogoProtos is missing: Its in the packages folder, just right click the CLI -> Add Reference -> Browse (tab) -> Browse (button) -> go to the packages folder in the root directory and find the PogoProtos
***

## Changing the Location of the Bot
1. Get a new latitude and longitude.
2. If your Bot is running, close it.
3. Change the value of `DefaultLatitude` and `DefaultLongitude` in your `NecroBot\PoGo.NecroBot.CLI\bin\Debug\Config\config.json` file
4. Run the bot