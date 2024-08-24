# Treasure Solver

The plugin solves treasure hunts using community data and integrates the result in the treasure hunt widget.

![Treasure Hunt widget](https://raw.githubusercontent.com/Dofus-Batteries-Included/DBI/main/img/treasure_hunt_widget.png)

## Configuration

- Clue Finder: 
  - DofusPourLesNoobs: (offline) use the pre-packaged clues from [Dofus pour les noobs website](https://www.dofuspourlesnoobs.com/resolution-de-chasse-aux-tresors.html), see `Resources/dofuspourlesnoobs_clues.json`.\
  This finder is offline: it will always use the pre-packaged file that is included. However the file is not embedded in the plugin so that it can be updated without rebuilding the whole plugin.
  - Dofus Map Hunt: (online) use the [Dofus Map website](https://dofus-map.com/fr/hunt) to find clues.\
  **NOTE**: doesn't work, don't use this.
  - Treasure Solver: (online, WIP not available yet) I am planning to create my own treasure hunt solver service and use it instead. The main advantage is that the plugin can automatically report valid and invalid clues whenever the player confirms their clues.