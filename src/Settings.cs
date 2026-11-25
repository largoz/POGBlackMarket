using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using POGLib.Shops.Model;

namespace POGBlackMarket
{
    /// <summary>The host's settings (MelonPreferences.cfg; POG Config's page only reads and writes them). Read when the game starts.</summary>
    internal static class Settings
    {
        internal static readonly string[] AllStalls = { "Antique", "Curiosa", "Foodlover", "MysticTent", "Smith", "Tinkerer" };
        internal const string DefaultStalls = "Curiosa,Tinkerer,MysticTent,Antique";

        private static MelonPreferences_Category _category;
        private static MelonPreferences_Entry<string> _stalls, _sells;

        internal sealed class LookWeight { public string Id, Label; public float Standard; public MelonPreferences_Entry<float> Entry; }
        internal static readonly LookWeight[] Looks =
        {
            new LookWeight { Id = "plain", Label = "Coat and hat", Standard = 5 },
            new LookWeight { Id = "mask", Label = "In a thief's mask", Standard = 3 },
            new LookWeight { Id = "bandana", Label = "Bandana over the mouth", Standard = 2 },
        };

        internal static void Init()
        {
            _category = MelonPreferences.CreateCategory("POGBlackMarket", "Black Market");
            foreach (var l in Looks)
                l.Entry = _category.CreateEntry("Weight" + char.ToUpperInvariant(l.Id[0]) + l.Id.Substring(1), l.Standard, l.Label, "Host: how often this look comes up, against the other two.");
            _stalls = _category.CreateEntry("Stalls", DefaultStalls, "Stalls", "Host: whose stalls he takes: " + string.Join(", ", AllStalls) + ".");
            // Empty by default and not the list itself: a saved list would keep a later version's goods away.
            _sells = _category.CreateEntry("Sells", "", "What he sells",
                "Host: empty for his own goods. Your own with commas between: item:weight; item*:weight does not run out; item+other:weight always stands with the other one, " +
                "which is in the list too. A mod's item is Mod/Item.");
        }

        /// <summary>The host's own list of goods; null when there is none.</summary>
        internal static List<PoolEntry> OwnGoods => string.IsNullOrWhiteSpace(_sells.Value) ? null : Goods.Parse(_sells.Value);

        internal static List<string> Stalls
        {
            get
            {
                var named = (_stalls.Value ?? "").Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).Distinct().ToList();
                foreach (var x in named.Where(x => !AllStalls.Contains(x))) MelonLogger.Warning($"[BlackMarket] Stalls: no stall '{x}'; there are {string.Join(", ", AllStalls)}.");
                return named.Where(x => AllStalls.Contains(x)).ToList();
            }
        }

        internal static bool Takes(string stall) => (_stalls.Value ?? "").Split(',').Any(x => x.Trim() == stall);

        internal static void SetTakes(string stall, bool on)
        {
            if (Takes(stall) == on) return;
            _stalls.Value = string.Join(",", AllStalls.Where(x => x == stall ? on : Takes(x)));
            Save();
        }

        internal static void SetWeight(LookWeight look, float weight)
        {
            if (look.Entry.Value == weight) return;
            look.Entry.Value = weight;
            Save();
        }

        private static void Save() => _category.SaveToFile(false);
    }
}
