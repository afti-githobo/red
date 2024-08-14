using Godot;

namespace Red.Data.Items
{
    [GlobalClass]
    public partial class Item : Resource
    {
        [Export]
        public ItemID ItemID { get; private set; }
        [Export]
        public int Durability { get; private set; }
    }
}