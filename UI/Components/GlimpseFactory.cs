using LiveSplit.Model;
using System;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(GlimpseFactory))]

namespace LiveSplit.UI.Components
{
    public class GlimpseFactory : IComponentFactory
    {
        public string ComponentName => "Glimpse";

        public string Description => "Allows users to send speedrun data and events to Glimpse for viewers to consume.";

        public ComponentCategory Category => ComponentCategory.Other;

        public IComponent Create(LiveSplitState state) => new GlimpseComponent(state);

        public string UpdateName => ComponentName;

        public string UpdateURL => ""; //url to download updates

        public Version Version => Version.Parse("1.0.0");

        public string XMLURL => ""; //xml for updating
    }
}
