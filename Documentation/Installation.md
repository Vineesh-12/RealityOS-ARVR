# RealityOS Installation and Setup Guide

This guide describes how to open, configure, run, and build **RealityOS** on your system.

---

## 1. System Requirements

### Workstation (Validated Target)
- **Computer**: Dell G15 Laptop
- **OS**: Windows 11 (64-bit)
- **CPU**: Intel Core i5 / i7 or AMD Ryzen 5 / 7
- **GPU**: NVIDIA GeForce RTX 3050 Laptop GPU (4GB VRAM)
- **RAM**: 16 GB DDR4/DDR5
- **Storage**: At least 10 GB free SSD storage

### Software Prerequisites
- **Unity Editor**: Unity 6 (`6000.6.4f1`)
- **Unity Hub**: Version 3.0+
- **Git**: Version 2.40+
- **Python**: Version 3.10+ (for running automated test suite)
- **Optional**: Android Studio & OpenJDK 17 (for Android AR builds)

---

## 2. Opening the Project in Unity Hub

1. Launch **Unity Hub** from your desktop or start menu.
2. In the Projects tab, click the **Add** button (or dropdown: "Add project from disk").
3. Navigate to and select the project directory:
   `C:\Users\vinee\Downloads\ARVR`
4. Confirm that Unity Hub detects Editor Version **6000.6.4f1**.
5. Click on the project name to open it.
6. Unity will automatically parse `Packages/manifest.json` and download required packages:
   - Universal Render Pipeline (`com.unity.render-pipelines.universal`)
   - Input System (`com.unity.inputsystem`)
   - Unity UI / TextMeshPro (`com.unity.ugui`)
   - AR Foundation (`com.unity.xr.arfoundation`)
   - Google ARCore XR Plugin (`com.unity.xr.arcore`)
   - XR Interaction Toolkit (`com.unity.xr.interaction.toolkit`)
   - OpenXR Plugin (`com.unity.xr.openxr`)

---

## 3. Running in the Unity Editor

1. In the **Project** window, navigate to `Assets > RealityOS > Scenes`.
2. Double-click `Bootstrap.unity` to open the bootstrap scene.
3. Press the **Play** button at the top of the Unity Editor.
4. The bootstrap manager initializes runtime singletons and opens the **MainMenu**.
5. Select any of the three modes:
   - **Desktop Spatial Mode**: Navigate with WASD + Right Mouse Button, manipulate panels with Left Mouse Button.
   - **Simulated XR Mode**: Experience simulated controllers, grabbing (G), teleportation (T), and snap turning (Q/E).
   - **Mobile AR Mode**: Review AR plane placement reticle and touch controls.

---

## 4. Running the Automated Test Suite

RealityOS includes a standalone automated regression and persistence test suite that can be run from PowerShell or terminal without opening Unity:

```powershell
cd C:\Users\vinee\Downloads\ARVR
python Tests\test_runner.py
```

### Expected Output:
```
============================================================
RealityOS Automated Test Runner
============================================================
Test Execution Results:
 [PASS] Test01_WorkspaceSerialization_ProducesValidJson
 [PASS] Test02_WorkspaceDeserialization_RestoresAllFieldsAccurately
 [PASS] Test03_PanelDataValidation_FixesNullsAndClampsExtremeScales
 [PASS] Test04_WorkspaceCRUD_FullLifecycle
 [PASS] Test05_PanelClone_GeneratesUniqueIdAndOffsetPosition
 [PASS] Test06_CorruptJson_GracefulRecoveryWithoutCrash
 [PASS] Test07_MultiWorkspaceListing_AndSampleShowcase
============================================================
Summary: 7 / 7 tests passed.
============================================================
```

---

## 5. Building the Desktop Application (Windows Standalone)

1. Open `File > Build Profiles` (or `Build Settings`).
2. Verify all 6 scenes are checked in the build list:
   - `Assets/RealityOS/Scenes/Bootstrap.unity` (Index 0)
   - `Assets/RealityOS/Scenes/MainMenu.unity` (Index 1)
   - `Assets/RealityOS/Scenes/DesktopWorkspace.unity` (Index 2)
   - `Assets/RealityOS/Scenes/ARWorkspace.unity` (Index 3)
   - `Assets/RealityOS/Scenes/XRSimulator.unity` (Index 4)
   - `Assets/RealityOS/Scenes/DemoWorkspace.unity` (Index 5)
3. Target Platform: **Windows, Mac, Linux** > Architecture: **Intel 64-bit**.
4. Click **Build**, select an output folder (e.g. `Builds/Windows/`), and name the executable `RealityOS.exe`.

---

## 6. Android Mobile AR Build Setup (When Target Hardware is Available)

To compile the actual ARCore build for an Android smartphone:
1. In Unity Hub, go to **Installs > 6000.6.4f1 > Add Modules**.
2. Check **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK**.
3. In Unity Editor, go to `File > Build Settings` and switch platform to **Android**.
4. In `Project Settings > XR Plug-in Management`, check **Google ARCore** under the Android tab.
5. In `Project Settings > Player > Other Settings`:
   - Set Minimum API Level: **Android 7.0 'Nougat' (API Level 24)** or higher.
   - Scripting Backend: **IL2CPP**.
   - Target Architectures: **ARM64**.
6. Connect your phone via USB with **USB Debugging** enabled and click **Build and Run**.

