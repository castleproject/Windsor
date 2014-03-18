namespace Castle.MicroKernel.Tests.Configuration.Components
{
    using Castle.MicroKernel.SubSystems.Conversion;

    /// <summary>
    /// The Config interface.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the servers.
        /// </summary>
        Server[] Servers { get; set; }
    }

    /// <summary>
    /// The config.
    /// </summary>
    public class Config : IConfig
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the servers.
        /// </summary>
        public Server[] Servers { get; set; }
    }

    /// <summary>
    /// The server.
    /// </summary>
    [Convertible]
    public class Server
    {
        /// <summary>
        /// Gets or sets the ip.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
    }
}