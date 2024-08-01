namespace Flyga.AdditionalAchievements.Integration
{
    /// <summary>
    /// Provides information on the current state of an integration.
    /// </summary>
    public enum IntegrationState
    {
        /// <summary>
        /// The <see cref="IntegrationState"/> has not been set.
        /// </summary>
        None,
        /// <summary>
        /// A dependency is currently not installed or could not be found.
        /// </summary>
        DependencyMissing,
        /// <summary>
        /// A dependency is installed and could be found, but is currently disabled.
        /// </summary>
        DependencyDisabled,
        /// <summary>
        /// The integration is working as intended.
        /// </summary>
        Working
    }
}
