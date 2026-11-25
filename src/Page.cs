using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using POGMods.Config;

namespace POGBlackMarket
{
    /// <summary>The optional POG Config page. Everything of POG Config's is behind a NoInlining call, so the mod works without it.</summary>
    internal static class Page
    {
        private static bool _capturing;   // POG Config takes a row's value at registration as the one RESTORE DEFAULTS goes back to

        internal static void TryRegister()
        {
            _capturing = true;
            try { Register(); }
            catch (Exception ex) when (ex is FileNotFoundException || ex is TypeLoadException || ex is MissingMethodException) { }   // no POG Config, or an older one
            finally { _capturing = false; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Register()
        {
            var rows = new List<ConfigEntry> { new HeaderEntry("How often each look comes up (host)") };
            var steps = Enumerable.Range(0, 11).Select(i => i == 0 ? "never" : i.ToString()).ToArray();
            foreach (var l in Settings.Looks)
                rows.Add(new OptionsSliderEntry(l.Label,
                    () => Math.Max(0, Math.Min(10, (int)Math.Round(_capturing ? l.Standard : l.Entry.Value))),
                    i => Settings.SetWeight(l, i), steps));
            rows.Add(new HeaderEntry("Whose stalls he takes (host)"));
            foreach (var s in Settings.AllStalls)
                rows.Add(new ToggleEntry(s, () => _capturing ? Settings.DefaultStalls.Split(',').Contains(s) : Settings.Takes(s), on => Settings.SetTakes(s, on)));
            POGConfig.Register("Black Market", rows, new ModInfo(BlackMarketMod.Version,
                "A goblin in a trench coat who sometimes keeps a dungeon merchant's stall. These settings count when you host, from the next start of the game. " +
                "How often he comes at all is on the page POG Shops.", "largo"));
        }
    }
}
