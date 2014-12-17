namespace NServiceBus.Routing
{
    using System;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    /// <summary>
    /// This class provides implementers of <see cref="IProvideDynamicRouting"/> with an extension mechanism for custom settings via extension methods.
    /// </summary>
    /// <typeparam name="T">The client distributor definition eg <see cref="FileBasedRoundRobinDistribution"/>.</typeparam>
    public class RoutingExtensions<T> : ExposeSettings where T : DynamicRoutingDefinition
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public RoutingExtensions(SettingsHolder settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Overrides the default logical address translator to the parameter that is passed to <see cref="IProvideDynamicRouting.TryGetRouteAddress"/>.
        /// </summary>
        /// <remarks>
        /// The default translator uses the <see cref="Address.ToString()"/>.
        /// </remarks>
        /// <param name="translateToLogicalAddress">The callback to do the translation.</param>
        public RoutingExtensions<T> WithTranslator(Func<Address, string> translateToLogicalAddress)
        {
            Settings.Set("Routing.Translator", new CustomTranslator(translateToLogicalAddress));
            return this;
        }

        class CustomTranslator : AddressTranslator
        {
            readonly Func<Address, string> translateToLogicalAddress;

            public CustomTranslator(Func<Address, string> translateToLogicalAddress)
            {
                this.translateToLogicalAddress = translateToLogicalAddress;
            }

            public override string Translate(Address address)
            {
                return translateToLogicalAddress(address);
            }
        }
    }
}