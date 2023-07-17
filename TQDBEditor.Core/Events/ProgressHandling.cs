using Prism.Events;

namespace TQDBEditor.Events
{
    /// <summary>
    /// Represents the progress state.
    /// </summary>
    public enum ProgressState
    {
        /// <summary>
        /// No progress.
        /// </summary>
        NoProgress,
        /// <summary>
        /// The progress is indeterminate.
        /// </summary>
        Indeterminate,
        /// <summary>
        /// Normal progress.
        /// </summary>
        Normal,
        /// <summary>
        /// An error occurred.
        /// </summary>
        Error,
        /// <summary>
        /// The operation is paused.
        /// </summary>
        Paused
    }

    public class MainProgressEvent : PubSubEvent<MainProgressEventPayload> { }

    public readonly struct MainProgressEventPayload
    {
        public ulong CurrentProgress { get; }
        public ulong MaxProgress { get; }
        public ProgressState? State { get; }

        /// <summary>
        /// Construct a new Payload for firing a main progress event.<br></br>
        /// If <paramref name="state"/> is null the state is assumed to be unchanged.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="state"></param>
        public MainProgressEventPayload(ulong current, ulong max = 100, ProgressState? state = null)
        {
            CurrentProgress = current;
            MaxProgress = max;
            State = state;
        }
    }
}
