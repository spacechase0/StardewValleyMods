using System.Diagnostics.CodeAnalysis;
using SpaceShared;

namespace SpaceCore.Events
{
    public class EventArgsServerGotClient
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.IsPublicApi)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.IsPublicApi)]
        public long FarmerID { get; set; }
    }
}
