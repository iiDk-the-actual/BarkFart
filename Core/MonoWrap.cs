using System;
using Photon.Pun;

namespace BarkFart.Core;

public class MonoWrap : MonoBehaviourPunCallbacks
{
    protected MonoWrap()
    {
    }

    protected MonoWrap(IntPtr handle) : base(handle)
    {
    }
}