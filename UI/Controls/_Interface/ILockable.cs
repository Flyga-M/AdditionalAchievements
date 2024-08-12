namespace Flyga.AdditionalAchievements.UI.Controls
{
    public interface ILockable
    {
        /// <summary>
        /// Determines whether the <see cref="ILockable"/> is locked.
        /// </summary>
        bool IsLocked { get; }
    }
}
