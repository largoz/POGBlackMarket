using System.Linq;
using Il2CppEntities;
using MelonLoader;
using POGLib.Items;
using UnityEngine;

namespace POGBlackMarket
{
    /// <summary>A player who comes close to him finds his clothes in the wardrobe from then on.</summary>
    internal static class Wardrobe
    {
        private const float Near = 6f;
        private static NPCEntity _him;
        private static float _next;

        internal static void Saw(NPCEntity npc) => _him = npc;

        internal static void Update()
        {
            if (_him == null || Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 0.5f;
            EntityPlayer me = null;
            foreach (var p in Object.FindObjectsOfType<EntityPlayer>()) if (p != null && p.IsOwner) { me = p; break; }
            if (me == null || Vector3.Distance(me.transform.position, _him.transform.position) > Near) return;
            var got = Outfit.Worn(_him).Where(Content.Cosmetics.Unlock).ToList();
            _him = null;   // once per meeting
            if (got.Count > 0) MelonLogger.Msg($"[BlackMarket] found: {string.Join(", ", got)} (in the wardrobe from now on).");
        }
    }
}
