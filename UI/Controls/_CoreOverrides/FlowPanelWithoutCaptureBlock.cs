using Blish_HUD.Controls;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    public class FlowPanelWithoutCaptureBlock : FlowPanel
    {
        protected override CaptureType CapturesInput()
        {
            return base.CapturesInput() | CaptureType.DoNotBlock;
        }
    }
}
