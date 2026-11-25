using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Il2CppEntities;
using MelonLoader;
using POGLib.Shops;
using POGLib.Shops.Model;

[assembly: MelonInfo(typeof(POGBlackMarket.BlackMarketMod), "Black Market", POGBlackMarket.BlackMarketMod.Version, "largo")]
[assembly: MelonGame(null, "Pit of Goblin")]
[assembly: MelonAdditionalDependencies("POGLib", "POGShops")]
[assembly: MelonOptionalDependencies("POGConfig")]
[assembly: HarmonyDontPatchAll]

namespace POGBlackMarket
{
    /// <summary>
    /// A goblin in a trench coat and a fedora sometimes keeps a dungeon merchant's stall instead of the merchant. POGShops
    /// decides whose the stall is, stocks it, names, dresses and arms the seller (this mod is one class of merchants, its looks are the
    /// merchants); here are the look itself (POGLib's cosmetics), the goods and how his clothes are found. The look travels with the game's own
    /// outfit data, so a player without the mod sees the same goblin without the coat and the hat.
    /// </summary>
    public sealed class BlackMarketMod : MelonMod
    {
        internal const string Id = "POGBlackMarket";
        internal const string Version = "0.1.0";

        public override void OnInitializeMelon()
        {
            Settings.Init();
            Outfit.Register(Path.GetDirectoryName(MelonAssembly.Location));
            HarmonyInstance.Patch(AccessTools.Method(typeof(NPCEntity), nameof(NPCEntity.OnNetworkPostSpawn)), postfix: new HarmonyMethod(typeof(BlackMarketMod), nameof(AfterNpc)));

            var stalls = Settings.Stalls;
            if (stalls.Count == 0) { MelonLogger.Msg("[BlackMarket] no stalls in the settings: he does not come when you host."); return; }
            var own = Settings.OwnGoods;
            bool added = own != null && Add(stalls, own);
            if (own != null && !added) MelonLogger.Warning("[BlackMarket] the list in \"Sells\" has a mistake (see above): he sells his own goods.");
            added = added || Add(stalls, Goods.Parse(Goods.Default));
            MelonLogger.Msg($"[BlackMarket] started; {(added ? "his merchants are with POGShops" : "POGShops did not take his merchants (see above)")}, stalls {string.Join(", ", stalls)}.");
        }

        private static bool Add(List<string> stalls, List<PoolEntry> goods) => ShopsApi.AddMerchants(new MerchantClass
        {
            ModId = Id,
            Title = "Black Market",
            Chance = 0.25,
            Merchants = Settings.Looks.Select((look, i) => new MerchantDef
            {
                Id = look.Id,
                Name = "Nobody",
                Weight = Math.Max(0, look.Entry.Value),
                Stalls = stalls.ToList(),
                Goods = goods.ToList(),
                Look = () => Outfit.Json(i),
                LeftHand = "vanilla:Item_Weapon_Dagger",   // where the game's sellers hold their shiv
            }).ToList(),
        });

        public override void OnLateInitializeMelon() => Page.TryRegister();

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) => Wardrobe.Saw(null);

        public override void OnUpdate()
        {
            try { Wardrobe.Update(); }
            catch (Exception ex) { Wardrobe.Saw(null); MelonLogger.Warning($"[BlackMarket] wardrobe: {ex.GetType().Name}: {ex.Message}"); }
        }

        // On every machine: his look has come by now, so his clothes say who he is.
        private static void AfterNpc(NPCEntity __instance)
        {
            try { if (__instance != null && Outfit.Worn(__instance).Count > 0) Wardrobe.Saw(__instance); }
            catch (Exception ex) { MelonLogger.Warning($"[BlackMarket] {ex.GetType().Name}: {ex.Message}"); }
        }
    }
}
