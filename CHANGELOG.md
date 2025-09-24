# Change Log

---

## 0.0.71

2025-09-24

-	Dummy utility auto-detect and disable other audiolisteners;

---

## 0.0.70

2025-09-15

-	Allow to clear event manager subscriptions;
-	Reset dual drag event on finger count change;

---

## 0.0.69

2025-09-05

-	Add a primitive tool to visualize pointer and up to 10 touches;
-	Slightly more descriptive variable name in previous commit;

---

## 0.0.68

2025-09-02

-	Extend the dual touch
	-	to allow for exclusive single and dual touch events;
	-	to allow variable raycast planes;

---

## 0.0.67

2025-08-21

-	Create generic Rolling Average structure;
-	Allow the world touch to have dynamic touch plane and expose private camera property for overrides;
-	Add mini tool to show cursor in editor;
	
---

## 0.0.66

2025-07-21

-	Allow to queue events in event manager;

---

## 0.0.65

2025-05-27

-	Expand spatial range structure:
	-	serialize;
	-	add encapsulation;
	-	add some static defaults;
	-	convert to bounds;
-	Expand NavMesh structure:
	-	try to generalize navmesh controller to allow for multiple walkable build sources;
	-	start to abstract away the structure to allow for inherited alterations;
	
---

## 0.0.64

2025-05-02

-	Expand new input system:
	-	Add dual touch drag handler passing the 3D touch locations with overridable plane handler (default horizontal 0 plane);
	-	Add auto enabling of enhanced touch in the manager;

---

## 0.0.63

2025-04-06

-	Expand New Input:
	-	add all applicable layer
	-	add world pinch detector
	-	expand world drag to have exposed start-end events;

---

## 0.0.62

2025-03-28

-	Safeguard AI navigation with conditional comiplation;
-	Fix curve offsetiing to handle curves better;
-	Expose Camera controller ZoomDistanceRanges to be able to override in derived classes;
-	Add default dummy network certificate handler;
	
---

## 0.0.61

2025-03-20

-	Rework triangulation to be based on the ear-clipping algorythm modified to work with holes, extend options;
-	Add ability to rotate a list;
-	Add name accessor for mesh objects;
-	Start curve offset utility;
-	Add world drag new input handler;
-	Adjust new input manager;

---

## 0.0.60

2025-03-12

-	Rework Mesh Curve tools to be able to split mesh at edges and simplify workflow;
-	Initiate new input system;
	-	set up centralized input action monitoring Manager;
	-	set up handler and receiver interfaces;
	-	set up layers to handle event priority automatically;
	-   start adapting events to new structure for pointer and mouse;
-	Add interface serialization with editor tools;
-	Try to adapt navmesh agent to be able to find closest point and add target visualizer;
-	Add Double Cross Vector operator and Vector3Double to string utility;
-	Add utility to find component in closest relative upstream in parents;
-	Rework logger to work with fixed buffer and reduce string operations;
-	Add Layer Attribute to preview single layer dropdowns in editor;
-	Small fixes with unused variables, nullables, static variable reset and obsolete methods;

---

## 0.0.59

2025-02-21

-	Adapt mesh manager to be able to return multiple meshes;

---

## 0.0.58

2025-02-14

-	Try to adjust Web build tools to Unity 6;
-	Temporarily disable webcam tool for web;

---

## 0.0.57

2025-02-13

-	Introduce Binary tree and Oct tree data structures;
-	Introduce Frame rate setter;
-	Adjust web cam accessor closing logic;
-	Add periminary draft of a tool for Web Dual Builds;
-	Clean up assembly definitions and web related items;

---

## 0.0.56

2024-12-09

-	Small adjustments to solver and grid structures;

---

## 0.0.55

2024-11-04

-	Create no GC versions for some vector conversions;

---

## 0.0.54

2024-10-14

-	Allow addressable Sprites to be accessed directly via callback;

---

## 0.0.53

2024-09-30

-	Expand Mesh joining to allow submesh isolation;

---

## 0.0.52

2024-09-18

-	Reorganize Mesh tools;
-	Add structure to do instanced mesh rendering;
-	Add trigger detector utility;
-	Organize camera input provider;

---

## 0.0.51

2024-09-09

-	Expand Runtime settings configurator.

---

## 0.0.50

2024-09-02

-	Fix get component to include not just monobehaviours but any component;
-	Set up avatar animation and navigation system:
	-	dynamically generate navmesh;
	-	set up avatar specific animation blend tree with audio sources for footsteps and actions and animationstate for actions;
	-	generalize animation settings into objects for convinience;

---

## 0.0.47

2024-08-05

-	Tweak input system:
	-	Cache raycasters and pointer over UI results per frame;
	-	Allow Vertical camera range for third person camera controllers to vary based on proximity to target;
	-	Add two finger gesture in touch third person camera controller to tilt camera.

---

## 0.0.46

2024-07-18

-	Tweak Input system:
	-	Allow only one Interaction event per update in receivers (end to start);
	-	Autosstart interaction if start is triggered;
	-	Handle more edgecases without handlers;
	-	Disable exclusiveness of camera controllers;
	-	Tweak touch camera controller's start condition;
	-	Fix touch camera controller's input event not being unsubscribed;

---

## 0.0.45

2024-07-18

-	Camera controllers:
	-	Split camera controller and input receiver;
	-	Add touch exclusive input receiver for camera controller;
	-	Try to minimize garbage generation by camera controllers;
-	Input System:
	-	Debug event racing with input controllers;
	-	Allow for input receivers to not have handlers;
	-	Minimize enabling/disabling actions byt input receivers;

---

## 0.0.44

2024-07-03

-	Allow to automatically disable stcktracing or disable logs completely in the logger;
-	Allow mesh Objects to be destroyed and disable auto asignation of meshes;
-	Make Mesh manager abstract and generic;
-	Allow diabling of unloading resources on scene loading;
-	Reduce logs in Scene management Extender and UI MAnager;
-	Safeguard Camera controller and UI manager for rare issues;

---

## 0.0.43

2024-06-28

-	small corrections to Solver base class;
-	add mesh object composite object;
-	add mesh manager to await mesh generation asynchronously;

---

## 0.0.42

2024-06-24

-	Allow spherical meshest to be reoriented;
-	Allow Spherical coordinates to be reoriented;
-	Try limit camea movement monitor;
-	Add basic tree data structure;
-	Add mini-helper function to set target framerate in app;

---

## 0.0.41

2024-06-20

-	Add Addressable manager and tools;
-	Clean assembly definitions;

---

## 0.0.40

2024-06-09

-	Try to adjust extended touch sensitivity;
-	Add timeout for single touch gestures in case second is a bit delayed;
-	Add ability to unregister input receivers and autounregister OnDestroy;

---

## 0.0.39

2024-05-21

-	Add curve extrusion extension for MeshTools;
	-	Allow curve capping, handle simple convex case and more generic case with inner curves (automatically handling winding, uvs, normals);
	-	Add a way to evaluate curve winding;
-	Add a way to merge and resize MeshData;
-	Automatically compute MeshData center;

---

## 0.0.38

2024-04-05

-	Allow UI state to request a  target state;
-	Allow controller to pass more actions and add a request state mechanism;
-	Update UI Manager to handle multiple actions by controller and add a request state mechanism;

---

## 0.0.37

2024-04-05

-	Add an event subscriber with exposed callback;
-	Add simple utility to sync transform with another transform;

---

## 0.0.36

2024-03-19

-	Organize UI Management:
	-	Switch UI states to be addressed by IDs (for now strings);
	-	Allow UIStates to Toggle;
	-	Expose Next or Previous UI state to not disable enable common canvases;
	-	Expose Event on UI state change;
	-	Refactor the manager into regions;
	-	Handle edge cases in controller;
	-	Allow non-linear controller flow and manual initial activativation;
-	Allow Object toggle to auto-set state on start;
-	Add helper to wait for animation state to be reached;
-	WebCam fixes:
	-	Add web cam state callback;
	-	Add web cam state accessors;
	-	Fix state not being set correctly in UI controlled texture receiver;
-	Add Animations and Prefab for smooth canvas Animator;

---

## 0.0.35

2024-03-12

-	Allow option generator to have overridable option creation;
-	Simplify and generalize WebCam controller;
-	Allow web camera texture receiver to autostart if was set up previously;
-	Split web camera texture receiver raw image logic from the controller;
-	Add protection to copy component to copy fields and properties by options;
-	Allow scene loader to work without an extender;
-	Auto set default loading scene by default in scene manager extender;
-	Add exposed proximity percentage in third person camera controller;
-	Allow move speed to be varied based on proximity for the third person camera controller;

---

## 0.0.34

2024-03-05

-	In webcamera UI if no dropdown is set - trigger start stream on camera connection;
-	Add a generic UI Manager, Controller and State system;
-	Add an animator utility to trigger a change of state;
-	Add UI utility to simplify working with a choice options;

---

## 0.0.33-2

2024-03-01

-	Manually copy rigidbody in camera controller as automatic one fails to copy some properties;
-	Make Collider optional for camera controllers;

---

## 0.0.33-1

2024-03-01

-	Fix UI to world interactor;
-	Allow interactors to request end of interaction;

---

## 0.0.33

2024-03-01

-	Smooth out camera controllers by introducing proxies and smooth damping to them;
-	Small Input fixes;
-	Split interaction and placement rays;
-	Set a generic way for Handlers to tell receiver if the interaction is relevant to cancel it if non are relevant;
-	Assign default input action to move and rotate input receivers;
-	Create a Copy Compontent tool in Unity Object Helpers;

---

## 0.0.32

2024-02-29

-	Massive refactoring of input controls:
	-	Clear distinction between input receiver and handler;
	-	Clean and refactor all input receivers and handlers;
	-	Introduce 3D object Manipuation and Rotation input receivers and handlers;
	-	Don't allow placement from UI if over UI;
	-	Introduce Layermasks for input receivers;
-	Transfer TMP LinkOpener;


---

## 0.0.31

2024-02-25

-	Introduce Standard Deviation evalutation for float collections and add normalization based on it;
-	Expand accessors of the CSV data;
-	Expand WebCamera accessor functionality and usability and expand default interface;
-	Handle WebCamera texture resolution not being set immediately;

---

## 0.0.30

2024-02-20

-	Add WebCamera Accessor, abstract TextureReceiver and RawTexture visualizer;
-	Add a simple pop up control for UI to display messages upon callback;
-	Add a base scene loader to allow for more customizable scene loading;

---

## 0.0.29-2

2024-02-09

-	Try fix third person camera controller rotation strangeness;
-	Fix touch events inversion in extended touch.

---

## 0.0.29-1

2024-02-05

-	Add Vector to double array conversion;
-	Fix float minmax calculations;
-	Add double minmax object implementation.

---

## 0.0.29

2024-01-31

-	Add vector translation to arrays and back;
-	Add MinMax array abstract class and default float implementation;
-	Add an abstract OptionsGenerator - script generating GameObjects for a given collection;

---

## 0.0.28

2024-01-30

-	Expand Scene Loading Manager:
	-	allow for additive scene in the process;
	-	add transitioning check;
	-	add on after scene activated callbacks;
	-	add support to '*' scene callback subscription for any scene;
	-	add object transitioning support between scenes;
	-	add overridable SceneReady check;
	-	split new scene loading and callbacks;
-	Make Scene Loader protected to be able to override it;
-	Make Default loading scene canvas last in order to always be on top.

---

## 0.0.27.3

2024-01-05

-	Create Vector3Double struct;
-	Switch all geohelpers to Vector3Double;

---

## 0.0.27.2

2024-01-05

-	Fix moving forward backward flip for the third person controller;
-	Account for center offset for Spherical mesh generation and geo Converters;

---

## 0.0.27.1

2024-01-05

-	Try fix third person camera controller orientation calculations for a general case.

---

## 0.0.27

2024-01-05

-	Fix spherical mesh generation orientation inversion;
-	Add option to flip faces of generated meshes;
-	Add a default Loading Scene;
-	Fix missing default action for object interactor input receiver;
-	Orient Camera controllers to up direction and expose overriden Unity Functions;

---

## 0.0.26

2024-01-03

-	Create Mesh Data struct to transition generation to separate threads and allow to split mesh preparation and uploading to GPU;
-	Split Regular Grid Mesh generation and add versions working with MeshData;
-	Create Spherical Grid Mesh generation and add Height Setting relative to orientation from 0;
-	Add optional matrix modifier for height setting;
-	Rewrite geoconverters to reference result holder to minimize temporary variable generation;
-	Add spherical coordinate converters;
-	Unify Geo converters to work with Vector3s and Vector3Ints for indicies;
-	Allow camera controllers to override Up direction.

---

## 0.0.25

2023-12-15

-	Set up Input receiver and handler for 3d object interactions;
-	Fix exclusive state setting and resetting with multiple possible interactors;
-	Implement own Pointer over UI check to mittigate weird behaviours;
-	Simplify UI to World interactors and make it cimilar to 3d object interactor;
-	Add default Unity Events for input handlers exposed in editor;
-	Add helper function to check if GameObject's layer is in layermask;

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
-	Redo Geography converters: 	
	-	Switch to fractional tile indices encoding UVs directly;
	-	Simplify and minimize cross calls;
	-	Improve precision;

---

## 0.0.18

2023-11-10

-	Fix Third Person Camera Controller to not allow rig to zoom out pushing the target;
-	Extend Scene Manager Extender:
	-	Add OnSceneLoaded and OnSceneUnloaded events;
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
