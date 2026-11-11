using UnityEngine;

namespace RealityOS.Core
{
    public interface IInputProvider
    {
        InteractionMode Mode { get; }
        bool IsInputActive { get; }
        
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsSprintPressed { get; }
        bool IsSelectPressed { get; }
        bool IsContextActionPressed { get; }
    }
}

