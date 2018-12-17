using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Mined.WPF.Controls
{
    [TemplatePart(Name = "PART_AddressTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_SuggestionList", Type = typeof(ListBox))]
    public class AddressBox : ContentControl
    {
        protected TextBox textbox;
        protected ListBox suggestionsList;
        protected Popup popup;

        private IAddressAutoCompletionService addressAutoCompletionService;

        protected List<string> candidates;
        private Dictionary<string, string> AddressPlaceIds;





        public static readonly RoutedEvent InitializingAddressAutoCompletionServiceEvent = EventManager.RegisterRoutedEvent(
       "InitializingAddressAutoCompletionService", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AddressBox));

        public event RoutedEventHandler InitializingAddressAutoCompletionService
        {
            add { AddHandler(InitializingAddressAutoCompletionServiceEvent, value); }
            remove { RemoveHandler(InitializingAddressAutoCompletionServiceEvent, value); }
        }


        public static readonly DependencyProperty AddressTextProperty = DependencyProperty.Register(
            "AddressText", typeof(string), typeof(AddressBox), new PropertyMetadata(default(string)));

        public string AddressText
        {
            get { return (string)GetValue(AddressTextProperty); }
            set { SetValue(AddressTextProperty, value); }
        }

        static AddressBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AddressBox), new FrameworkPropertyMetadata(typeof(AddressBox)));
        }
        public AddressBox()
        {
            
            if(AutoCompleteCheckInterval == 0)
            {
                AutoCompleteCheckInterval = 500;
            }
            cts = new CancellationTokenSource();
            System.Timers.Timer timer = new System.Timers.Timer(AutoCompleteCheckInterval);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            candidates = new List<string>();
            Focusable = false;
        }

        public int AutoCompleteCheckInterval
        {
            get { return (int)GetValue(AutoCompleteCheckIntervalProperty); }
            set { SetValue(AutoCompleteCheckIntervalProperty, value); }
        }

        public static readonly DependencyProperty AutoCompleteCheckIntervalProperty =
            DependencyProperty.Register("AutoCompleteCheckInterval", typeof(int), typeof(AddressBox), new PropertyMetadata(0));


        private void ClearSuggestions()
        {

            candidates = new List<string>();
            suggestionsList.Items.Clear();
            popup.IsOpen = false;

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            textbox = (TextBox)GetTemplateChild("PART_AddressTextBox");
            suggestionsList = (ListBox)GetTemplateChild("PART_SuggestionList");

            popup = (Popup)GetTemplateChild("PART_Popup");

            if (popup != null)
            {
                
                popup.IsOpen = false;
            }
            OnInitializingAddressAutoCompletionService();
            

            if (addressAutoCompletionService == null)
            {
                return;
            }

            textbox.PreviewKeyDown += TextBox_PreviewKeyDown;
            textbox.LostKeyboardFocus += TextBox_LostKeyboardFocus;
            textbox.TextChanged += Textbox_TextChanged;
            suggestionsList.MouseUp += SuggestionsList_MouseUp;

        }

        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            popup.Width = textbox.ActualWidth;
        }

        private async void SuggestionsList_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (suggestionsList != null)
            {
                await MakeSelection();
            }
        }

        private void TextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (textbox != null && suggestionsList != null)
            {
                if (suggestionsList.IsKeyboardFocusWithin)
                {
                    return;
                }
            }
            ClearSuggestions();
        }

        private async void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (candidates.Count == 0)
            {
                e.Handled = false;
                return;
            }
            switch (e.Key)
            {
                case Key.Down:
                case Key.Up:
                case Key.Tab:
                    bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                    int step = e.Key == Key.Down || (e.Key == Key.Tab && !shiftPressed) ? 1 : -1;
                    ChangeHighlitedSuggestion(step);
                    if (candidates.Count > 0)
                    {
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Tab)
                    {
                        e.Handled = false;
                    }
                    break;
                case Key.Enter:
                    var textbox = e.Source as TextBox;
                    await MakeSelection();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    if (candidates.Count == 0)
                    {
                        e.Handled = false;
                    }
                    else
                    {
                        ClearSuggestions();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private readonly object timerLock = new object();
        private bool skipSearch = false;

        private string previousSearchText = null;
        private CancellationTokenSource cts;

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (addressAutoCompletionService == null || textbox == null)
                return;
            bool searchTextChanged = false;

            lock (timerLock)
            {
                if (!skipSearch)
                {
                    Dispatcher.Invoke(() =>
                    {
                        searchTextChanged =
                            !textbox.Text?.Equals(previousSearchText, StringComparison.OrdinalIgnoreCase) ?? false;
                        previousSearchText = textbox.Text;
                    });
                }
                else
                {
                    skipSearch = false;
                    Dispatcher.Invoke(() => previousSearchText = textbox.Text);

                    return;
                }
            }

            if (searchTextChanged)
            {

                cts.Cancel();
                cts.Dispose();
                cts = new CancellationTokenSource();
                await Dispatcher.Invoke(async () => await SearchExecute());

            }
        }





        private async Task SearchExecute()
        {
            if (textbox != null && textbox.Text.Length < 4)
                return;
            if (addressAutoCompletionService == null)
            {
                throw new Exception("Address auto-completion service not initialized.");
            }
            try
            {
                var result = await addressAutoCompletionService.AutoCompleteAddressAsync(textbox.Text, cts.Token);
                if (result != null && result.Predictions.Count > 0)
                {
                    AddressPlaceIds = result.Predictions.ToDictionary(x => x.Description, x => x.Place_Id);
                    candidates = new List<string>(result.Predictions.Select(p => p.Description));
                    suggestionsList.Items.Clear();
                    candidates.ForEach(c => suggestionsList.Items.Add(c));
                    popup.IsOpen = true;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }


        private void ChangeHighlitedSuggestion(int step)
        {
            if (candidates.Count > 0)
            {
                int newIndex = (suggestionsList.SelectedIndex + step) % candidates.Count;
                suggestionsList.SelectedIndex = newIndex;
            }
        }

        private async Task MakeSelection()
        {
            var fullText = await GetFullAddressString();

            lock (timerLock)
            {
                skipSearch = true;
                textbox.Text = fullText;
                AddressText = fullText;
                ClearSuggestions();

                if (textbox != null)
                {
                    textbox.SelectAll();
                }
            }
        }

        private async Task<string> GetFullAddressString()
        {
            try
            {
                if (suggestionsList.SelectedIndex >= 0)
                {
                    string placeId = AddressPlaceIds[(string)suggestionsList.SelectedItem];

                    PlaceDetailsResult placeDetailsResult = await addressAutoCompletionService.GetPlaceDetailsAsync(placeId, cts.Token);

                    Address address = new Address();
                    if (placeDetailsResult.Status == "OK" && placeDetailsResult.Result?.Address_Components != null &&
                        placeDetailsResult.Result.Address_Components.Any())
                    {
                        foreach (PlaceDetailsResult.AddressComponent addressComponent in placeDetailsResult.Result
                            .Address_Components)
                        {
                            if (addressComponent.Types.Any(x => x == "street_number"))
                            {
                                address.StreetNo = addressComponent.Short_Name;
                            }

                            else if (addressComponent.Types.Any(x => x == "route"))
                            {
                                address.StreetName = addressComponent.Short_Name;
                            }

                            else if (addressComponent.Types.Any(x => x == "locality"))
                            {
                                address.Locality = addressComponent.Short_Name;
                            }

                            else if (addressComponent.Types.Any(x => x == "administrative_area_level_1"))
                            {
                                address.StateOrTerritory = addressComponent.Short_Name;
                            }

                            else if (addressComponent.Types.Any(x => x == "country"))
                            {
                                address.Country = addressComponent.Long_Name;
                            }

                            else if (addressComponent.Types.Any(x => x == "postal_code"))
                            {
                                address.PostCode = addressComponent.Short_Name;
                            }
                        }
                    }

                    return address.ToString();
                }
            }
            catch (OperationCanceledException)
            {

            }
            return null;
        }


        protected virtual void OnInitializingAddressAutoCompletionService()
        {
            RaiseInitializingAddressAutoCompleteService();
        }

        private void RaiseInitializingAddressAutoCompleteService()
        {
            InitializingAddressAutoCompletionServiceEventArgs args = new InitializingAddressAutoCompletionServiceEventArgs(InitializingAddressAutoCompletionServiceEvent);
            RaiseEvent(args);
            addressAutoCompletionService = args.AddressAutoCompletionService;
        }
    }
}