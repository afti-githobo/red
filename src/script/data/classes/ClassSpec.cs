using System.Collections.Generic;
using Godot;
using System.Collections.ObjectModel;
using System;
using CsvHelper;
using System.IO;
using System.Globalization;
using Red.Data.Map;
using Red.Data.Items;
using System.Security.AccessControl;

namespace Red.Data.Classes
{
    public readonly struct ClassSpec
    {
        public static ClassSpec Of(ClassID cls) => Table[(int)cls - 1];
        public static IReadOnlyList<ClassSpec> Table => _table.Value;
        private static readonly Lazy<IReadOnlyList<ClassSpec>> _table = new Lazy<IReadOnlyList<ClassSpec>>(LoadAll);

        public readonly string AssetID;
        public readonly string Name;
        public readonly int BaseHP;
        public readonly int BaseStrength;
        public readonly int BaseDefense;
        public readonly int BaseSpeed;
        public readonly int BaseLuck;
        public readonly int BaseMove;
        public readonly int BaseConstitution;
        public readonly float MoveSpeed;
        public readonly ActionFlags ActionFlags;
        public readonly ItemFlags ItemFlags;
        public readonly IReadOnlyList<int> MovementCosts;

        public ClassSpec(string assetID, string name, int baseHP, int baseStrength, int baseDefense, int baseSpeed, int baseLuck, int baseMove, 
            int baseConstitution, float moveSpeed, ActionFlags actionFlags, ItemFlags itemFlags, IReadOnlyList<int> movementCosts)
        {
            AssetID = assetID;
            Name = name;
            BaseHP = baseHP;
            BaseStrength = baseStrength;
            BaseDefense = baseDefense;
            BaseSpeed = baseSpeed;
            BaseLuck = baseLuck;
            BaseMove = baseMove;
            BaseConstitution = baseConstitution;
            MoveSpeed = moveSpeed;
            ActionFlags = actionFlags;
            ItemFlags = itemFlags;
            MovementCosts = movementCosts;
        }

        private static IReadOnlyList<ClassSpec> LoadAll()
        {
            var csv = Godot.FileAccess.GetFileAsString(ProjectConstants.ClassTablePath);
            var table = new ClassSpec[(int)ClassID.Length - 1];
            using (var stringReader = new StringReader(csv))
            using (var reader = new CsvReader(stringReader, CultureInfo.InvariantCulture)) {
                GD.Print("Loading class data table...");
                var id = ClassID.None + 1;
                try
                {
                    reader.Read();
                    reader.ReadHeader();
                    while (reader.Read())
                    {
                        var _id = Enum.Parse<ClassID>(reader.GetField<string>("ID"));
                        if (_id != id) throw new Exception($"Expected ID {id} but got {_id}");
                        var assetId = reader.GetField<string>("AssetID");
                        var name = reader.GetField<string>("Name");
                        var baseHP = reader.GetField<int>("BaseHP");
                        var baseStrength = reader.GetField<int>("BaseStrength");
                        var baseDefense = reader.GetField<int>("BaseDefense");
                        var baseSpeed = reader.GetField<int>("BaseSpeed");
                        var baseLuck = reader.GetField<int>("BaseLuck");
                        var baseMove = reader.GetField<int>("BaseMove");
                        var baseCon = reader.GetField<int>("BaseConstitution");
                        var moveSpeed = reader.GetField<float>("MoveSpeed");
                        var flagsStr = reader.GetField<string>("ActionFlags");
                        var splut = flagsStr.Split(',');
                        var actionFlags = ActionFlags.None;
                        for (int i = 0; i < splut.Length; i++) actionFlags |= Enum.Parse<ActionFlags>(splut[i]);
                        flagsStr = reader.GetField<string>("ItemFlags");
                        splut = flagsStr.Split(',');
                        var itemFlags = ItemFlags.None;
                        for (int i = 0; i < splut.Length; i++) itemFlags |= Enum.Parse<ItemFlags>(splut[i]);
                        var movementCosts = new int[(int)TileAttribute.Length];
                        var movementCostsBase = reader.GetFieldIndex("MovementCosts");
                        for (int i = 0; i < movementCosts.Length; i++)
                        {
                            var mod = reader.GetField<float>(movementCostsBase + i);
                            if (mod == float.PositiveInfinity) movementCosts[i] = int.MaxValue;
                            else movementCosts[i] = Mathf.RoundToInt(mod * ProjectConstants.BaseMovementCost);
                        }
                        table[(int)id - 1] = new ClassSpec(assetId, name, baseHP, baseStrength, baseDefense, baseSpeed, baseLuck, baseMove, baseCon, moveSpeed, 
                            actionFlags, itemFlags,new ReadOnlyCollection<int>(movementCosts));
                        id++;
                    }
                    GD.Print("Loaded class data table successfully!");
                } catch(Exception e)
                {
                    GD.PrintErr($"Error during class table parsing for row {id}: {e.Message}");
                    throw;
                }
            }
            return new ReadOnlyCollection<ClassSpec>(table);
        }
    }
}