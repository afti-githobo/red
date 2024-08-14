using System.Collections.Generic;
using System;
using Red.Data.Units;
using CsvHelper;
using Godot;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Red.Data.Classes;

namespace Red.Data.Items
{
    public readonly struct ItemSpec
    {
        public static ItemSpec Of(ItemID id) => Table[(int)id - 1];
        public static IReadOnlyList<ItemSpec> Table => _table.Value;
        private static readonly Lazy<IReadOnlyList<ItemSpec>> _table = new Lazy<IReadOnlyList<ItemSpec>>(LoadAll);

        public readonly string AssetID;
        public readonly string Name;
        public readonly ItemFlags ItemFlags;
        public readonly TargetFlags TargetFlags;
        public readonly int BaseDurability;
        public readonly int MinRange;
        public readonly int MaxRange;
        public readonly int Power;
        public readonly int Weight;
        public readonly int Value;

        public ItemSpec(string assetID, string name, ItemFlags itemFlags, TargetFlags targetFlags, int baseDurability, int minRange, int maxRange, int power, int weight, int value)
        {
            AssetID = assetID;
            Name = name;
            ItemFlags = itemFlags;
            TargetFlags = targetFlags;
            BaseDurability = baseDurability;
            MinRange = minRange;
            MaxRange = maxRange;
            Power = power;
            Weight = weight;
            Value = value;
        }

        private static IReadOnlyList<ItemSpec> LoadAll()
        {
            var csv = Godot.FileAccess.GetFileAsString(ProjectConstants.ItemTablePath);
            var table = new ItemSpec[(int)ItemID.Length - 1];
            using (var stringReader = new StringReader(csv))
            using (var reader = new CsvReader(stringReader, CultureInfo.InvariantCulture))
            {
                GD.Print("Loading item data table...");
                var id = ItemID.None + 1;
                try
                {
                    reader.Read();
                    reader.ReadHeader();
                    while (reader.Read())
                    {
                        var _id = Enum.Parse<ItemID>(reader.GetField<string>("ID"));
                        if (_id != id) throw new Exception($"Expected ID {id} but got {_id}");
                        var assetId = reader.GetField<string>("AssetID");
                        var name = reader.GetField<string>("Name");
                        var flagsStr = reader.GetField<string>("ItemFlags");
                        var splut = flagsStr.Split(',');
                        var itemFlags = ItemFlags.None;
                        for (int i = 0; i < splut.Length; i++) itemFlags |= Enum.Parse<ItemFlags>(splut[i]);
                        flagsStr = reader.GetField<string>("TargetFlags");
                        splut = flagsStr.Split(',');
                        var targetFlags = TargetFlags.None;
                        for (int i = 0; i < splut.Length; i++) targetFlags |= Enum.Parse<TargetFlags>(splut[i]);
                        var baseDurability = reader.GetField<int>("BaseDurability");
                        var minRange = reader.GetField<int>("MinRange");
                        var maxRange = reader.GetField<int>("MaxRange");
                        var power = reader.GetField<int>("Power");
                        var weight = reader.GetField<int>("Weight");
                        var value = reader.GetField<int>("Value");
                        table[(int)id - 1] = new ItemSpec(assetId, name, itemFlags, targetFlags, baseDurability, minRange, maxRange, power, weight, value);
                        id++;
                    }
                    GD.Print("Loaded item data table successfully!");
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error during item table parsing for row {id}: {e.Message}");
                    throw;
                }
            }
            return new ReadOnlyCollection<ItemSpec>(table);
        }
    }
}