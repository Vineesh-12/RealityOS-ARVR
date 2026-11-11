using UnityEngine;

namespace RealityOS.Core
{
    public interface IManipulationHandler
    {
        ManipulationMode CurrentMode { get; }
        
        void BeginManipulation(ManipulationMode mode);
        void EndManipulation();
        
        void MoveTo(Vector3 worldPosition);
        void TranslateDelta(Vector3 deltaMovement);
        void RotateBy(Vector3 deltaEuler);
        void ScaleBy(float scaleMultiplier);
        void SetScale(Vector3 newScale);
    }
}

