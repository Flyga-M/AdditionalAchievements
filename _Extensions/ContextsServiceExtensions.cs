using Blish_HUD;
using Blish_HUD.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flyga.AdditionalAchievements
{
    public static class ContextsServiceExtensions
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(ContextsServiceExtensions));

        private const string REGISTERED_CONTEXTS_FIELD_NAME = "_registeredContexts";

        /// <summary>
        /// Gets a registered <see cref="Context"/> by <paramref name="typeName"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="typeName"/> has to be the full name, including namespace. See <see cref="Type.FullName"/>.
        /// </remarks>
        /// <param name="contexts"></param>
        /// <param name="typeName"></param>
        /// <returns>
        /// The registered <see cref="Context"/> with typename <paramref name="typeName"/> or
        /// <see langword="null"/>, if no <see cref="Context"/> with that <paramref name="typeName"/> is
        /// currently registered.
        /// </returns>
        public static Context GetContext(this ContextsService contexts, string typeName)
        {
            if (contexts == null)
            {
                throw new ArgumentNullException(nameof(contexts));
            }
            
            object registeredContextsValue;

            try
            {
                FieldInfo[] fields = contexts.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                FieldInfo registeredContextsInfo = fields.Where(field => field.Name == REGISTERED_CONTEXTS_FIELD_NAME).FirstOrDefault();

                if (registeredContextsInfo == null)
                {
                    Logger.Error($"Unable to retrieve context by {nameof(typeName)} ({typeName}), because no private field with " +
                        $"the name {REGISTERED_CONTEXTS_FIELD_NAME} exists on the type {contexts.GetType()}");
                    return null;
                }

                registeredContextsValue = registeredContextsInfo.GetValue(contexts);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to retrieve context by {nameof(typeName)} ({typeName}), because an exception during " +
                    $"reflection was thrown: {ex}");
                return null;
            }
            

            if (!(registeredContextsValue is Dictionary<Type, Context> _registeredContexts))
            {
                Logger.Error($"Unable to retrieve context by {nameof(typeName)} ({typeName}), because the private field with " +
                    $"the name {REGISTERED_CONTEXTS_FIELD_NAME} is not of the expected type {typeof(Dictionary<Type, Context>)}. " +
                    $"Given type: {registeredContextsValue.GetType()}.");
                return null;
            }

            foreach (KeyValuePair<Type, Context> context in _registeredContexts)
            {
                if (context.Key.FullName == typeName)
                {
                    return context.Value;
                }
            }

            return null;
        }
    }
}
