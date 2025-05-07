# Refactoria

## Overview

This is the repository for Refactoria, a game that teaches refactoring techniques aimed at college students.

Interested in playing the offical game? [Click here to download](https://se4airesearch.github.io/Refactoria/).

The top-level directories are set up as follows:
* `Art/`: All the raw graphics (including early versions) included in the game
* `Audio/`: All the songs and sound effects included in the game
* `Puzzles/`: All the JSON and starter code for all the puzzles in the game
* `RefactorGameUnity/`: The Unity project
* `RefactorLang/`: The home of our toy language, parser, tokenizer, and backend interpeter

## Unity Source Code

### Open the Game
To open the game follow these instructions:
* Download and Open Unity Hub
* Install an editor
	* For development we used 2022.3.X
* Install build modules (Mac or Windows)
* Open `RefactorGameUnity/RefactorGame`

### Build Instructions
* Open the game through Unity Hub
* Select File > Build Settings
* Select the Platform you want to build for
* Select Build or Build and Run

### Debugging

If you are using VSCode, you will need to install the [Unity extension](https://marketplace.visualstudio.com/items/?itemName=VisualStudioToolsForUnity.vstuc).

You must open `RefactorGameUnity/RefactorGame` to attach the debugger while the Unity Editor is open.

## RefactorLang

### Overview

The `RefactorLang/` directory is compromised of 4 modules:
* `ParserLibrary`: Language grammar and parser in F#
* `RefactorLib`: The tokens used by the language
* `RefactorLang`: The tokenizer, backend interpreter, and structures shared with the Unity game
  * e.g., state of the kitchen, puzzle model, dictionary model, station/module models
* `RefactorLangConsole`: Play around with the tokenizer, parser, and interpreter outside of the game

## Build

### DotNet

If not already, you may have to [download](https://dotnet.microsoft.com/en-us/) .NET first.

### Command Line

`ParserLibrary`, `RefactorLib`, `RefactorLang`, and `RefactorLangConsole` all have `.csproj` files.

Navigate to the appropriate directory and execute:
```
dotnet run --project {fileName}.csproj
```
*Replace `{fileName}` with the name of whichever file.*

### VSCode

If you are using VSCode, it is recommend to install the [C# Dev Kit](https://marketplace.visualstudio.com/items/?itemName=ms-dotnettools.csdevkit), [Iodine for F#](https://open-vsx.org/vscode/item?itemName=Ionide.Ionide-fsharp) and [.NET Install Tool](https://marketplace.visualstudio.com/items/?itemName=ms-dotnettools.vscode-dotnet-runtime) extensions.

You can build it by following these instructions:
* Open the root directory of the repository
* Press Cmd+Shift+P or Ctrl+Shift+P
* Type/Select ".NET: Open Solution"
* Select "RefactorLang/RefactorLang.sln"
* Press Cmd+Shift+P or Ctrl+Shift+P
* Type/Select "MSBuild: Build current solution"