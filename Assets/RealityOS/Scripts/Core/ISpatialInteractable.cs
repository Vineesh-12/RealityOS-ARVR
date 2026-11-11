using UnityEngine;

namespace RealityOS.Core
{
    public interface ISpatialInteractable
    {
        bool IsSelected { get; }
        bool IsHovered { get; }
        
        void OnSelect();
        void OnDeselect();
        void OnHoverEnter();
        void OnHoverExit();
        
        Transform TransformTarget { get; }
    }
}
