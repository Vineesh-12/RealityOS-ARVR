# RealityOS

### *A Multi-Modal Spatial Computing Environment Integrating Desktop Interaction, Mobile Augmented Reality and Simulated Extended Reality*

---

## 1. Project Overview & Problem Statement

### 1.1. Overview
**RealityOS** is an academic and professional spatial computing environment developed in **Unity 6**. It liberates traditional digital interfaces from rigid two-dimensional rectangular windows, transforming notes, task boards, information feeds, and 3D objects into manipulable, persistent spatial objects.

RealityOS delivers a unified cross-modal architecture operating across three distinct interaction environments:
1. **Desktop Spatial Mode**: A 3D spatial interface navigated using standard mouse and keyboard controls on a modern laptop (e.g., Dell G15 with NVIDIA RTX 3050).
2. **Mobile Augmented Reality Mode**: Real-world camera-based augmented reality leveraging Unity AR Foundation and Google ARCore to detect physical surfaces, anchor floating digital panels, and manipulate them via multi-touch gestures.
3. **Simulated Extended Reality Mode**: An XR Interaction Toolkit environment demonstrating 6-DoF simulated controllers, laser-pointer ray selection, direct grabbing, comfort snap-turning, and teleportation without requiring physical VR headset hardware.

### 1.2. Problem Statement
Traditional operating system windowing paradigms were designed for flat physical displays, constraining multi-tasking to 2D overlaps. With the emergence of spatial computing (e.g., Apple Vision Pro, Meta Horizon, Qualcomm Spaces), human-computer interaction is shifting to 3D spatial volumes. However, developing spatial applications often suffers from vendor lock-in, heavy headset requirements, or fragmented architectures. RealityOS solves this by establishing a **unified, decoupled interaction and persistence architecture** that runs predictably on everyday desktop hardware, mobile devices, and simulated XR rigs alike.

---

## 2. Project Objectives
1. Build a robust 3D spatial computing environment using Unity 6 and C#.
2. Implement 5 distinct floating spatial panels with 3D manipulation (select, move, rotate, resize, duplicate, delete, focus).
3. Deliver high-precision desktop mouse & keyboard navigation (WASD, look orbit, zoom, shortcuts).
4. Implement mobile AR surface detection and touch manipulation using AR Foundation and Google ARCore.
5. Implement simulated XR interactions (raycasting, direct grabbing, comfort snap turning, teleportation).
6. Provide atomic JSON-based local persistence surviving application restarts.
7. Demonstrate core syllabus outcomes for **21CSE353T (Augmented, Virtual and Mixed Reality)**.

---

## 3. Technology Stack

- **Game Engine**: Unity 6 (`6000.6.4f1`)
- **Render Pipeline**: Universal Render Pipeline (URP 17.6.0) / Built-in Lit
- **Programming Language**: C# (.NET Standard 2.1)
- **Input System**: Unity Input System (v1.20.0)
- **UI & Typography**: TextMeshPro & Unity UI (com.unity.ugui v2.6.0)
- **Augmented Reality**: Unity AR Foundation (v6.0.4) & Google ARCore XR Plugin (v6.0.4)
- **Extended Reality**: XR Interaction Toolkit (v3.0.5) & OpenXR Plugin (v1.12.1)
- **Testing**: Python 3.13 Automated Test Runner & Unity NUnit Test Framework
- **Companion Suite**: Unreal Engine 5 C++ Architecture (`RealityOSPanel.h`, `RealityOSPanel.cpp`)

---

## 4. Main Features

### 4.1. Spatial Panels
- **Notes Panel**: Editable title, multiline rich text input, character count tracker, auto-persisting contents.
- **Task Board Panel**: Dynamic task creation, status toggles (`Pending` -> `In Progress` -> `Completed`), deletion, completion ratio summary.
- **Information Panel**: Descriptive spatial information, system metadata badges, and 3D positioning coordinates.
- **3D Object Viewer Panel**: Embedded 3D geometric preview model, orbit rotation, zoom adjustment, and auto-spin toggle.
- **Quick Access Panel**: User-configured navigation shortcuts for rapid scene/mode switching.

### 4.2. Spatial Manipulation Controls
- **Kinematic Smoothing**: Smooth interpolation (`Vector3.Lerp`, `Quaternion.Slerp`) using delta time.
- **Visual Feedback**: Real-time hover tinting and selection glow borders (`#00D2C4` cyan accent).
- **Preset Arrangements**: One-click arrangement of all active panels into a **3D Curved Arc** or a **Horizontal Grid**.

### 4.3. Local Persistence Engine
- Workspaces saved atomically to `Application.persistentDataPath/RealityOS/Workspaces/{workspaceId}.json`.
- Automatic schema validation, boundary clamping, and quarantine recovery for corrupted files.
- Auto-save timer triggers periodically whenever workspace modifications occur.

---

## 5. Project Directory Structure

```
ARVR/
├── Assets/
│   └── RealityOS/
│       ├── Audio/
│       ├── Materials/
│       ├── Models/
│       ├── Prefabs/
│       │   ├── AR/
│       │   ├── Objects/
│       │   ├── Panels/
│       │   ├── UI/
│       │   └── XR/
│       ├── Resources/
│       ├── Scenes/
│       │   ├── Bootstrap.unity
│       │   ├── MainMenu.unity
│       │   ├── DesktopWorkspace.unity
│       │   ├── ARWorkspace.unity
│       │   ├── XRSimulator.unity
│       │   └── DemoWorkspace.unity
│       ├── Scripts/
│       │   ├── AR/
│       │   │   ├── ARPlacementReticle.cs
│       │   │   ├── ARTouchGestureHandler.cs
│       │   │   ├── ARUIOverlay.cs
│       │   │   └── ARWorkspaceController.cs
│       │   ├── Core/
│       │   │   ├── AppEnums.cs
│       │   │   ├── ApplicationBootstrap.cs
│       │   │   ├── AppModeManager.cs
│       │   │   ├── IInputProvider.cs
│       │   │   ├── IManipulationHandler.cs
│       │   │   ├── ISpatialInteractable.cs
│       │   │   ├── IWorkspacePanel.cs
│       │   │   ├── IWorkspacePersistence.cs
│       │   │   ├── SceneNavigationManager.cs
│       │   │   └── SettingsManager.cs
│       │   ├── Desktop/
│       │   │   ├── DesktopCameraController.cs
│       │   │   ├── DesktopInteractionRaycaster.cs
│       │   │   └── DesktopUIOverlay.cs
│       │   ├── Panels/
│       │   │   ├── InformationPanel.cs
│       │   │   ├── NotesPanel.cs
│       │   │   ├── ObjectViewerPanel.cs
│       │   │   ├── PanelControlBar.cs
│       │   │   ├── QuickAccessPanel.cs
│       │   │   ├── TaskBoardPanel.cs
│       │   │   └── WorkspacePanelBase.cs
│       │   ├── Persistence/
│       │   │   ├── DataModels.cs
│       │   │   └── PersistenceService.cs
│       │   ├── Tests/
│       │   │   └── RealityOSTests.cs
│       │   ├── UI/
│       │   │   └── MainMenuUI.cs
│       │   ├── Workspace/
│       │   │   ├── SpatialReferenceGrid.cs
│       │   │   └── WorkspaceManager.cs
│       │   └── XR/
│       │       ├── XRLocomotionProvider.cs
│       │       ├── XRSimulatedController.cs
│       │       └── XRSimulatorController.cs
│       ├── Settings/
│       └── Textures/
├── Documentation/
│   ├── Architecture.md
│   ├── Installation.md
│   ├── SyllabusMapping.md
│   ├── TechnicalLimitations.md
│   ├── Testing.md
│   └── UserGuide.md
├── Packages/
│   └── manifest.json
├── ProjectSettings/
│   ├── EditorBuildSettings.asset
│   ├── ProjectSettings.asset
│   └── ProjectVersion.txt
├── Tests/
│   └── test_runner.py
├── UnrealCompanion/
│   ├── README.md
│   ├── Documentation/
│   │   └── UnrealSetupGuide.md
│   └── Source/
│       ├── RealityOSPanel.cpp
│       └── RealityOSPanel.h
└── README.md
```

---

## 6. How to Open and Run RealityOS

### 6.1. Opening in Unity
1. Open **Unity Hub**.
2. Click **Add > Add project from disk** and select `C:\Users\vinee\Downloads\ARVR`.
3. Open with Unity **6000.6.4f1**.
4. Open scene `Assets/RealityOS/Scenes/Bootstrap.unity` and click **Play**.

### 6.2. Operating Desktop Spatial Mode
- **Move**: `W`, `A`, `S`, `D` (+ `Left Shift` to sprint).
- **Look**: Hold `Right Mouse Button` and drag.
- **Zoom**: `Mouse Scroll Wheel`.
- **Select Panel**: Left-click on any floating panel.
- **Drag Panel**: Click and hold panel header or body, then move mouse.
- **Focus Camera**: Press `F` to focus on selected panel.
- **Reset Rotation**: Press `R` to re-align panel to face camera.
- **Duplicate Panel**: Press `Ctrl + D`.
- **Save Workspace**: Press `Ctrl + S`.
- **Delete Panel**: Press `Delete`.

### 6.3. Operating Mobile AR Mode
- Point camera at physical floors or tabletops to scan.
- The **Placement Reticle** aligns to detected surfaces.
- Tap **Place Panel** to spawn anchored panels.
- Touch gestures: Single-finger drag (reposition), two-finger pinch (scale), two-finger twist (rotate), long press (context actions).

### 6.4. Operating Simulated XR Mode
- Aim with mouse; laser pointer shows surface intersection.
- Left Mouse Button (`Trigger`): Selects UI buttons and panels.
- Press `G` (`Grip`): Grabs and carries the hovered panel.
- Release `G`: Drops panel at new position.
- Press `Q` / `E`: Comfort snap turn 45° left/right.
- Hold `T`: Projects teleport arc; release to teleport.

---

## 7. Running the Automated Tests

Run the regression test suite from powershell:
```powershell
python Tests\test_runner.py
```
**Results**: 7 / 7 tests passed (Serialization, Deserialization, Clamping/Validation, CRUD, Clones, Corrupted Recovery, Multi-Workspace discovery).

---

## 8. Feature Mapping to Unity Scenes and Scripts

| Feature | Unity Scene | Key C# Scripts |
| :--- | :--- | :--- |
| **Bootstrap & Initialization** | `Bootstrap.unity` | `ApplicationBootstrap.cs`, `AppModeManager.cs`, `SceneNavigationManager.cs` |
| **Main Menu & Workspace Manager**| `MainMenu.unity` | `MainMenuUI.cs`, `WorkspaceManager.cs`, `PersistenceService.cs`, `SettingsManager.cs` |
| **Desktop Spatial Environment** | `DesktopWorkspace.unity` | `DesktopCameraController.cs`, `DesktopInteractionRaycaster.cs`, `DesktopUIOverlay.cs`, `SpatialReferenceGrid.cs` |
| **Mobile AR Surface Detection** | `ARWorkspace.unity` | `ARWorkspaceController.cs`, `ARPlacementReticle.cs`, `ARTouchGestureHandler.cs`, `ARUIOverlay.cs` |
| **Simulated XR & Locomotion** | `XRSimulator.unity` | `XRSimulatorController.cs`, `XRSimulatedController.cs`, `XRLocomotionProvider.cs` |
| **Showcase Demonstration** | `DemoWorkspace.unity` | `WorkspacePanelBase.cs`, `NotesPanel.cs`, `TaskBoardPanel.cs`, `InformationPanel.cs`, `ObjectViewerPanel.cs`, `QuickAccessPanel.cs` |

---

## 9. Academic Course Syllabus Mapping (21CSE353T)
See [Documentation/SyllabusMapping.md](Documentation/SyllabusMapping.md) for full mapping across Units I–V and Course Outcomes CO1–CO5.

