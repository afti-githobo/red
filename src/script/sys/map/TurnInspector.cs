using Godot;
using Red.MapScene;

namespace Red.Sys.MapScene
{
    [GlobalClass]
    public partial class TurnInspector : Control
    {
        private RichTextLabel label;

        public override void _Ready()
        {
            label = GetNode<RichTextLabel>("Label");
            ISingleton<TurnManager>.Instance.ChangedPhase += ChangedPhase;
        }

        private void ChangedPhase(int turnPhase, int turnCount)
        {
            label.Text = $"Turn {turnCount + 1}\n{((TurnPhase)turnPhase).PrimaryPhase()}";
        }
    }
}