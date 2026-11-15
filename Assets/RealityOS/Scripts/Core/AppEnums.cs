using System;

namespace RealityOS.Core
{
    public enum InteractionMode
    {
        DesktopSpatial,
        MobileAR,
        SimulatedXR
    }

    public enum PanelType
    {
        Notes,
        TaskBoard,
        Information,
        ObjectViewer,
        QuickAccess,
        LaptopConnect,
        Browser
    }

    public enum ExternalAppType
    {
        Chrome,
        Notepad,
        Calculator,
        VSCode,
        FileExplorer
    }

    public enum WindowLayoutPreset
    {
        SideBySide,
        DockRight,
        MaximizeExternal,
        Restore
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public enum ManipulationMode
    {
        None,
        Moving,
        Rotating,
        Resizing
    }

    public enum TrackingState
    {
        Unavailable,
        Limited,
        Tracking
    }
}

