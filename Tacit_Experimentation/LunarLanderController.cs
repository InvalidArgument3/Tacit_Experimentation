using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using Stride.Input;
using System;

public class LunarLanderController : SyncScript
{
    // Original control variables
    public float ThrustForce = 10f;
    public float TorqueForce = 1f;

    // Stabilization parameters
    public float StabilizationTorqueFactor = 2.0f;
    public float AngularDampingFactor = 0.5f;
    public float StabilizationDeadzone = 0.01f;
    public float MaxStabilizationTorque = 5.0f;
    public float StabilizationDelay = 0.5f;

    // Throttle control parameters
    public float ThrottleSmoothingFactor = 3.0f;          // Controls how quickly throttle changes
    public float ThrottleGravityCompensationFactor = 0.4f; // Throttle needed to counter gravity
    public float ThrottleIncrementRate = 0.5f;            // Rate of throttle change per second
    public float MaxThrust = 20f;                         // Maximum possible thrust force
    public float VerticalDampingFactor = 0.1f;            // Dampens vertical velocity

    // Auto-pitch control parameters
    public float MaxPitchAngle = 45f;              // Maximum allowed pitch angle in degrees
    public float PitchRecoveryFactor = 2.0f;       // How strongly to correct excessive pitch
    public float PitchStabilizationSpeed = 3.0f;   // How quickly to return to level

    // Ground cushioning parameters
    public float GroundCushionHeight = 5.0f;       // Height at which cushioning begins
    public float CushionStrengthFactor = 2.0f;     // Multiplier for cushioning force
    public float DownwardVelocityThreshold = -5f;  // Velocity at which cushioning activates

    // Runtime variables
    private RigidbodyComponent rigidbody;
    private float lastUserInputTime;
    private bool isUserRotating;
    public float currentThrottle;       // Current throttle value (0 to 1)
    public float targetThrottle;        // Target throttle value
    public float effectiveThrust;       // Calculated thrust after smoothing
    public float distanceToGround;
    public bool isNearGround;

    public override void Start()
    {
        rigidbody = Entity.Get<RigidbodyComponent>();

        if (rigidbody == null)
        {
            Log.Error("RigidbodyComponent missing. Please add a RigidbodyComponent to the entity.");
            return;
        }

        rigidbody.AngularDamping = AngularDampingFactor;
        lastUserInputTime = 0f;

        // Initialize throttle values
        currentThrottle = ThrottleGravityCompensationFactor;
        targetThrottle = currentThrottle;
        effectiveThrust = 0f;
    }

    public override void Update()
    {
        if (rigidbody == null) return;

        UpdateGroundDistance();
        HandlePositionConstraints();
        UpdateThrottle();
        HandleRotationInput();
        ApplyPitchControl();
        ApplyStabilization();
        ApplyThrust();
        ApplyGroundCushioning();
    }

    private void HandlePositionConstraints()
    {
        Entity.Transform.Position.Z = 0;
        rigidbody.LinearVelocity = new Vector3(rigidbody.LinearVelocity.X, rigidbody.LinearVelocity.Y, 0);
        rigidbody.AngularVelocity = new Vector3(0, 0, rigidbody.AngularVelocity.Z);
    }

    private void UpdateThrottle()
    {
        float deltaTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

        // Update target throttle based on input
        if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W))
        {
            targetThrottle += ThrottleIncrementRate * deltaTime;
        }
        else if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S))
        {
            targetThrottle -= ThrottleIncrementRate * deltaTime;
        }

        // Clamp target throttle
        targetThrottle = MathUtil.Clamp(targetThrottle, 0f, 1f);

        // Smooth throttle transition
        currentThrottle = MathUtil.Lerp(currentThrottle, targetThrottle,
            ThrottleSmoothingFactor * deltaTime);

        // Calculate effective thrust with gravity compensation
        float gravityCompensation = ThrottleGravityCompensationFactor;
        effectiveThrust = MaxThrust * (currentThrottle + gravityCompensation);
    }

    private void HandleRotationInput()
    {
        isUserRotating = false;

        if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
        {
            rigidbody.ApplyTorque(Vector3.UnitZ * TorqueForce);
            UpdateUserInputTime();
        }
        else if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
        {
            rigidbody.ApplyTorque(Vector3.UnitZ * -TorqueForce);
            UpdateUserInputTime();
        }
    }

    private void UpdateUserInputTime()
    {
        lastUserInputTime = (float)Game.UpdateTime.Total.TotalSeconds;
        isUserRotating = true;
    }

    private void ApplyPitchControl()
    {
        // Get current pitch angle in degrees
        float currentPitch = GetCurrentPitchAngle();

        // If pitch exceeds maximum allowed angle, apply corrective torque
        if (Math.Abs(currentPitch) > MaxPitchAngle)
        {
            float exceededAngle = Math.Abs(currentPitch) - MaxPitchAngle;
            float correctionDirection = -Math.Sign(currentPitch);
            float correctionTorque = exceededAngle * PitchRecoveryFactor;

            // Apply stronger correction the more the angle is exceeded
            rigidbody.ApplyTorque(Vector3.UnitZ * correctionTorque * correctionDirection);
        }
        // Apply gentler stabilization when not actively rotating
        else if (!isUserRotating)
        {
            float stabilizationTorque = -currentPitch * PitchStabilizationSpeed;
            rigidbody.ApplyTorque(Vector3.UnitZ * stabilizationTorque);
        }
    }

    private void ApplyStabilization()
    {
        if (!isUserRotating && (Game.UpdateTime.Total.TotalSeconds - lastUserInputTime) > StabilizationDelay)
        {
            float currentRotation = GetCurrentZRotation();

            if (Math.Abs(currentRotation) < StabilizationDeadzone) return;

            float stabilizationTorque = -currentRotation * StabilizationTorqueFactor;
            float dampingTorque = -rigidbody.AngularVelocity.Z * AngularDampingFactor;
            stabilizationTorque += dampingTorque;

            stabilizationTorque = MathUtil.Clamp(stabilizationTorque, -MaxStabilizationTorque, MaxStabilizationTorque);
            rigidbody.ApplyTorque(Vector3.UnitZ * stabilizationTorque);
        }
    }

    private void ApplyGroundCushioning()
    {
        if (isNearGround)
        {
            float verticalVelocity = rigidbody.LinearVelocity.Y;

            // Only cushion if moving downward faster than threshold
            if (verticalVelocity < DownwardVelocityThreshold)
            {
                // Calculate cushioning force based on distance and velocity
                float cushionIntensity = 1.0f - (distanceToGround / GroundCushionHeight);
                float velocityFactor = Math.Abs(verticalVelocity / DownwardVelocityThreshold);
                float cushioningForce = MaxThrust * cushionIntensity * velocityFactor * CushionStrengthFactor;

                // Apply upward force
                rigidbody.ApplyForce(Vector3.UnitY * cushioningForce);
            }
        }
    }

    private void UpdateGroundDistance()
    {
        var raycastStart = Entity.Transform.WorldMatrix.TranslationVector;
        var raycastEnd = raycastStart - Vector3.UnitY * GroundCushionHeight;

        // Perform raycast to detect the ground
        Simulation simulation = this.GetSimulation();
        var result = simulation.Raycast(raycastStart, raycastEnd, CollisionFilterGroups.DefaultFilter, CollisionFilterGroupFlags.DefaultFilter);

        if (result.Succeeded)
        {
            // Calculate and set distance to ground
            distanceToGround = (raycastStart - result.Point).Length();
            isNearGround = distanceToGround <= GroundCushionHeight;
        }
        else
        {
            // If ground is not detected, set distanceToGround to a high but reasonable default, or mark as out of range
            distanceToGround = 9999f; // or a maximum value you'd like to represent "out of range"
            isNearGround = false;
        }
    }

    private float GetCurrentPitchAngle()
    {
        // Convert quaternion rotation to pitch angle in degrees
        Quaternion rotation = Entity.Transform.Rotation;
        float pitch = (float)Math.Atan2(
            2 * (rotation.W * rotation.X - rotation.Y * rotation.Z),
            1 - 2 * (rotation.X * rotation.X + rotation.Y * rotation.Y));
        return MathUtil.RadiansToDegrees(pitch);
    }

    private float GetCurrentZRotation()
    {
        Quaternion rotation = Entity.Transform.Rotation;
        return (float)Math.Atan2(
            2 * (rotation.W * rotation.Z + rotation.X * rotation.Y),
            1 - 2 * (rotation.Y * rotation.Y + rotation.Z * rotation.Z));
    }

    private void ApplyThrust()
    {
        Vector3 thrustDirection = Vector3.Transform(Vector3.UnitY, Entity.Transform.Rotation);

        // Apply main thrust, potentially modified by ground cushioning
        rigidbody.ApplyForce(thrustDirection * effectiveThrust);

        // Apply vertical damping with reduced effect near ground
        float dampingMultiplier = isNearGround ? 0.5f : 1.0f;
        Vector3 verticalVelocity = new Vector3(0, rigidbody.LinearVelocity.Y, 0);
        rigidbody.ApplyForce(-verticalVelocity * VerticalDampingFactor * dampingMultiplier);
    }
}
