namespace Munters.Giphy.Api.Giphy;

public class GiphyUpstreamException : Exception
{
    public GiphyUpstreamException(string message) : base(message) { }
    public GiphyUpstreamException(string message, Exception inner) : base(message, inner) { }
}

public sealed class GiphyUpstreamTimeoutException : GiphyUpstreamException
{
    public GiphyUpstreamTimeoutException(string message) : base(message) { }
    public GiphyUpstreamTimeoutException(string message, Exception inner) : base(message, inner) { }
}
