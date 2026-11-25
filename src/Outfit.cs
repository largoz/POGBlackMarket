using System.Collections.Generic;
using System.IO;
using Il2CppEntities;
using POGLib.Cosmetics;
using POGLib.Items;

namespace POGBlackMarket
{
    /// <summary>His coat and hat (POGLib's cosmetics; a player finds them by meeting him) and the three looks.</summary>
    internal static class Outfit
    {
        internal const string Coat = BlackMarketMod.Id + ":Coat", Hat = BlackMarketMod.Id + ":Hat";

        // In the order of Settings.Looks.
        private static readonly GoblinLook[] Looks =
        {
            Look("CC_Goblin_Feet_Reward_Shoes_01", 11, 13, 11, 6, 0),
            Look(null, 11, 13, 11, 12, 1, "CC_Goblin_Accessory_Reward_Thiefmask", "CC_Goblin_Clothing_Reward_Gloves_01"),
            Look(null, 1, 18, 3, 17, 2, "CC_Goblin_Clothes_Reward_Drop_HeadMask_01"),
        };

        private static GoblinLook Look(string feet, int coat, int hat, int legs, int skin, int eyes, params string[] accessories)
        {
            var l = new GoblinLook { Hair = Hat, Chest = Coat, Legs = "CC_Goblin_Reward_SlimPants_01", Feet = feet, ChestColor = coat, HairColor = hat, LegsColor = legs, Skin = skin, EyeColor = eyes };
            l.Accessories.AddRange(accessories);
            return l;
        }

        internal static void Register(string folder)
        {
            void Part(string id, CosmeticSlot slot, params string[] hidden)
            {
                var d = new CosmeticDefinition
                {
                    ModId = BlackMarketMod.Id, ContentId = id, Slot = slot, Locked = true,
                    ModelFile = Path.Combine(folder, id.ToLowerInvariant() + ".glb"), ThumbnailFile = Path.Combine(folder, id.ToLowerInvariant() + ".png"),
                };
                d.HiddenInFirstPerson.AddRange(hidden);
                Content.Cosmetics.Register(d);
            }
            Part("Coat", CosmeticSlot.Chest, "collar");   // the collar stands before the wearer's own eyes
            Part("Hat", CosmeticSlot.Hair);               // a hair style: the game colours it with the wearer's hair colour
        }

        internal static string Json(int look) => Content.Cosmetics.LookJson(Looks[look]);

        /// <summary>Which of his clothes this NPC wears, read from the look every player has.</summary>
        internal static List<string> Worn(NPCEntity npc)
        {
            var worn = new List<string>();
            var d = npc.CosmeticsData;
            uint coat = Content.Cosmetics.IdOf(Coat), hat = Content.Cosmetics.IdOf(Hat);
            if (coat != 0 && d.ChestId == coat) worn.Add(Coat);
            if (hat != 0 && d.HairId == hat) worn.Add(Hat);
            return worn;
        }
    }
}
