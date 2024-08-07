using Flyga.AdditionalAchievements.Textures.Cases;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Textures
{
    internal static class TextureManager
    {
        private static Notification _notification;

        private static Display _display;

        public static Notification Notification => _notification;

        public static Display Display => _display;

        public static void Initialize()
        {
            FreeResources();
            
            _notification = new Notification();
            _display = new Display();
        }

        public static async Task WaitUntilResolved()
        {
            await _notification.WaitUntilResolved();
            await _display.WaitUntilResolved();
        }

        public static void FreeResources()
        {
            _notification?.Dispose();

            _display?.Dispose();
        }
    }
}
