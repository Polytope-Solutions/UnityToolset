# Change Log

---

## 0.0.24

2023-12-14

-	ObjectHelpers_UnityObjects:
	-	Add options to Object Finding to have prototype or component list;
	-	Add option to find Components by type not generic type;
	-	Create easy accessors to debug log compactly and consistently;
-	UIFPS:
	-	Add option to choose between deltaTime or unscaled deltaTime;
	-	Add option in show lowest FPS in buffer;

---

## 0.0.23

2023-11-29

-	Create UIToWorld Input Receiver and Handler abstract prototype - to allow to drag from UI into world;
-	Allow for InputReceivers to be activated exlusively, by temporarily disabling the other receivers;
-	Bring in and clean up Solver functionality:
	-	Add ISolutionDescriptor interface;
	-	Bring in common Tick functionality in the Solvers;
	-	Bring in Input Controller for the Solvers;
	-	GEneral clean up, reorganization and more consistent naming;
-	Add generic Coroutine to Invoke next frame in ObjectHelpers_UnityObjects;

---

## 0.0.22

2023-11-27

-	Switch TManager to more consistent naming and add default Awake - to ensure instance is set as early as possible;
-	Add discarding for Extended Touch when starting over UI;
-	Remove unnecessary delegates;
-	Split Mesh Generation tools into own static class MeshTools and merge with Aggregator;
-	Add generic tool to set images in UI in ObjectHelpers_UI;
-	Put more explicit names for Unity Helper functions in ObjectHelpers_UnityObjects;
-	Add quit utility;

---

## 0.0.21

2023-11-16

-	Expand Geo Converters:
	-	Add Parent, Children and Sibling indicies calculations;
	-	Add separate Tile Index converters (as axis flip, 0 fraction resulted in wrong reference in other system);
-	Debug Scene Manager:
	-	Scene Loader changed to Start as OnEnable can happen before Scene Manager's Awake;
	-	Wait for previous scene to activate if for some reason it didn't activate yet;
	-	Unload assets once not with every unloaded scene;

---

## 0.0.20

2023-11-15

-	Introduce Concurrent Task Controller for threaded operations with limited amount of sub-tasks;
-	Expand Scene Manager Extender: 	
	-	Improve order of operations;
	-	Simplify callbacks;
	-	Introduce blocking coroutine for scene Setup;
	-	Create manually controlled previous and next scene names accessors (to address uncertain order of Unity's operations);
	-	More precise progress calculation based on amount of operations to perform;
	-	Unload all loaded scenes not just the active one (in case switched before previous switching finished);
	-	Unload assets when unloading a scene;
-	Fix error in Geo converter: GameTile to Game;

---

## 0.0.19

2023-11-13

-	Fix Scene loading for builds (didn't waot enough time to load scene before setting it active);
-	Redo Geography converters: 		-	Switch to fractional tile indices encoding UVs directly;
	-	Simplify and minimize cross calls;
	-	Improve precision;

---

## 0.0.18

2023-11-10

-	Fix Third Person Camera Controller to not allow rig to zoom out pushing the target;
-	Extend Scene Manager Extender:	-	Add OnSceneLoaded and OnSceneUnloaded events;
	-	Make loading scene additive - for smooth transitioning;
	-	Make unloading old scene asyncronous;
-	Add Loading Scene camera Preset;
-	Add Loading Screen script to smoothly animate it in and out;
-	Smooth out loading bar transitions;
-	Add preliminary Spawning system.

---

## 0.0.17

2023-11-09

-	Introduced InputManager and abstract InputReceiver to prepare for multiple input handlers;
-	Switched Camera Controller to implement InputReceiver;
-	Expanded Geography Converters: filled in missing ones, accounted for base coordinate offset, handled inverted axis and fix non-square aspect ratio of tiles in geo-cooridnate space;
-	Filled in ChangeLog from the beginning.

---

## 0.0.16

-	Introduced Camera controller abstract prototype and abstract away common First and Third Person functionality;
-	Clean Camera Controller code;
-	Add Quaternion to Vector4 Converter.

---

## 0.0.15

-	Create ExtendedTouch virtual device interfacing with regular touch events and firing separate events for single and dual finger interactions;
-	Add ExtendedTouchEmulatorActivator to bind to touchscreen if present and initialize ExtendedTouch in PlayMode;
-	Expand default InputActions to use ExtendedTouch events;
-	Switch Camera Controllers to performed actions instead of started to be able to handle continuously changing touch inputs (unlike with keyboard which is descrete);
-	Expand and bug fixes for Geo Conversion.

---

## 0.0.14

-	Unify First and Third person camera controllers;
-	Create rectangular and square grid mesh generator in ObjectHelers_Geometry;
-	Create generic Texture2D sampler in ObjectHelpers_Texture.

---

## 0.0.13

-	Create static vector converters in ObjectHelers_Geometry;
-	Introduce basic Geography helpers in ObjectHelers_Geography;
-	Create basic Third Person Camera Controller.

---

## 0.0.12

-	Small bug fixes in Grid and Element classes (addd some virtual accessors and functions);
-	Switch Solver Elements to int state tracking (instead of separate bools);
-	Create basic UI FPS counter.

---

## 0.0.11

-	Introduce basic First Person Camera Controller;
-	Introduce basic Events for Camera Controllers;
-	Add Input System Dependency to the package.

---

## 0.0.10

-	Fix Obj Encoder failing in some locales;
-	Add WebGL helper and javascript to interact with browser to save files.

---

## 0.0.9

-	Small improvements to Solver prototype;
-	Add a Toggle for animator bool property;
-	Create basic OBJ exporter;
-	OBJ exporter: optimize (exclude uvs and normals if not set) and expand (store texture properties);
-	Mesh Aggregator: handle bigger meshes, copy more mesh properties, account for mesh holder transformations.

---

## 0.0.8

-	Small improvements to Solver prototype (delay auto solve on clear to let clean up finish);
-	Create basic CSV Parser.

---

## 0.0.7

-	Create test UV Texture;
-	Start Mesh Aggregator;
-	Add Weighted prng sampler;
-	Generalize Solvers to have parent Sovlers that is depends on;
-	Small fixes to Element Lists and Solvers;
-	Add an Object Toggle;
-	Introduce Scene Manager Extender, Scene Loader and Loading Bar to simplify scene management;
-	Allow Scene Switcher to skip loading scene;
-	Add prefab for default Loading Bar UI;
-	Add blank non-rounded UI background image.

---

## 0.0.6

-	Create Event Manager and Event Invoker to allow for non-referenced communication.

---

## 0.0.5

-	Separate Grid and List structures.

---

## 0.0.4

-	Inroduce generic Solver prototype and corresponding editor;
-	Solver prototype improvements (accessibility of properties and events, expand and auto-invoke events, expand solution states, introduce prng);
-	Addembler Definitions rename.

---

## 0.0.3

-	Introduce Grid and Grid Element Constraint prototypes;
-	Separate vector and array handlers to ObjectHelpers_Geometry.

---

## 0.0.2

-	Add basic Unity Object extensions in ObjectHelpers_UnityObjects: 
	-	TryFind a component;
	-	Actiavate/Deactivate children;
	-	TryFind GameObject in gameObject;
	-	Destroy children;
-	Add UI helper in ObjectHelpers_UI: Set Text to automatically detect which Text Components are used;
-	Add generic C# heler in ObjectHelpers_PrimitiveTypes: safe GetItem for lists;
-	Add Debug Logger to intercept Debug Logs and put into a UI;
-	Add TManager prototype for singleton classes.

---

## 0.0.1

2023-02-08

-	Initialize package structure (folders, assembly definitions, package descriptor, changelog, and documentation) and git (license and readme).
