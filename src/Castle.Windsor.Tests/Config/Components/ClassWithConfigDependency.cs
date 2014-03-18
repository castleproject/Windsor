namespace Castle.MicroKernel.Tests.Configuration.Components
{
    using System.Linq;

    /// <summary>
    /// The ClassWithConfigDependency interface.
    /// </summary>
    public interface IClassWithConfigDependency
    {
        /// <summary>
        /// The get name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetName();

        /// <summary>
        /// The get server ip.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string GetServerIp(string name);
    }

    /// <summary>
    /// The class with config dependency.
    /// </summary>
    public class ClassWithConfigDependency : IClassWithConfigDependency
    {
        /// <summary>
        /// The _config.
        /// </summary>
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassWithConfigDependency"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        public ClassWithConfigDependency(IConfig config)
        {
            this._config = config;
        }

        /// <summary>
        /// The get name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetName()
        {
            return this._config.Name;
        }

        /// <summary>
        /// The get server ip.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetServerIp(string name)
        {
            return this._config.Servers.First(s => s.Name == name).Ip;
        }
    }
}