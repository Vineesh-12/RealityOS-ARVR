using UnityEngine;
using RealityOS.Persistence;

namespace RealityOS.Core
{
    public interface IWorkspacePanel : ISpatialInteractable, IManipulationHandler
    {
        string PanelId { get; }
        PanelType PanelType { get; }
        string Title { get; set; }
        
        PanelData ExportData();
        void ImportData(PanelData data);
        
        void Duplicate();
        void ClosePanel();
        void FocusPanel();
        void ResetOrientation();
        void BringToForeground();
    }
}

