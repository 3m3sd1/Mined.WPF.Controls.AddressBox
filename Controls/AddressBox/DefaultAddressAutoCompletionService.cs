using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mined.WPF.Controls.Extensions;

namespace Mined.WPF.Controls
{
    public class DefaultAddressAutoCompletionService : IAddressAutoCompletionService
    {
        private readonly string addressTypesString;
        private readonly string componentsString;
        private readonly string apiKey;
        private readonly string fieldsString;

        protected static readonly string baseUrl = "https://maps.googleapis.com/maps/api/place/";
        protected static readonly string searchPlaceResource = "autocomplete/json";
        protected static readonly string placeDetailsResource = "details/json";
        protected static readonly string inputParameterKey = "input";
        protected static readonly string inputTypeParamKey = "inputtype";
        protected static readonly string typesParamString = "types";
        protected static readonly string componentsParamString = "components";
        protected static readonly string apiKeyParamKey = "key";
        protected static readonly string placeIdKey = "placeid";

        public DefaultAddressAutoCompletionService(string addressTypesString, string componentsString, string apiKey, string fieldsString = "address_component")
        {
            if (string.IsNullOrWhiteSpace(addressTypesString))
            {
                throw new ArgumentException("message", nameof(addressTypesString));
            }

            if (string.IsNullOrWhiteSpace(componentsString))
            {
                throw new ArgumentException("message", nameof(componentsString));
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("message", nameof(apiKey));
            }

            this.addressTypesString = addressTypesString;
            this.componentsString = componentsString;
            this.apiKey = apiKey;
            this.fieldsString = fieldsString;
        }

        private static void AppendQuery(UriBuilder baseUri, string key, string value)
        {
            string queryToAppend = $"{key}={value}";
            if (baseUri.Query != null && baseUri.Query.Length > 1)
                baseUri.Query = $"{baseUri.Query.Substring(1)}&{queryToAppend}";
            else
                baseUri.Query = queryToAppend;
        }

        public async Task<AddressAutoCompleteResult> AutoCompleteAddressAsync(string text, CancellationToken ct)
        {
            UriBuilder baseUri = new UriBuilder(baseUrl + searchPlaceResource);

            AppendQuery(baseUri, inputParameterKey, text);
            AppendQuery(baseUri, componentsParamString, componentsString);
            AppendQuery(baseUri, typesParamString, addressTypesString);
            AppendQuery(baseUri, apiKeyParamKey, apiKey);

            WebRequest request = WebRequest.Create(baseUri.ToString());
            request.Proxy = null;

            var response = await request.GetResponseAsync(ct);
            try
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseText = reader.ReadToEnd();
                AddressAutoCompleteResult result = JsonConvert.DeserializeObject<AddressAutoCompleteResult>(responseText);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PlaceDetailsResult> GetPlaceDetailsAsync(string placeId, CancellationToken ct)
        {
            var now = DateTime.Now;
            UriBuilder baseUri = new UriBuilder(baseUrl + placeDetailsResource);

            AppendQuery(baseUri, placeIdKey, placeId);
            AppendQuery(baseUri, typesParamString, fieldsString);
            AppendQuery(baseUri, apiKeyParamKey, apiKey);

            WebRequest request = WebRequest.Create(baseUri.ToString());
            request.Proxy = null;

            var response = await request.GetResponseAsync(ct);
            StreamReader reader = new StreamReader(response.GetResponseStream());

            string responseText = reader.ReadToEnd();
            PlaceDetailsResult results = JsonConvert.DeserializeObject<PlaceDetailsResult>(responseText);

            var start = now;
            now = DateTime.Now;
            return results;
        }
    }
}
