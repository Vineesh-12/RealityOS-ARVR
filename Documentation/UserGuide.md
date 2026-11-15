# RealityOS User Guide

A complete operational guide for navigating and interacting within **RealityOS** across Desktop Spatial Mode, Mobile AR Mode, and Simulated XR Mode.

---

## 1. Main Menu Navigation

Upon launching RealityOS, the **MainMenu** presents a unified spatial hub:

- **Desktop Spatial Mode**: Enters the 3D desktop workspace designed for high-precision mouse & keyboard productivity.
- **Mobile AR Mode**: Activates AR Foundation surface detection and panel placement for Android ARCore devices.
- **Simulated XR Mode**: Enters the virtual XR room using simulated dual 6-DoF controllers, laser pointers, and grabbing mechanics.
- **Manage Workspaces**: Opens the Workspace Browser where you can create, load, rename, or delete workspaces.
- **Settings**: Configure mouse sensitivities, movement speeds, audio, and graphics quality.
- **Help & Shortcuts**: Comprehensive in-app reference for all interaction methods.

---

## 2. Desktop Spatial Mode

### 2.1. Camera Navigation Controls
| Action | Input Key / Button | Description |
| :--- | :--- | :--- |
| **Move Forward / Backward** | `W` / `S` | Smooth movement along camera look vector |
| **Strafe Left / Right** | `A` / `D` | Lateral movement across horizontal plane |
| **Elevate / Descend** | `Space` / `Q` | Vertical elevation in 3D space |
| **Sprint / Fast Movement** | `Left Shift` (Hold) | 2x acceleration multiplier |
| **Look Around / Orbit** | `Right Mouse Button` (Hold + Drag) | Freely aim camera yaw and pitch |
| **Zoom Distance** | `Mouse Scroll Wheel` | Dollying camera in and out |
| **Focus on Panel** | `F` | Centers camera view on the currently selected panel |
| **Reset Panel Rotation** | `R` | Re-orients selected panel to face the camera directly |
| **Duplicate Panel** | `Ctrl + D` | Instantiates a clone of the selected panel with offset |
| **Save Workspace** | `Ctrl + S` | Persists current layout and content immediately |
| **Delete Panel** | `Delete` | Removes the selected panel from workspace |

### 2.2. Panel Interaction
- **Select**: Left-click on any floating panel. A cyan outline indicates active selection.
- **Move / Drag**: Left-click and hold the panel header or body, then move the mouse to reposition the panel in 3D space.
- **Resize**: Drag the corner handles or use the scale methods.
- **Double Click**: Quick focus camera directly on the panel.
- **Layout Presets**: Use the top toolbar to automatically organize panels in a **3D Curved Arc** or a **Horizontal Grid**.

---

## 3. Mobile Augmented Reality Mode

### 3.1. Surface Scanning & Placement
1. **Scan Environment**: Move your mobile device slowly while pointing at floors, tables, or walls. The visual reticle will lock onto detected planes.
2. **Select Panel Type**: Use the dropdown at the bottom to choose between Notes, Task Board, Information, 3D Object Viewer, or Quick Access.
3. **Place**: Tap **Place Panel** to anchor the panel to the physical surface facing your device.

### 3.2. Touch Gestures
| Gesture | Target | Result |
| :--- | :--- | :--- |
| **Single Tap** | Panel Body | Selects the panel; brings up selection border |
| **One-Finger Drag** | Panel | Repositions the panel across the detected plane |
| **Two-Finger Pinch** | Selected Panel | Uniformly scales the panel up or down |
| **Two-Finger Twist** | Selected Panel | Rotates the panel around its vertical normal axis |
| **Double Tap** | Panel | Centers and focuses device view on panel |
| **Long Press (>0.6s)**| Panel | Opens context action menu (Duplicate, Delete, Reset) |

---

## 4. Simulated Extended Reality Mode

Simulated XR Mode allows testing of XR Interaction Toolkit mechanics without requiring a physical VR headset:

### 4.1. Simulated Controller Mappings
- **Right Hand Pointer / Laser**: Directed by mouse cursor or simulated right controller.
- **Select / Click UI**: Left Mouse Button (`Trigger`).
- **Grab Panel / Object**: Press `G` (`Grip`) while aiming at a panel. Move the controller to carry the panel in 3D space, then release `G` to drop it.
- **Left Hand Pointer**: Press `H` (`Left Grip`) or `T` (`Teleport Aim`).
- **Comfort Snap Turning**: Press `Q` to snap turn 45° left, or `E` to snap turn 45° right.
- **Teleportation**: Hold `T` to project the teleport arc on the floor, aim at your destination, and release `T` to teleport.

---

## 5. Panel Types & Functional Usage

### A. Notes Panel
- Editable title and multiline text area.
- Auto-updating character counter.
- Saves text automatically to local JSON storage.

### B. Task Board Panel
- Input box to type new tasks and click **Add**.
- Click any task to cycle its status: `[ ] Pending` -> `[~] In Progress` -> `[v] Completed`.
- Progress indicator shows live completed / total ratio.

### C. Information Panel
- Displays rich descriptive text and system metadata (coordinates, ID).
- Great for room guidelines, project specs, or instructions.

### D. 3D Object Viewer Panel
- Displays an interactive 3D spatial model in front of the panel.
- Supports orbit rotation, zoom magnification slider, and auto-spin toggle.

### E. Quick Access Panel
- User-configurable shortcuts to other workspaces or interaction modes.
- One-click navigation trigger.

### F. Laptop Integration Hub Panel
- **Launch Real Windows Applications**: Click buttons to launch Google Chrome, Windows Notepad, Calculator, VS Code, and File Explorer.
- **Live Notepad Synchronization**: Click **Sync Active Note to Notepad** to export the note to a physical `.txt` file and open it in Notepad. When you edit and save in Notepad (`Ctrl+S`), the 3D panel inside RealityOS updates instantly in real time via `FileSystemWatcher`!
- **Spatial Web Browsing**: Enter any web address (e.g. `github.com`) or click quick bookmark pills (Google, GitHub, Wikipedia, Unity Docs) to open verified web resources.
- **Side-by-Side Window Layouts**: Tile RealityOS on the left half of your laptop screen and Chrome or Notepad on the right half with one click using native Win32 window positioning (`SetWindowPos`).

---

## 6. Faculty Demonstration Walkthrough

When presenting RealityOS to faculty or evaluators, follow this recommended sequence:

1. **Launch from Bootstrap Scene**:
   - Open `Bootstrap.unity` and click Play.
   - Point out the unified Main Menu with the **Windows Integration** status strip showing native application readiness.
2. **Explore Desktop Spatial Mode**:
   - Click **Launch Desktop Mode**.
   - Use `W/A/S/D` to fly through 3D space, showing the floating panels with cyan selection borders.
   - Click the top toolbar button **Curved Arc** to demonstrate automated 3D spatial window arrangement.
3. **Showcase Windows Laptop Integration**:
   - Click `+ Laptop` to spawn the **Laptop Integration Hub**.
   - Click **Sync Active Note to Notepad**: show Windows Notepad popping up with the note text.
   - In Windows Notepad, type: `Demonstrated live to faculty at [Time]!`, press `Ctrl+S`, and switch back to Unity. Show that the floating 3D panel updated immediately without restarting!
   - Click **Tile Side-by-Side** to demonstrate Win32 window docking (Unity on left 50%, Chrome/Notepad on right 50%).
   - Click **Google** or **GitHub** to launch the browser from 3D space.
4. **Demonstrate Persistence**:
   - Press `Ctrl+S` to save.
   - Click **Menu** to return to the Main Menu and reload the workspace; show all panels restored with exact positions, rotations, and notes content.
5. **Demonstrate Simulated XR**:
   - Click **Launch Simulated XR**.
   - Show mouse-directed 6-DoF controllers, laser pointer targeting, grab panel with `G`, and comfort snap turns with `Q`/`E`.


