# building-tools

This is a Unity package that provides editor tools and run-time components designed to help with configuring and optimising buildings in Unity.

# Installation

The package relies on a number of scripts and resources in the "daftapple-core" package. You must install this first through package manager:

1. Open the package manager window.
2. From the menu in the top left, pick "Install package from git URL..."
3. Copy and paste this URL: `https://github.com/mroshaw/daftapple-core.git`
4. Click install.

You can now install the "building-tools" package by following the same process with this URL:

`https://github.com/mroshaw/3dforge-tools.git`

The package has these Unity dependencies, which you'll be prompted to install:

- Splines 2.7.2 (com.unity.splines)







## Components

These components can be added to your game objects and prefabs to provide specific functionality.

### Building

A core component that must be added to the root of any building game object or prefab. This provides methods and properties that are used by the other components in the package, some of which must be manually configured.

#### Properties



#### Methods

#### Usage

### Door

A component that turns the game object on which it resides into an opening and closing door, triggered by attached `DoorTrigger` components and `BoxColliders`.

#### Properties

#### Methods

#### Usage

### DoorTrigger

### BuildingLight

### BuildingLightController

### MeshCombineExcluder

## Editor Tools

The editors provide tools that modify, configure and tune selected game objects, prefab instances and prefab assets. The editor windows are accessible via the "Daft Apple Games" menu in the Unity editor.

### Building Editor Window

### Optimisation Editor Window

### Lighting Editor Window

