using System;
using Microsoft.Extensions.Options;

namespace Http.Options
{
    /// <summary>
    /// Implementation of <see cref="IConfigureNamedOptions{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type being configured.</typeparam>
    internal class ConfigureOptionsAction<TOptions> : IConfigureNamedOptions<TOptions> where TOptions : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">The action to register.</param>
        public ConfigureOptionsAction(Action<string, TOptions> action)
        {
            Action = action;
        }


        /// <summary>
        /// The configuration action.
        /// </summary>
        public Action<string, TOptions> Action { get; }

        /// <summary>
        /// Invokes the registered configure <see cref="Action"/>.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configure.</param>
        public virtual void Configure(string name, TOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }


            Action?.Invoke(name, options);
        }

        /// <summary>
        /// Invoked to configure a <typeparamref name="TOptions"/> instance with the <see cref="Options.DefaultName"/>.
        /// </summary>
        /// <param name="options">The options instance to configure.</param>
        public void Configure(TOptions options) => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
    }
}