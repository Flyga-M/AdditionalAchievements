using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyga.AdditionalAchievements.Textures.Cases
{
    public class Repo : IDisposable
    {
        private readonly TextureReference _pkgBodyBackground;

        public Texture2D PkgBodyBackground => _pkgBodyBackground;

        public Repo()
        {
            _pkgBodyBackground = new TextureReference(155209);
        }

        public void Dispose()
        {
            _pkgBodyBackground?.Dispose();
        }
    }
}
