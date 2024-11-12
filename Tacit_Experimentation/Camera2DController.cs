using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using System;

namespace Tacit_Experimentation
{
    public class CameraController2D : SyncScript
    {
        // Public properties for configuration in the editor
        public Entity PlayerDrone { get; set; }
        public Entity CameraTarget { get; set; }
        public float DroneRadius { get; set; } = 1.0f;
        public float SmoothTime { get; set; } = 0.3f;
        public float ZoomSpeed { get; set; } = 2.0f;
        public float MinZoom { get; set; } = 5.0f;
        public float MaxZoom { get; set; } = 15.0f;

        // Private fields
        private CameraComponent cameraComponent;
        private Vector3 currentVelocity;
        private float currentZoom;
        private float targetZoom;

        public override void Start()
        {
            // Validate required references
            if (PlayerDrone == null)
                throw new System.Exception("PlayerDrone reference is required");
            if (CameraTarget == null)
                throw new System.Exception("CameraTarget reference is required");

            // Get camera component
            cameraComponent = Entity.Get<CameraComponent>();
            if (cameraComponent == null)
                throw new System.Exception("CameraComponent not found on entity");

            // Initialize zoom
            currentZoom = targetZoom = 10.0f;

            // Set initial camera position
            UpdateCameraPosition(true);
        }

        public override void Update()
        {
            HandleZoomInput();
            UpdateCameraPosition();
        }

        private void HandleZoomInput()
        {
            if(Input.IsKeyDown(Keys.LeftAlt))
            {
                ResetCamera();
            }
            // Handle zoom input from mouse wheel
            float zoomDelta = Input.MouseWheelDelta;
            if (zoomDelta != 0)
            {
                targetZoom = MathUtil.Clamp(targetZoom - zoomDelta * ZoomSpeed, MinZoom, MaxZoom);
            }

            // Smoothly interpolate current zoom to target zoom
            currentZoom = MathUtil.Lerp(currentZoom, targetZoom, (float)Game.UpdateTime.Elapsed.TotalSeconds * 4.0f);
        }

        private void UpdateCameraPosition(bool immediate = false)
        {
            // Calculate target position based on player position
            Vector3 targetPosition = CameraTarget.Transform.Position;

            // Only update Y position to maintain side-scrolling perspective
            Vector3 currentPosition = Entity.Transform.Position;
            Vector3 newPosition = immediate
                ? targetPosition
                : Vector3.Lerp(currentPosition, targetPosition, SmoothTime * (float)Game.UpdateTime.Elapsed.TotalSeconds);

            // Update camera position
            Entity.Transform.Position = new Vector3(
                currentPosition.X,
                newPosition.Y,
                -currentZoom); // Adjust Z based on zoom level

            // Ensure camera is looking at the target
            Vector3 direction = targetPosition - Entity.Transform.Position;
            if (direction.Length() > 0)
            {
                direction.Normalize();
                Quaternion targetRotation = Quaternion.RotationYawPitchRoll(
                    MathF.Atan2(direction.X, -direction.Z),
                    MathF.Atan2(direction.Y, direction.Length()),
                    0);
                Entity.Transform.Rotation = targetRotation;
            }
        }

        // Helper method to reset camera position and zoom
        public void ResetCamera()
        {
            targetZoom = 10.0f;
            currentZoom = targetZoom;
            UpdateCameraPosition(true);
        }
    }
}
