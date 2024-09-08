# Treasure Solver

The plugin solves treasure hunts using community data and integrates the result in the treasure hunt widget.

![Treasure Hunt widget](https://raw.githubusercontent.com/Dofus-Batteries-Included/DBI/main/img/treasure_hunt_widget.png)

## Configuration

- Treasure Hunt - Solver: 
  - __Remote__: (online) the [Treasure Solver API](https://api.dofusbatteriesincluded/treasure-solver) is used to find the position of the next clue.
  - __Local__: (offline) use the pre-packaged clues from [Dofus pour les noobs website](https://www.dofuspourlesnoobs.com/resolution-de-chasse-aux-tresors.html) (see `Resources/dofuspourlesnoobs_clues.json`), and the local clues store that is updated every time clues are validated in the game. \
  This finder is offline: it will always use local data, it will never call an remote API. 
  The local data can still be updated:
    - The Dofus pour les noobs file is not embedded in the plugin: it can be updated without rebuilding the whole plugin.
    - The local store is updated continuously: if a clue is unknown, and is found by the player, its position will be saved and reused. If a clue has disappeared, a failed validation will mark it as removed so that it is not suggested again.
   