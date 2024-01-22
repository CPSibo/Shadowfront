using Godot;
using System;

namespace Shadowfront.Backend
{
    public partial class DisposableRefCounted : RefCounted, IDisposable
    {
        [Signal]
        public delegate void DisposingEventHandler(DisposableRefCounted sender);

        protected override void Dispose(bool disposing)
        {
            EmitSignal(SignalName.Disposing, this);

            base.Dispose(disposing);
        }
    }
}
