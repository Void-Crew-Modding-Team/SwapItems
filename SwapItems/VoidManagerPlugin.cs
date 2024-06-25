using VoidManager.MPModChecks;

namespace SwapItems
{
    public class VoidManagerPlugin : VoidManager.VoidPlugin
    {
        public override MultiplayerType MPType => MultiplayerType.Client;

        public override string Author => "18107";

        public override string Description => "Allows the user to pick up and place items at the same time";
    }
}
