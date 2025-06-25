# Procedural City Generators

This Unity project shows how to create small reimagined locations with simple procedural techniques.
Two example generators are included:

- **FantasyVillageGenerator** – spawns houses radially around a central plaza.
- **CyberCityBlockGenerator** – creates a dense city block. Building heights decrease away from the center and an optional landmark tower can be spawned.

## Getting Started
1. Open the project in **Unity 2022.3** or newer.
2. Create a new empty scene or duplicate an existing one.
3. Add an empty GameObject and attach either generator script.
4. Assign one or more modular prefabs to the script.
   Press **G** during play mode to regenerate the layout.
5. To create a landmark tower, assign a prefab with a `LandmarkTower` component to
   the `specialBuildingPrefab` field of `CyberCityBlockGenerator`.

## Files
- `Assets/Scripts/GeneralScripts/FantasyVillageGenerator.cs`
- `Assets/Scripts/GeneralScripts/CyberCityBlockGenerator.cs`
- `Assets/Scripts/ExampleGrammars/LandmarkTower/LandmarkTower.cs`

These examples build upon the supplied modular meshes and grammar-based generators in the repository.
