
namespace AccountManager.Core.Models
{
    public sealed class ConfirmationRequest
    {
        public string RequestTitle { get; set; } = "Are you sure?";
        public string RequestMessage { get; set; } = string.Empty;
        public Action<bool>? Callback { get; set; } = delegate { };
    }
}
