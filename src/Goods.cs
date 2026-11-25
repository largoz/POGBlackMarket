using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MelonLoader;
using POGLib.Shops.Model;

namespace POGBlackMarket
{
    /// <summary>
    /// What he sells: human things, the weapons and the books of the old human mages. Written as goods with commas between:
    /// <c>item:weight</c>, <c>item*:weight</c> (does not run out), <c>item+other:weight</c> (the other one always stands with it).
    /// A mod's item is <c>Mod/Item</c>.
    /// </summary>
    internal static class Goods
    {
        internal const string Default =
            "Item_Crossbow+Item_CrossbowBolt:4, Item_CrossbowBolt*:1, Item_Weapon_Dagger:3, Item_Weapon_Ward:2, " +
            "Item_MagicTome_Book_Ghost:1, Item_MagicTome_Book_Necromancy:1, Item_MagicTome_Book_Greed:1, Item_MagicTome_Book_Rat:1, " +
            "Item_MagicTome_Book_ChaosFire:1, Item_MagicTome_Book_Void:1, Item_MagicTome_Book_Berserking:1, " +
            "Item_MagicTome_Book_Engineering:1, Item_MagicTome_Book_MushroomHead:1";

        internal static List<PoolEntry> Parse(string list)
        {
            var goods = new List<PoolEntry>();
            foreach (var part in (list ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => p.Length > 0))
            {
                var bits = part.Split(':');
                double w = 1;
                var names = bits[0].Split('+').Select(n => n.Trim()).ToList();
                bool endless = names[0].EndsWith("*", StringComparison.Ordinal);
                if (endless) names[0] = names[0].TrimEnd('*').Trim();
                if (names.Any(n => n.Length == 0) || bits.Length > 2 || (bits.Length == 2 && !double.TryParse(bits[1], NumberStyles.Float, CultureInfo.InvariantCulture, out w)) || !(w >= 0))
                { MelonLogger.Warning($"[BlackMarket] Goods: '{part}' is not item:weight; left out."); continue; }
                goods.Add(new PoolEntry { Item = Id(names[0]), Weight = w, Endless = endless, With = names.Skip(1).Select(Id).ToList() });
            }
            return goods;
        }

        private static string Id(string name) => name.Contains("/") ? name.Replace('/', ':') : "vanilla:" + name;
    }
}
