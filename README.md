# HuniePopArchipelago
Collection of projects for connecting/tracking HuniePop with Archipelago 


# HuniePopArchipelagoClient
Built using template from https://github.com/alwaysintreble/ArchipelagoBepInExPluginTemplate

Requires Hunie Pop APWorld ([link](https://github.com/DotsofdarknessArchipelago/HuniePop-APWorld)) to generate a world

A BepInEx plugin for Hunie Pop to connect and talk to a Archipelago server for multiworld randomization games

Note items are processed when moving between locations <br/>
F8 will bring up a console to interact with AP server and view logs

BACKUP YOUR SAVE FILE BEFORE USING THIS AS I CANT GUARANTEE THAT IT WILL NOT CORRUPT IT
(also is a good idea to back up your saves when modding any game)
- Windows save location: "C:/Users/{YOUR USERNAME}/AppData/LocalLow/HuniePot/HuniePop/"
- Mac save location: "/Users/{YOUR USERNAME}/Library/Application Support/com.HuniePot.HuniePop/"

INSTALLATION INSTRUCTIONS:

- Have Hunie Pop Installed
- Download Hunie Pop Archipelago plugin (See Releases for latest version)
- Extract and copy the contents of "Hunie Pop Archipelago plugin.zip" to the directory where "HuniePop.exe" is if it asks you to overwrite files click yes

NOTE if you get a game crash when starting the game make sure that in "{huniepop game directory}/bepinex/config/bepinex.cfg" the 2nd last option is "type = MonoBehaviour" <b><ins>not</ins></b> "type = Application"

# HuniePopArchipelago APWorld
APWorld For Hunie Pop 

Tutorials for setting up and starting a Archipelago world/server https://archipelago.gg/tutorial/

Locations checked:
 - gifting girls gifts (18 general, 6 unique per girl, 288 total)
 - taking girls on dates (4 per girl, 48 total)
 - sleeping with girls (complete a date at night) (1 per girl, 12 total)
 - giving kyu girls panties (1 per girl, 12 total)
 - learning their details (12 per girl, 144 total)

items implemented:
 - unlock condition for each girl (12 total)
 - girl panties (12 total)
 - gift items(18 general, 6 unique per girl 288 total)
 - talent/romance/flirtation/sexuality/passion/sensitivity/charisma/luck token level up items (6 each type, 48 total) (Note you can not level up these any other way)

goals:
- give kyu all available panties
- sleep with all avaliable girls

# Hunie Pop Archipelago PopTracker Plugin

Item tracker for Hunie Pop Archipelago

## Installation

Just download the latest build or source and put in your packs folder.
