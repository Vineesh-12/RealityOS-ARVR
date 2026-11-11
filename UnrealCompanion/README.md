# RealityOS – Unreal Engine Companion Demonstration

**Course Syllabus Mapping**: 21CSE353T – Augmented, Virtual and Mixed Reality  
**Course Outcome**: CO4 – Develop a use case using Unreal Engine  
**Unit**: Unit IV – Unreal Engine and Techniques  

---

## 1. Overview & Purpose
This Unreal Engine companion project demonstrates spatial computing concepts within Unreal Engine 5 to satisfy the syllabus requirements of Unit IV (Unreal Engine and Techniques) and Course Outcome CO4.

The companion implements:
- A modern 3D spatial workspace environment.
- Floating interface panels (`ARealityOSPanel`) with backing geometry, world-space UMG widget rendering, and interaction bounds.
- Ray-trace based selection, hover feedback, and kinematic smooth movement (`VInterpTo`, `RInterpTo`).
- Enhanced Input System integration for mouse/keyboard manipulation.
- Lumen dynamic global illumination and physical materials matching the RTX 3050 capability.

---

## 2. Directory Structure
```
UnrealCompanion/
├── README.md
├── Documentation/
│   └── UnrealSetupGuide.md
└── Source/
    ├── RealityOSPanel.h
    └── RealityOSPanel.cpp
```

---

## 3. Setup and Build Instructions in Unreal Engine 5

If you have Unreal Engine 5 (UE 5.3 or 5.4) installed on your system:

### Step 1: Create a Blank C++ Project
1. Open **Unreal Engine 5** via Epic Games Launcher.
2. Select **Games > Blank**.
3. Set project type to **C++**, target platform **Desktop**, quality **Maximum**.
4. Name the project `RealityOSCompanion`.

### Step 2: Add RealityOS C++ Classes
1. Copy `UnrealCompanion/Source/RealityOSPanel.h` and `RealityOSPanel.cpp` into your project's `Source/RealityOSCompanion/` directory.
2. Open the project in Visual Studio or Rider, or right-click `RealityOSCompanion.uproject` and select **Generate Visual Studio project files**.
3. Build the solution in `Development Editor` configuration.

### Step 3: Create Blueprint Derived Panel
1. In Unreal Editor, open the Content Browser and navigate to `Content/Blueprints/`.
2. Right-click > **Blueprint Class** > Search for `RealityOSPanel` as the Parent Class.
3. Name it `BP_RealityOSPanel`.
4. Assign a rounded rectangle static mesh to `BackingMeshComponent` with material `M_PanelGlass` (translucent slate with cyan edge glow).
5. In `SpatialWidgetComponent`, create and assign a UMG Widget `WBP_NotesPanel` displaying a title, multi-line text block, and control icons.

### Step 4: Configure Enhanced Input
1. Create Input Actions:
   - `IA_Select` (Digital Bool, mapped to Left Mouse Button)
   - `IA_Look` (Axis2D, mapped to Mouse XY)
   - `IA_Move` (Axis2D, mapped to WASD)
2. In the Character Blueprint, perform a line trace from `CameraComponent` on tick or click.
3. When `LineTraceSingleByChannel` hits an actor implementing `ARealityOSPanel`, call `OnHoverEnter()` or `OnSelect()`.

---

## 4. Current Environment Status & Validation
- **Hardware Environment**: Dell G15 Laptop (Intel Core / NVIDIA GeForce RTX 3050).
- **Installed Engines**: Unity 6 (`6000.6.4f1`) is installed and is the primary production engine. Epic Games Launcher is present, but the 40GB+ Unreal Engine 5 Editor binaries are not installed in the current workstation image.
- **Outcome Status**: **IMPLEMENTED AS ARCHITECTURAL & C++ COMPANION SPECIFICATION**. Complete C++ header/source and step-by-step setup documentation are provided for immediate build when UE5 is installed.

