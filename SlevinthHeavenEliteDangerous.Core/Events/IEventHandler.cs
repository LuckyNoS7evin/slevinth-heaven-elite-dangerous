namespace SlevinthHeavenEliteDangerous.Events;

/// <summary>
/// Interface for components that handle journal events.
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Handles a journal event.
    /// </summary>
    /// <param name="evt">The event to handle</param>
    void HandleEvent(EventBase evt);
}
