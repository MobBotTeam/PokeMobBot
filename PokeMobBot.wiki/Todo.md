# Bug fixes

* GoogleAuth not saving/loading

# Feature

## Auto-update

In the auto-update branch you will find the current work in progress of the auto-update.
It needs to download Release.zip from the latest release, keep the old config files and replace the running binaries with what's in Release.zip.

## Websockets

We want to expose a websocket interface so that we can start working on HTML/JS interfaces.

## Speed controls

All the Thread.Sleep and Task.Delay calls need to be configurable via ILogicSettings.

## Serialize/Deserialize settings

We want all settings to be serialize to JSON, both ClientSettings and LogicSettings should be serialized to the same file.

## Log4net

The current Logger system isn't great, we want to replace it with log4net.