using Blish_HUD.Gw2WebApi;
using Blish_HUD.Modules.Managers;
using System;
using System.Reflection;

namespace Flyga.AdditionalAchievements
{
    public static class Gw2ApiManagerExtensions
    {
        /// <summary>
        /// Attempts to determine, whether the <see cref="Gw2ApiManager"/> has a subtoken 
        /// assigned.
        /// </summary>
        /// <remarks>
        /// This extension method is obsolete upwards of Blish HUD 1.1.1, since the 
        /// <see cref="Gw2ApiManager"/> has it's own HasSubtoken field that does the same.
        /// </remarks>
        /// <param name="gw2ApiManager"></param>
        /// <param name="hasSubtoken">Determines whether the <see cref="Gw2ApiManager"/> 
        /// has a subtoken assigned. Is only correct, if this method returns 
        /// <see langword="true"/>.</param>
        /// <returns><see langword="true"/>, if the attempt to determine whether the 
        /// <see cref="Gw2ApiManager"/> has a subtoken assigned was successfull. 
        /// Otherwise <see langword="false"/>.</returns>
        public static bool TryHasSubtoken(this Gw2ApiManager gw2ApiManager, out bool hasSubtoken)
        {
            hasSubtoken = false;

            // not neccessary in newer Blish HUD versions
            // _gw2ApiManager.HasSubtoken does it
            FieldInfo connectionField = gw2ApiManager.GetType().GetField(
                "_connection",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (connectionField != null)
            {
                try
                {
                    if (connectionField.GetValue(gw2ApiManager) is ManagedConnection connection)
                    {
                        hasSubtoken = !string.IsNullOrEmpty(connection.Connection.AccessToken);
                        return true;
                    }
                }
                catch (Exception)
                {
                    // do nothing, since there's a catch all return false at the end
                }
            }

            return false;
        }
    }
}
