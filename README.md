# Hermit Crab Tech Test

This repository contains the source code for a technical test for Hermit Crab, which followed these rules:

- You have 1 (ONE) week to complete the test;
- You must use the attached assets in the game project, but feel free to complement them;

## Objective:
Develop a mobile game with at least the following requirements:

### Main Menu:
A scene with at least a Play Button;

### Game Play Scene:
Must include:
- Obstacles;
- A win/lose condition;
- A playable character;
- Must be playable in both Editor and mobile;
- A HUD displaying the player's current score;

### End Game:

Must include:
- Display of the player's score;
- Retry / Play again option;
- Back to the menu option;

### Delivery:
- A repository link to the source code with public access;
- An Android APK;
- Project documentation;

## Overview
The project is a 2D game developed in Unity, with a modular architecture that facilitates maintenance and scalability. The code is organized into several namespaces, each responsible for specific aspects of the game:

- HermitCrab.Camera: Manages the behavior and configuration of the camera using Cinemachine.
- HermitCrab.Character: Contains the logic of the characters (player and enemies), including movement, animation, attacks, collisions and reactions to damage.
- HermitCrab.Core: Groups the core logic of the game, such as level management, scoring, lives and a simple Behavior Tree for the AI.
- HermitCrab.Level: Responsible for the procedural generation of levels, definition and behavior of obstacles (barrels, saws, spikes) and collectible items, in addition to the spawn and end points of the level.
- HermitCrab.UI: Implements the user interface, with menu screens, in-game HUD, pop-ups and end-of-game screens, as well as handling input events for both desktop and mobile devices.

## Architectures Used

- **Modular Architecture:**
  The project is divided into clearly separated namespaces (e.g., Camera, Character, Core, Level, UI), each responsible for a specific domain. This separation of concerns improves maintainability and scalability.
- **Component-Based Architecture:**
  The code leverages Unity's component model with MonoBehaviours for behaviors such as character movement, animation, and level generation, keeping functionality encapsulated in distinct components.
- **Data-Driven Architecture:**
  Extensive use of ScriptableObjects enables a data-driven approach where configuration data (e.g., character settings, game configurations, level parameters) is separated from the code, allowing designers to tweak values without modifying the logic.

## Design patterns Used

- **Observer Pattern:**
  Events and delegates are used throughout the project (for example, health changes, level events, and UI updates) to decouple components and allow for reactive programming.
- **Factory Pattern:**
  The GameObjectFactory is used to instantiate and initialize game objects (such as projectiles and environmental objects), centralizing object creation.
- **Facade Pattern:**
  Classes like CharacterBehaviour act as a facade by aggregating various functionalities (movement, attack, animation, etc.) into a single interface that the rest of the system interacts with.
- **Behavior Tree Pattern:**
  A simple behavior tree is implemented for enemy AI, which defines actions like hurt, chase, and patrol in a structured and extensible way.
- **(Implicit) Dependency Injection:**
  While not using a formal DI container, the project assigns component references via the Inspector and constructor injection (in non-MonoBehaviour classes), reducing coupling between systems.

**Unity Best Practices:**

- **Separation of Concerns:**
  Each system (UI, character control, level generation, etc.) is implemented in its own module or namespace, which minimizes interdependencies.
- **Use of ScriptableObjects:**
  ScriptableObjects are employed for configuration data, which enhances flexibility and allows for adjustments without modifying the code.
- **Proper Event Management:**
  The code subscribes and unsubscribes from events (typically in Awake/OnDestroy) to prevent memory leaks and unintended behavior.
- **Effective Use of Coroutines:**
  Coroutines manage asynchronous tasks such as level generation and visual effects without blocking the main thread.
- **Debugging and Visualization:**
  The use of Gizmos (e.g., in OnDrawGizmosSelected) assists in debugging and visualizing important parameters like detection ranges directly in the Unity Editor.
- **Optimized Physics and Input Handling:**
  Utilizing layer masks for physics checks and providing flexible input methods (keyboard and mobile UI) to accommodate different platforms are important practices.
- **Consistent Naming and Code Organization:**
  The project follows clear naming conventions and structured code organization, which enhances readability and ease of maintenance.

## Major Difficulties

- **Procedural Generation Was Challenging:**
  Procedural generation required careful balancing between randomness and design intent. It was challenging to ensure that the generated levels maintained a coherent layout, provided a fair difficulty curve, and remained fun to play while handling various obstacles, gaps, enemies, and collectibles dynamically. Additionally, optimizing performance during runtime and ensuring that all elements aligned correctly on a grid demanded meticulous planning and iterative testing.

- **Enemy AI Was Challenging:**
  Developing the enemy AI was labor-intensive because it needed to be responsive and adaptive to the player's actions. Implementing a behavior tree that manages multiple states—such as patrolling, chasing, and reacting to damage—involved complex state management and condition handling. Ensuring that the AI transitions smoothly between behaviors while interacting correctly with the game's physics and environment further increased the complexity of the task.

## Suggestions for Improvement:

- **Enhance Procedural Generation:**
  Consider integrating more advanced algorithms (e.g., wave function collapse or modular design patterns) to generate levels with greater variety and coherence. Implement additional constraints or adaptive difficulty mechanisms that adjust to player performance.
- **Refine Enemy AI:**
  Introduce more sophisticated decision-making frameworks, such as utility-based systems or improved pathfinding algorithms (like A*) to make enemy movements more realistic and challenging.
- **Optimization and Scalability:**
  Optimize performance by leveraging object pooling for frequently instantiated objects, and consider using asynchronous processing to smooth out generation and AI computations. Additionally, I could have used a custom library I developed called [LegendaryTools.Pool](https://github.com/LeGustaVinho/pool "LegendaryTools.Pool"), which efficiently implements the pooling design pattern. This library is designed to manage and recycle objects effectively, reducing memory overhead and minimizing the cost of runtime instantiation, thereby further enhancing overall performance and scalability.
- **Modularization and Testing:**
  Further decouple components to facilitate unit testing and debugging. Implement more robust logging and debugging tools to monitor AI behaviors and procedural generation outcomes during runtime.

## Components and Features

### CameraControllerCinemachine
**Responsibility:** Configures and controls the virtual camera (Cinemachine) to track the player or another target.

**Features:**
- Assigns the target to be tracked.
- Adjusts framing offsets for camera positioning.
- Optional configuration of a confiner to limit the camera's movement area.

### CharacterBehaviour

**Responsibility:** Acts as the main facade of the character. It is a MonoBehaviour that integrates the various components (movement, attack, animation, damage visual effects) and manages events such as death, damage and item collection. **Flow:** Initializes components (MovimentCharComponent, AttackCharComponent, AnimationCharComponent), subscribes to events and coordinates the character's actions.

### CharacterLogic

**Responsibility:** Implements the character's core logic independently from Unity, managing:
- Health and energy.
- Actions such as jumping, attacking and cooldown control.
- Damage events and death verification.

### MovimentCharComponent

**Responsibility:** Manages the character's physical movement:
- Horizontal movement (walking and running).
- Processing jumps and landings.
- Adjusting the sprite's direction.
- Checking if the character is on the ground (ground check).

### AnimationCharComponent

**Responsibility:** Interacts with the animation system (Animator) to fire triggers and switch between states (e.g.: jump, punch, shoot).

### AttackCharComponent

**Responsibility:** Manages the character's attack actions (shoot and punch), checking cooldowns, instantiating projectiles and calculating areas of effect.

### DamageFxCharComponent

Responsibility: Provides visual feedback to the character when taking damage (e.g.: red flashes or color change for poisoning).

### InputController

**Responsibility:** Maps user input (keyboard or UI buttons) to the character's action methods (movement, jump, shot, punch).

### EnemyAIController

**Responsibility:** Implements enemy AI using a Behavior Tree to manage states such as:
- Hurt: Reacts when the enemy takes damage.
- Chase: Chases and attacks the player when detected.
- Patrol: Performs patrols when the player is not close.

### CharacterData (ScriptableObject)

**Responsibility:** Stores configurable character parameters, such as:
Health and energy.
- Movement speeds.
- Jump strength.
- Cooldowns and energy costs.
- Animation parameters (strings and hashes).

### ProjectileController & ProjectileInfo

**Responsibility:**
- ProjectileController: Manages the movement, collision and destruction of projectiles fired by the character. - ProjectileInfo: Encapsulates projectile initialization data, such as speed, damage, direction, and attacker position.

### GameConfig (ScriptableObject)

**Responsibility:** Centralizes general game settings, such as:
- Prefabs (player, enemies, etc.).
- Number of initial lives.
- Number of levels to beat.
- Settings for points to collect and eliminate enemies.

### GameLogic

**Responsibility:** Implements the game's progress logic, managing:
- Level changes.
- Score and lives updates.
- Events such as level start and end, game over, and victory.

### GameController

**Responsibility:** Acts as the game's main coordinator:
- Manages level generation (through the ProceduralLevelGeneratorRuntime).
- Instantiates and configures the player. - Turns on the camera and input controls.
- Propagates events related to level completion, player death, and score.

### GameObjectFactory

**Responsibility:** Utility class that centralizes the creation of GameObjects, such as projectiles and environmental objects, ensuring proper initialization.

### Behavior Tree Implementation
The Behavior Tree for enemy AI is implemented with the following classes:

- **BTNode:** Abstract base class for tree nodes.
- **BTAction & BTCondition:** Nodes that execute actions or evaluate conditions, returning a status (Success, Failure or Running).
- **StatelessBTSequence & StatelessBTSelector:** Composite nodes that manage the sequential or selective execution of child nodes.

### ProceduralLevelGeneratorRuntime

Responsibility: Generates levels procedurally, using configurable parameters for:
Creating platforms (floors), gaps and filling areas.
Inserting obstacles (spikes, explosive/poisonous barrels, saws) and collectible items.
Defining the spawn (start) and victory (end of the level) points.
Creating background elements and vertical walls to delimit the level.
Obstacles and Items:

### Obstacles and Items:
- **Saw & SawData:** Implements the behavior of a mobile saw that causes continuous damage and knockback.
- **Spike & SpikeData**: Represents fixed spikes that cause damage and knockback on contact.
- **CollectibleItem & CollectibleItemData:** Manages items that restore energy or health when collected.
- **DoorTrigger:** Detects the player entering a “portal” that ends the level.
- **Barrel, BarrelLogic & BarrelData:**
    - **Barrel:** Component that reacts to damage and, depending on the type (Explosive or Poisonous), performs the explosion (causing damage, knockback) or applies a poison effect.
    - **BarrelLogic:** Independent logic to manage barrel activation.
    - **BarrelData:** ScriptableObject with settings (damage, explosion radius, poison parameters).

### MainScreenView

**Responsibility:** Displays the main screen (start menu) and manages the button to start the level.

### LevelScreenView

**Responsibility:** Displays the HUD during the game, including:
- Health and energy bars.
- Life and score indicators.
- Control buttons for touch input (movement, jump, shooting, punching).

### PopupView

**Responsibility:** Displays pop-ups with feedback to the player and a close button.

### EndScreenView

**Responsibility:** Displays the end screen of the game, differentiating between victory and defeat, and showing the final score.

### UIController

**Responsibility:** Acts as the interface coordinator, connecting the views to the GameController and InputController events. Manages:
- Switching between screens (main menu, HUD, pop-up, end of game).
- Propagating input events to the game systems.

### ContinuousPointerEventUpdate

**Responsibility:** Triggers repeated events while the user holds down a button, ideal for mobile controls (e.g.: continuous movement).

## General Considerations

- **Modularity and Configuration:** The extensive use of ScriptableObjects (such as CharacterData, GameConfig, BarrelData, etc.) allows you to adjust game parameters without having to change the code, facilitating testing and balancing.

- **Integration between Systems:** The GameController acts as a central hub, coordinating level generation, instantiating the player, configuring the camera, and propagating events between subsystems (characters, AI, UI).

- **Behavior Tree for AI:**
  The Behavior Tree implementation, while simple, organizes enemy AI in a clear and extensible way, prioritizing behaviors such as reacting to damage, chasing the player, and patrolling.

- **Procedural Generation:**
  The level generator uses a grid-based approach, with variations controlled by configurable probabilities to insert gaps, obstacles, enemies, and items, enabling dynamic and varied levels in each match.

- **Adaptive Interface:**
  The UI components were developed to work on both desktop and mobile platforms, with specific controls for touch input and consistent visual feedback (health bars, score, etc.).

### Behavior Tree Implementation
The Behavior Tree for enemy AI is implemented with the following classes:

- **BTNode:** Abstract base class for tree nodes.
- **BTAction & BTCondition:** Nodes that execute actions or evaluate conditions, returning a status (Success, Failure or Running).
- **StatelessBTSequence & StatelessBTSelector:** Composite nodes that manage the sequential or selective execution of child nodes.

### ProceduralLevelGeneratorRuntime

Responsibility: Generates levels procedurally, using configurable parameters for:
Creating platforms (floors), gaps and filling areas.
Inserting obstacles (spikes, explosive/poisonous barrels, saws) and collectible items.
Defining the spawn (start) and victory (end of the level) points.
Creating background elements and vertical walls to delimit the level.
Obstacles and Items:

### Obstacles and Items:
-** Saw & SawData:** Implements the behavior of a mobile saw that causes continuous damage and knockback.
- **Spike & SpikeData**: Represents fixed spikes that cause damage and knockback on contact.
- **CollectibleItem & CollectibleItemData:** Manages items that restore energy or health when collected.
- **DoorTrigger:** Detects the player entering a “portal” that ends the level.
- ** Barrel, BarrelLogic & BarrelData:**
- **Barrel:** Component that reacts to damage and, depending on the type (Explosive or Poisonous), performs the explosion (causing damage, knockback) or applies a poison effect.
  -** BarrelLogic:** Independent logic to manage barrel activation.
- **BarrelData:** ScriptableObject with settings (damage, explosion radius, poison parameters).

### MainScreenView

**Responsibility:** Displays the main screen (start menu) and manages the button to start the level.

### LevelScreenView

**Responsibility:** Displays the HUD during the game, including:
- Health and energy bars.
- Life and score indicators.
- Control buttons for touch input (movement, jump, shooting, punching).

### PopupView

**Responsibility:** Displays pop-ups with feedback to the player and a close button.

### EndScreenView

**Responsibility:** Displays the end screen of the game, differentiating between victory and defeat, and showing the final score.

### UIController

**Responsibility:** Acts as the interface coordinator, connecting the views to the GameController and InputController events. Manages:
- Switching between screens (main menu, HUD, pop-up, end of game).
- Propagating input events to the game systems.

### ContinuousPointerEventUpdate

**Responsibility:** Triggers repeated events while the user holds down a button, ideal for mobile controls (e.g.: continuous movement).

## General Considerations

- **Modularity and Configuration:** The extensive use of ScriptableObjects (such as CharacterData, GameConfig, BarrelData, etc.) allows you to adjust game parameters without having to change the code, facilitating testing and balancing.

- **Integration between Systems:** The GameController acts as a central hub, coordinating level generation, instantiating the player, configuring the camera, and propagating events between subsystems (characters, AI, UI).

- **Behavior Tree for AI:**
  The Behavior Tree implementation, while simple, organizes enemy AI in a clear and extensible way, prioritizing behaviors such as reacting to damage, chasing the player, and patrolling.

- **Procedural Generation:**
  The level generator uses a grid-based approach, with variations controlled by configurable probabilities to insert gaps, obstacles, enemies, and items, enabling dynamic and varied levels in each match.

- **Adaptive Interface:**
  The UI components were developed to work on both desktop and mobile platforms, with specific controls for touch input and consistent visual feedback (health bars, score, etc.).