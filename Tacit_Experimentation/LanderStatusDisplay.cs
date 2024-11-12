using Stride.Engine;
using Stride.UI.Controls;
using System.Linq;
using System.Text;

public class LanderStatusDisplay : SyncScript
{
    // Reference to the LunarLanderController script attached to the entity
    private LunarLanderController landerController;

    // Reference to the TextBlock UI element on the InfoPanel page
    private TextBlock textBlockOutput;

    public override void Start()
    {
        // Retrieve the LunarLanderController script
        landerController = Entity.Get<LunarLanderController>();
        if (landerController == null)
        {
            Log.Error("LunarLanderController component is missing.");
            return;
        }

        // Find the UI entity named InfoPanel
        var uiEntity = Entity.Scene.Entities.FirstOrDefault(e => e.Name == "InfoPanelEntity");
        if (uiEntity != null)
        {
            // Retrieve the UIComponent and find the TextBlock named "TextBlockOutput"
            var uiComponent = uiEntity.Get<UIComponent>();
            textBlockOutput = uiComponent.Page.RootElement.FindName("TextBlockOutput") as TextBlock;

            if (textBlockOutput == null)
            {
                Log.Error("TextBlockOutput not found on InfoPanel.");
            }
        }
        else
        {
            Log.Error("InfoPanel UI page not found.");
        }
    }

    public override void Update()
    {
        if (landerController == null || textBlockOutput == null) return;

        var statusText = new StringBuilder();
        statusText.AppendLine($"Throttle: {landerController.currentThrottle:F2}");
        statusText.AppendLine($"Effective Thrust: {landerController.effectiveThrust:F2}");

        // Check if distanceToGround is out of range and adjust display accordingly
        if (landerController.distanceToGround >= 9999f)
        {
            statusText.AppendLine("Distance to Ground: Out of Range");
        }
        else
        {
            statusText.AppendLine($"Distance to Ground: {landerController.distanceToGround:F2}");
        }

        statusText.AppendLine($"Is Near Ground: {landerController.isNearGround}");
        statusText.AppendLine($"Target Throttle: {landerController.targetThrottle:F2}");
        statusText.AppendLine($"Vertical Damping Factor: {landerController.VerticalDampingFactor:F2}");

        textBlockOutput.Text = statusText.ToString();
    }

}
