using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollectionCreator
{
    internal static class AppOptions
    {
        public const string ResourceGroup = "rg";
        public const string SubscriptionId = "subid";
        public const string Action = "action";
    }

    internal static class AppOptionsExtensions
    {
        public static CommandOption ResourceGroupOption(this CommandLineApplication app)
        {
            return app.Options.First(o => o.ShortName == AppOptions.ResourceGroup);
        }
        public static CommandOption SubscriptionIdOption(this CommandLineApplication app)
        {
            return app.Options.First(o => o.ShortName == AppOptions.SubscriptionId);
        }

        public static CommandOption ActionOption(this CommandLineApplication app)
        {
            return app.Options.First(o => o.ShortName == AppOptions.Action);
        }
    }
}
