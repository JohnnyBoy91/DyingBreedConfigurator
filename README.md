# DyingBreedConfigurator
A mod for customizing Dying Breed to your liking
## Features

* Customizable unit and structure stats
* Customizable attack-armor damage modifier table
* Customizable harvester fill rate, quantity, and capacity

#  
* Full config lists here: https://github.com/JohnnyBoy91/DyingBreedConfigurator/tree/main/DyingBreedModding/Templates

## Guide
* The mod contains .json text files, simply edit the values in the mod jsons(i.e. ModUnitData.json) and they will be loaded when you start playing
* For your convenience, the game's default values have been provided in a subfolder called "DefaultConfigData"(i.e. DefaultUnitData.json), use these as a backup or to compare/restore vanilla values. These default values are automatically updated when you play the game
* The mod does not alter any vanilla game files, but instead applies the changes at runtime. Your adjustments remain between balance patches.

## Download

https://github.com/JohnnyBoy91/DyingBreedConfigurator/releases

## Source Code

https://github.com/JohnnyBoy91/DyingBreedConfigurator/blob/main/DyingBreedModding/ModManager.cs

## Installation & Setup Guide
 
Requires BepInEx Bleeding Edge build for IL2CPP  
https://builds.bepinex.dev/projects/bepinex_be

Tested on Build# 738, June 20th 2025 af0cba7, "BepInEx Unity (IL2CPP) for Windows (x64) games"  
BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.738+af0cba7

Follow Installation Guide for IL2CPP Unity
https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html
1. Download Bepinex and confirm you have correct version
2. Extract into game root folder
3. Run the game and allow some time to generate modding files
4. Download this mod from the "Releases" section
5. Drop this mod into "Bepinex/plugins" folder.

## Uninstallation

* To remove completely, delete bepinex folder and files from game directory(the folder and files added during installation step 3). May have to also verify game files on steam.
* To disable the mod temporarily, move the mod folder out of the "Bepinex/plugins" folder. I recommend creating a "pluginsoff" folder to switch easily if you find yourself wanting to switch back to vanilla gameplay often.
