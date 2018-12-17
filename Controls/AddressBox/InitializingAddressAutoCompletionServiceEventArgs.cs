using System.Windows;

namespace Mined.WPF.Controls
{
    public class InitializingAddressAutoCompletionServiceEventArgs : RoutedEventArgs
    {
        public InitializingAddressAutoCompletionServiceEventArgs()
        {
        }

        public InitializingAddressAutoCompletionServiceEventArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }

        public InitializingAddressAutoCompletionServiceEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
        }

        public IAddressAutoCompletionService AddressAutoCompletionService { get; set; }

    }
}