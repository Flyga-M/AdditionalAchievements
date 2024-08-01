namespace Flyga.AdditionalAchievements.Solve.Handler
{
    /// <summary>
    /// Contains information on the state of an <see cref="IActionHandler{TAction}"/>.
    /// </summary>
    public enum HandlerState
    {
        /// <summary>
        /// <see cref="HandlerState"/> has not been set.
        /// </summary>
        None,
        /// <summary>
        /// Working as intended.
        /// </summary>
        Working,
        /// <summary>
        /// Suspended, because neccessary services or contexts are currently not available, 
        /// but might become available again.
        /// </summary>
        Suspended,
        /// <summary>
        /// Some functionality is suspended, because a neccessary service or context is currently 
        /// OR PERMANENTLY not available.
        /// </summary>
        PartiallySuspended,
        /// <summary>
        /// Stopped working, because neccessary services or contexts are unavailable and will not 
        /// become available again.
        /// </summary>
        Fatal,
        /// <summary>
        /// The handler was disposed.
        /// </summary>
        Disposed
    }
}
