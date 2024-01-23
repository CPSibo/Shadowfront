using Godot;

namespace Shadowfront.Backend
{
    public partial class DisposableNode2D : Node2D
    {
        [Signal]
        public delegate void DisposingEventHandler(DisposableNode sender);

        protected override void Dispose(bool disposing)
        {
            EmitSignal(SignalName.Disposing, this);

            base.Dispose(disposing);
        }
    }
}
