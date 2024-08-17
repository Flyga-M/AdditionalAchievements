using AchievementLib.Pack;
using Blish_HUD;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Integration
{
    /// <summary>
    /// A wrapper for the <see cref="GraphicsDeviceContext"/> to work with the 
    /// <see cref="AchievementLib"/>.
    /// </summary>
    /// <remarks>
    /// This was necessary to implement, because previously the loading of an achievement pack was 
    /// just wrapped inside a big GameService.Graphics.QueueMainThreadRender() which would then 
    /// freeze Blish HUD for the entire time the packs were being loaded.
    /// </remarks>
    public class GraphicsDeviceProvider : IGraphicsDeviceProvider
    {
        private static GraphicsService Graphics => GameService.Graphics;

        public static GraphicsDeviceProvider Instance { get; private set; }

        private GraphicsDeviceProvider()
        { /** NOOP **/ }

        public static void Initialize()
        {
            Instance = new GraphicsDeviceProvider();
        }

        public void LendGraphicsDevice(Action<GraphicsDevice> action)
        {
            GraphicsDeviceContext ctx = Graphics.LendGraphicsDeviceContext();
            
            try
            {
                action(ctx.GraphicsDevice);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                ctx.Dispose();
            }
        }
    }
}
