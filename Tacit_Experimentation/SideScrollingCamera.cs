using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using System;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Graphics;


namespace Tacit_Experimentation
{
    public class SideScrollingCamera : AsyncScript
    {
        private static readonly Vector3 ForwardVector = new Vector3(0, 0, -1);

        private Vector3 position;
        private Vector3 targetPos;
        private float currentYPosition; // Track current vertical position
        private float targetYPosition;  // Track target vertical position
        float targetXPosition;
        private const float VERTICAL_SMOOTHING = 5.0f; // Adjust this value to control smoothing speed

        private CameraComponent Component => Entity?.Get<CameraComponent>();

        public float MouseWheelZoomSpeedFactor { get; set; } = 0.01f;
        private float Yaw { get; set; }
        private float Pitch { get; set; }
        public Entity Target { get; set; }
        public float DistanceFromTarget { get; set; } = 10.0f;
        public Vector3 OffsetFromTarget { get; set; }

        public override async Task Execute()
        {
            // Initialize camera settings
            Texture backBuffer = GraphicsDevice.Presenter.BackBuffer;
            if (backBuffer != null)
            {
                Component.AspectRatio = backBuffer.Width / (float)backBuffer.Height;
            }
            Component.NearClipPlane = CameraComponent.DefaultNearClipPlane;
            Component.FarClipPlane = CameraComponent.DefaultFarClipPlane;
            Component.UseCustomViewMatrix = true;
            Component.UseCustomAspectRatio = true;
            Reset();

            Input.LockMousePosition(true);

            // Initialize vertical position
            if (Target != null)
            {
                currentYPosition = Target.Transform.WorldMatrix.TranslationVector.Y;
                targetYPosition = currentYPosition;
            }

            while (true)
            {
                UpdateCamera();
                await Script.NextFrame();
            }
        }

        public void Reset()
        {
            Pitch = (float)Math.Atan2(
                2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.W -
                2 * Entity.Transform.Rotation.Y * Entity.Transform.Rotation.Z,
                1 - 2 * Entity.Transform.Rotation.X * Entity.Transform.Rotation.X -
                2 * Entity.Transform.Rotation.Z * Entity.Transform.Rotation.Z);
            position = Entity.Transform.Position;

            if (Target != null)
            {
                currentYPosition = Target.Transform.WorldMatrix.TranslationVector.Y;
                targetYPosition = currentYPosition;
            }
        }

        /// <summary>
        /// Updates the camera position with smooth vertical tracking.
        /// The camera now slides smoothly up and down an imaginary vertical pole while maintaining
        /// its horizontal distance from the target. Vertical movement uses interpolation for smoothness.
        /// </summary>
        protected virtual void UpdateCamera()
        {
            if (Target == null) return;

            HandleInput();

            // Calculate rotation and direction
            Matrix rotation = Matrix.RotationYawPitchRoll(Yaw, Pitch, 0);
            Vector3 direction = Vector3.Normalize(Vector3.TransformNormal(ForwardVector, rotation));

            // Update target vertical position
            targetYPosition = Target.Transform.WorldMatrix.TranslationVector.Y + OffsetFromTarget.Y;

            // Smooth interpolation for vertical movement
            currentYPosition = (float)MathUtil.Lerp(currentYPosition, targetYPosition,
                this.Game.UpdateTime.Elapsed.TotalSeconds * VERTICAL_SMOOTHING);

            targetXPosition = 0f;

            // Construct target position maintaining vertical sliding behavior
            targetPos = new Vector3(
                targetXPosition + OffsetFromTarget.X,
                currentYPosition,
                Target.Transform.WorldMatrix.TranslationVector.Z + OffsetFromTarget.Z
            );

            // Calculate camera position while maintaining distance
            position = targetPos - direction * DistanceFromTarget;

            UpdateViewMatrix();
        }

        private void HandleInput()
        {
            var rotate = Input.IsMousePositionLocked;

            if (Input.IsKeyReleased(Keys.LeftAlt) ||
                Input.IsKeyReleased(Keys.RightAlt) ||
                Input.IsKeyReleased(Keys.Escape))
            {
                if (rotate)
                    Input.UnlockMousePosition();
                else
                    Input.LockMousePosition(true);
            }

            // Handle zoom input
            if (Math.Abs(Input.MouseWheelDelta) > MathUtil.ZeroTolerance)
            {
                DistanceFromTarget -= MouseWheelZoomSpeedFactor * Input.MouseWheelDelta;
                DistanceFromTarget = Math.Max(0.0f, DistanceFromTarget);
            }
        }

        private void UpdateViewMatrix()
        {
            CameraComponent camera = Component;
            if (camera == null) return;

            Quaternion rotation = Quaternion.Invert(Quaternion.RotationYawPitchRoll(Yaw, Pitch, 0));
            Matrix viewMatrix = Matrix.Translation(-position) * Matrix.RotationQuaternion(rotation);
            camera.ViewMatrix = viewMatrix;
        }
    }
}