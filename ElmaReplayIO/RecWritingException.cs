namespace ElmaReplayIO
{
    /// <summary>
    /// Defines an exception raised while writing a replay file.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RecWritingException"/> class.
    /// </remarks>
    /// <param name="message">The exception message.</param>
    public class RecWritingException(string message)
                : Exception(message)
    {
    }
}
