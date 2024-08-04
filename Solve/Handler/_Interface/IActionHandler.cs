using AchievementLib.Pack;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace Flyga.AdditionalAchievements.Solve.Handler
{
    /// <inheritdoc/>
    public interface IActionHandler<TAction> : IActionHandler
        where TAction : IAction
    {
        /// <inheritdoc cref="IActionHandler.TryRegisterAction(IAction)"/>
        bool TryRegisterAction(TAction action);

        /// <inheritdoc cref="IActionHandler.TryUnregisterAction(IAction)"/>
        bool TryUnregisterAction(TAction action);

        /// <inheritdoc cref="IActionHandler.TryRegisterActions(IEnumerable{IAction}, out IAction[])"/>
        bool TryRegisterActions(IEnumerable<TAction> actions, out TAction[] failedActions);

        /// <inheritdoc cref="IActionHandler.TryUnregisterActions(IEnumerable{IAction})"/>
        bool TryUnregisterActions(IEnumerable<TAction> actions);
    }

    /// <summary>
    /// Represents a class, that handles the fulfillment of an <see cref="IAction"/>.
    /// </summary>
    public interface IActionHandler : IDisposable
    {
        /// <summary>
        /// Fires, when the <see cref="State"/> changes.
        /// </summary>
        event EventHandler<HandlerState> StateChanged;

        /// <summary>
        /// The current state of the <see cref="IActionHandler"/>.
        /// </summary>
        HandlerState State { get; }

        /// <summary>
        /// The <see cref="IAction">IActions</see> that are registered with the <see cref="IActionHandler"/>.
        /// </summary>
        IEnumerable<IAction> Actions { get; }

        /// <summary>
        /// Fires, when the <see cref="State"/> changes to fatal. Carries the registered 
        /// <see cref="IAction">IActions</see> at the time of the crash.
        /// </summary>
        event EventHandler<IEnumerable<IAction>> Fatal;

        /// <summary>
        /// Determines whether the <see cref="IActionHandler"/> can handle the given <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>True, if the <paramref name="action"/> can be handled by the <see cref="IActionHandler"/>. 
        /// Otherwise false.</returns>
        bool CanHandle(IAction action);

        /// <summary>
        /// Attempts to register the <paramref name="action"/> with the <see cref="IActionHandler"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>True, if the <paramref name="action"/> was successfully registered. Otherwise false.</returns>
        bool TryRegisterAction(IAction action);

        /// <summary>
        /// Attempts to remove the <paramref name="action"/> from the <see cref="IActionHandler"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>True, if the <paramref name="action"/> was successfully removed. Otherwise false.</returns>
        bool TryUnregisterAction(IAction action);

        /// <summary>
        /// Attempts to register the <paramref name="actions"/> with the <see cref="IActionHandler"/>.
        /// </summary>
        /// <remarks>
        /// Continues to attempt to register every <see cref="IAction"/>, even if one fails.
        /// </remarks>
        /// <param name="actions"></param>
        /// <param name="failedActions"></param>
        /// <returns><see langword="true"/>, if each one of the <paramref name="actions"/> was successfully 
        /// registered. Otherwise <see langword="false"/>.</returns>
        bool TryRegisterActions(IEnumerable<IAction> actions, out IAction[] failedActions);

        /// <summary>
        /// Attempts to remove the <paramref name="actions"/> from the <see cref="IActionHandler"/>.
        /// </summary>
        /// <remarks>
        /// Continues to attempt to remove every <see cref="IAction"/>, even if one fails.
        /// </remarks>
        /// <param name="actions"></param>
        /// <returns><see langword="true"/>, if each one of the <paramref name="actions"/> was successfully 
        /// removed. Otherwise <see langword="false"/>.</returns>
        bool TryUnregisterActions(IEnumerable<IAction> actions);

        /// <summary>
        /// Attempts to remove the <see cref="IAction"/>s of the <paramref name="pack"/> from 
        /// the <see cref="IActionHandler"/>.
        /// </summary>
        /// <remarks>
        /// Continues to attempt to remove every <see cref="IAction"/>, even if one fails.
        /// </remarks>
        /// <param name="pack"></param>
        /// <returns><see langword="true"/>, if each one of the <see cref="IAction"/>s was successfully 
        /// removed. Otherwise <see langword="false"/>.</returns>
        bool TryUnregisterActions(IAchievementPackManager pack);

        /// <summary>
        /// Updates the <see cref="IActionHandler"/>.
        /// </summary>
        /// <param name="gameTime"></param>
        void Update(GameTime gameTime);
    }
}
