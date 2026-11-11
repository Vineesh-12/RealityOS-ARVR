# Course Syllabus Mapping

**Subject**: 21CSE353T – Augmented, Virtual and Mixed Reality  
**Degree**: Bachelor of Technology in Computer Science and Engineering  
**Project**: RealityOS – A Multi-Modal Spatial Computing Environment  

---

## 1. Syllabus Unit Mapping

| Syllabus Unit | Course Topics Covered | RealityOS Implementation & Evidence | Status |
| :--- | :--- | :--- | :---: |
| **Unit I: Introduction to Mixed Reality** | VR, AR, MR, and XR concepts; differences between interaction environments; Human-Computer Interaction (HCI); immersion and presence; spatial UI/UX; ergonomics; user comfort and safety considerations. | **Unified multi-modal architecture**: Demonstrates distinct paradigms for Desktop Spatial (monoscopic 3D), Mobile AR (video see-through physical augmentation), and Simulated XR (virtual immersion with simulated 6-DoF controllers). Incorporates comfort snap-turning (45°) and teleportation to mitigate simulator sickness. | **IMPLEMENTED AND TESTED** |
| **Unit II: AR Components and Techniques** | AR frameworks; real-world AR application development; plane detection; raycasting; surface tracking; object manipulation; 3D navigation and interaction. | **AR Foundation & ARCore integration** (`ARWorkspaceController`, `ARPlacementReticle`, `ARTouchGestureHandler`): Implements horizontal and vertical plane detection, AR raycast hit testing, AR anchors, and multi-touch gestures (single-finger drag, two-finger pinch scale, two-finger rotation). | **IMPLEMENTED BUT NOT DEVICE-TESTED** (code validated with simulated plane fallback) |
| **Unit III: Application using Unity** | Unity application development; AR Foundation; ARCore; XR-related SDK usage; interactive Unity scenes; 3D object and panel manipulation. | **Complete Unity 6 project**: 6 dedicated scenes (`Bootstrap`, `MainMenu`, `DesktopWorkspace`, `ARWorkspace`, `XRSimulator`, `DemoWorkspace`), shared interaction engine (`ISpatialInteractable`, `IManipulationHandler`), 5 functional panel types, and local JSON persistence. | **IMPLEMENTED AND TESTED** |
| **Unit IV: Unreal Engine and Techniques** | Unreal Engine environment; basic interaction; 3D spatial interface; rendering and optimization. | **Unreal Companion Demonstration** (`UnrealCompanion/`): Custom C++ panel actor (`RealityOSPanel.h`, `RealityOSPanel.cpp`) with world-space UMG widget support, kinematic interpolation (`VInterpTo`), and comprehensive setup guide for Unreal Engine 5. | **IMPLEMENTED AS ARCHITECTURAL COMPANION** |
| **Unit V: Modeling the Physical World** | Geometric modeling; kinematic modeling; physical modeling; locomotion; object interaction; spatial UI/UX; virtual environment design. | **Geometric & Kinematic Modeling**: Local/world coordinate transforms, Euler/quaternion rotations, uniform scaling bounds `[0.3, 5.0]`, smooth interpolation using delta time (`Vector3.Lerp`, `Quaternion.Slerp`). BoxColliders for spatial ray targeting; teleportation and snap turning locomotion. | **IMPLEMENTED AND TESTED** |

---

## 2. Course Outcomes (CO) Mapping

### CO1: Describe similarities and differences between MR technologies
- **Demonstration**: Through the interactive Main Menu and in-app documentation, RealityOS explicitly contrasts Desktop Spatial Computing (laptop mouse/keyboard), Augmented Reality (optical/video camera see-through overlaying digital panels onto physical geometry), and Extended Reality (fully enclosed virtual space with 6-DoF simulated controllers).
- **Status**: **Fully Satisfied**.

### CO2: Demonstrate AR/VR applications
- **Demonstration**: Provides runnable scenes for Desktop Spatial interaction, Mobile AR surface anchoring, and XR controller grabbing/teleportation.
- **Status**: **Fully Satisfied**.

### CO3: Build applications using Unity
- **Demonstration**: Engineered in Unity 6 (`6000.6.4f1`) with Universal Render Pipeline, Unity Input System, TextMeshPro, AR Foundation, ARCore, and XR Interaction Toolkit.
- **Status**: **Fully Satisfied**.

### CO4: Develop a use case using Unreal Engine
- **Demonstration**: Created the `UnrealCompanion/` suite featuring production-ready C++ classes (`RealityOSPanel`) demonstrating spatial panel kinematics, world-space widgets, and trace-based interaction in Unreal Engine 5.
- **Status**: **Satisfied via Companion Specification & Source Code**.

### CO5: Create UI for VR applications
- **Demonstration**: Designed and implemented 5 distinct floating spatial panels (Notes, Task Board, Information, 3D Object Viewer, Quick Access) featuring world-space UI controls, move/resize/rotate handles, and visual feedback without relying on traditional 2D window borders.
- **Status**: **Fully Satisfied**.

