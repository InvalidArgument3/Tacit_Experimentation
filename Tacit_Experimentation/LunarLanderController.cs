using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using Stride.Input;

public class LunarLanderController : SyncScript
{
    // Thrust and rotation variables
    public float ThrustForce = 10f;     // Controls the amount of forward thrust
    public float TorqueForce = 1f;      // Controls the torque applied for rotation

    private RigidbodyComponent rigidbody;

    public override void Start()
    {
        // Retrieve the RigidbodyComponent for applying physics
        rigidbody = Entity.Get<RigidbodyComponent>();

        // Ensure a RigidbodyComponent is attached
        if (rigidbody == null)
        {
            Log.Error("RigidbodyComponent missing. Please add a RigidbodyComponent to the entity.");
        }
    }

    public override void Update()
    {
        // If there's no Rigidbody, we can't apply physics, so return early
        if (rigidbody == null) return;

        // Lock the entity to the 2D plane by setting Z position and Z velocity to zero
        Entity.Transform.Position.Z = 0;
        rigidbody.LinearVelocity = new Vector3(rigidbody.LinearVelocity.X, rigidbody.LinearVelocity.Y, 0);
        rigidbody.AngularVelocity = new Vector3(0, 0, rigidbody.AngularVelocity.Z);

        // Rotation control: Apply torque for realistic rotation around the Z-axis only
        if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
        {
            // Apply positive torque to rotate counterclockwise
            rigidbody.ApplyTorque(Vector3.UnitZ * TorqueForce);
        }
        else if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
        {
            // Apply negative torque to rotate clockwise
            rigidbody.ApplyTorque(Vector3.UnitZ * -TorqueForce);
        }

        // Thrust control: Apply force in the forward direction of the entity
        if (Input.IsKeyDown(Keys.Space))
        {
            // Calculate the forward direction based on the entity's rotation
            Vector3 forwardDirection = Vector3.Transform(Vector3.UnitY, Entity.Transform.Rotation);

            // Apply the thrust force in the forward direction
            rigidbody.ApplyForce(forwardDirection * ThrustForce);
        }
    }
}
