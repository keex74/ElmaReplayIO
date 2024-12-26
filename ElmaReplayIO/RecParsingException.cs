namespace ElmaReplayIO
{
    /// <summary>
    /// Defines an exception raised while parsing a replay file.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RecParsingException"/> class.
    /// </remarks>
    /// <param name="message">The exception message.</param>
    public class RecParsingException(string message)
                : Exception(message)
    {
    }
}
