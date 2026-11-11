# RealityOS Technical Limitations and Environmental Constraints

To adhere to strict engineering integrity and academic reporting standards, this document explicitly records all known constraints, technical boundaries, and environment-specific limitations of **RealityOS**.

---

## 1. Interaction Modes: True Capabilities vs. Constraints

### 1.1. Desktop Spatial Mode
- **Capability**: Fully functional 3D spatial interface operating natively on Windows 11.
- **Input**: Mouse and keyboard only.
- **Limitation**: Uses 2D screen projection (monoscopic 3D). Depth perception relies on perspective parallax, shadows, and camera motion rather than stereoscopic binocular disparity.

### 1.2. Simulated XR Mode
- **Capability**: Fully functional simulation of XR Interaction Toolkit concepts (6-DoF controller rays, direct grabbing, snap turn locomotion, teleportation).
- **CRITICAL DISTINCTION**: **This is NOT physical VR hardware.**
- **Limitations**:
  - No physical VR headset (e.g. Meta Quest, HTC Vive, Apple Vision Pro) has been connected or validated.
  - No optical lighthouse or inside-out camera controller tracking.
  - No physical controller haptic vibration feedback.
  - No stereoscopic dual-display rendering or interpupillary distance (IPD) optical adjustment.
  - Head motion is driven by mouse look rather than physical IMU neck tracking.

### 1.3. Mobile Augmented Reality Mode
- **Capability**: Complete AR Foundation and Google ARCore architecture (`ARSession`, `ARPlaneManager`, `ARRaycastManager`, `ARAnchorManager`, touch gestures).
- **CRITICAL DISTINCTION**: **IMPLEMENTED BUT NOT DEVICE-TESTED on physical Android phone in this session.**
- **Limitations**:
  - The installed Unity Editor (`6000.6.4f1`) on this workstation currently has `windowsstandalonesupport` and `WebGLSupport` playback engines installed, but **not** the Android Playback Engine or Android SDK/NDK modules.
  - Actual deployment to an Android device requires installing the Unity Android Build Support module via Unity Hub and deploying to an ARCore-certified phone (e.g., Google Pixel, Samsung Galaxy S series).
  - When running inside the Unity Editor on Desktop, the AR mode utilizes a virtual ground plane raycast simulation fallback so that panel placement and manipulation logic can be reviewed without hardware errors.

---

## 2. Unreal Engine Companion Demonstration Status
- **Requirement**: Address Course Outcome CO4 and Unit IV syllabus.
- **Limitation**: Epic Games Launcher is installed on this Dell G15 laptop, but the full 40GB+ Unreal Engine 5 Editor binaries are not installed.
- **Delivered Solution**: Complete architectural design, C++ header (`RealityOSPanel.h`), C++ implementation (`RealityOSPanel.cpp`), and step-by-step Unreal Engine setup guide (`UnrealCompanion/Documentation/UnrealSetupGuide.md`). No claims of live Unreal executable compilation on this machine are made.

---

## 3. Unity Licensing & Batchmode Behavior
- **Workstation Configuration**: Unity 6 (`6000.6.4f1`) with Unity Personal license.
- **Constraint**: Unity Personal licenses require the Unity Hub desktop client to be open and signed in. Headless command-line batchmode (`-batchmode`) requires a Unity Pro entitlement or interactive GUI activation.
- **Solution**: The automated test suite is provided as both NUnit C# tests and an independent Python test runner (`Tests/test_runner.py`), allowing regression testing directly from the CLI without licensing blocks.
