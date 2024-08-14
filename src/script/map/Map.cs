using Godot;
using Red.Data.Map;

namespace Red.MapScene
{
	/// <summary>
	/// Map container class.
	/// 
	/// Provides other nodes with easy access to music, tile attributes, etc.
	/// Also handles map scripting.
	/// </summary>
	[GlobalClass]
	public partial class Map : Node2D
	{
        const int tileAttributesToCachePerProcessPump = 20;

        private static StringName attributeName = "TileAttribute";
		private static StringName subAttributeName = "TileSubAttribute";
		[Export]
		private Vector2I topLeft;
		[Export]
		private Vector2I bottomRight;
        /// <summary>
        /// Tilemap reference. Used to grab tile attribute data.
        /// </summary>
        [Export]
		private TileMap tileMap;
		/// <summary>
		/// Initial BGM for the map. This may be overridden by map scripting.
		/// </summary>
		[Export]
		private AudioStream defaultBGM;
		/// <summary>
		/// Script-specified override BGM for the map, if any.
		/// </summary>
		private AudioStream overrideBGM;

		/// <summary>
		/// Getting custom data for tiles is *slow* and trying to grab
		/// </summary>
		private (TileAttribute, TileSubAttribute)[][] tileAttributeCache;
		private int cacheProgressX;
		private int cacheProgressY;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			tileAttributeCache = new (TileAttribute, TileSubAttribute)[bottomRight.Y - topLeft.Y][];
			for (int y = 0; y < tileAttributeCache.Length; y++) tileAttributeCache[y] = new (TileAttribute, TileSubAttribute)[bottomRight.X - topLeft.X];
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{

			for (int i = 0; i < tileAttributesToCachePerProcessPump; i++)
			{
				if (cacheProgressX == tileAttributeCache[cacheProgressY].Length)
				{
					cacheProgressX = 0;
					cacheProgressY++;
                    if (cacheProgressY == tileAttributeCache.Length)
                    {
                        ProcessMode = ProcessModeEnum.Disabled; // we're done here!
						break;
                    }
                }
				tileAttributeCache[cacheProgressY][cacheProgressX] = _GetTileAttributesSlow(topLeft.X + cacheProgressX, topLeft.Y + cacheProgressY);
				cacheProgressX++;
			}
		}

		public bool IsInBounds(int x, int y)
		{
			return (x >= topLeft.X && x < bottomRight.X && y >= topLeft.Y && y < bottomRight.Y);
		}

		/// <summary>
		/// Returns attribute and sub-attribute values for a given tile on the current map.
		/// </summary>
		/// <param name="x">X-coordinate of the tile to check</param>
		/// <param name="y">Y-coordinate of the tile to check</param>
		/// <returns></returns>
		public (TileAttribute, TileSubAttribute) GetTileAttributes (int x, int y)
		{
			if (y - topLeft.Y < cacheProgressY) return tileAttributeCache[y - topLeft.Y][x - topLeft.X]; // fast path
			else return _GetTileAttributesSlow(x, y); // slow path
		}

		private (TileAttribute, TileSubAttribute) _GetTileAttributesSlow(int x, int y)
		{
            var dat = tileMap.GetCellTileData(-1, new Vector2I(x, y));
            int attr = (int)(dat?.GetCustomData(attributeName) ?? 0);
            int subAttr = (int)(dat?.GetCustomData(subAttributeName) ?? 0);
            return ((TileAttribute)attr, (TileSubAttribute)subAttr);
        }
	}

}
