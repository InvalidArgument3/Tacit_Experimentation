using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;

public class Simple2DMovement : SyncScript
{
    // Variables for speed
    public float Speed = 5f;

    public override void Start()
    {
        // Initialization if needed
    }

    public override void Update()
    {
        // Check if we have a valid input
        if (Input == null) return;

        // Initialize movement vector
        Vector2 moveDirection = Vector2.Zero;

        // Check for horizontal movement
        if (Input.IsKeyDown(Keys.Left) || Input.IsKeyDown(Keys.A))
            moveDirection.X -= 1f; // Move left
        if (Input.IsKeyDown(Keys.Right) || Input.IsKeyDown(Keys.D))
            moveDirection.X += 1f; // Move right

        // Check for vertical movement
        if (Input.IsKeyDown(Keys.Up) || Input.IsKeyDown(Keys.W))
            moveDirection.Y += 1f; // Move up
        if (Input.IsKeyDown(Keys.Down) || Input.IsKeyDown(Keys.S))
            moveDirection.Y -= 1f; // Move down

        // Normalize the direction to prevent faster diagonal movement
        if (moveDirection.Length() > 1)
            moveDirection.Normalize();

        // Apply the movement based on the speed and deltaTime for smooth movement
        Entity.Transform.Position += new Vector3(moveDirection * Speed * (float)Game.UpdateTime.Elapsed.TotalSeconds, 0);
    }
}
