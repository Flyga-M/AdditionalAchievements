namespace Flyga.AdditionalAchievements.Status
{
    public enum Status
    {
        /// <summary>
        /// The status is unknown.
        /// </summary>
        Unknown,
        /// <summary>
        /// The service is running as expected.
        /// </summary>
        Normal,
        /// <summary>
        /// The service is running, but not with full functionality.
        /// </summary>
        Inhibited,
        /// <summary>
        /// The service has stopped running, but might continue at a later time.
        /// </summary>
        Paused,
        /// <summary>
        /// The service has stopped running and won't start again.
        /// </summary>
        Stopped
    }
}
