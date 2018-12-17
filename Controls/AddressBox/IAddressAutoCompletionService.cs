using System.Threading;
using System.Threading.Tasks;

namespace Mined.WPF.Controls
{
    public interface IAddressAutoCompletionService
    {
        Task<AddressAutoCompleteResult> AutoCompleteAddressAsync(string text, CancellationToken ct);
        Task<PlaceDetailsResult> GetPlaceDetailsAsync(string placeId, CancellationToken ct);
    }
}