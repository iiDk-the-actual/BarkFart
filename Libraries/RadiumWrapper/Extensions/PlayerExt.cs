using Photon.Pun;

namespace BarkFart.Libraries.RadiumWrapper.Extensions;

public static class PlayerExt
{
    public static bool IsLocal(this Player player) =>
        player.owner == PhotonNetwork.LocalPlayer;
}