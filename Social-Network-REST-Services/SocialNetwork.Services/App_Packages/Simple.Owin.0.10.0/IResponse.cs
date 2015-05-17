using System.IO;

namespace Simple.Owin
{
    /// <summary>
    /// Abstraction for an HTTP response, to be implemented by hosting.
    /// </summary>
    internal interface IResponse
    {
        Stream Body { get; }

        /// <summary>
        /// The response headers.
        /// </summary>
        IResponseHeaders Headers { get; }

        /// <summary>
        /// Gets or sets the status code and description.
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        Status Status { get; set; }
    }
}