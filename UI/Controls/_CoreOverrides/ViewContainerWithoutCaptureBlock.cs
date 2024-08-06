using Blish_HUD.Controls;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    public class ViewContainerWithoutCaptureBlock : ViewContainer
    {
        protected override CaptureType CapturesInput()
        {
            return base.CapturesInput() | CaptureType.DoNotBlock;
        }
    }
}
