using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Oblivion
{
    // Keep non-static so users can extend to add their own ModKeys cleanly.  No harm no foul
    public class Constants
    {
        public static readonly ModKey Oblivion = new ModKey("Oblivion", type: ModType.Master);
        public static readonly ModKey Knights = new ModKey("Knights", type: ModType.Plugin);
        public static readonly ModKey ShiveringIsles = new ModKey("DLCShiveringIsles", type: ModType.Plugin);
        public static readonly ModKey BattlehornCastle = new ModKey("DLCBattlehornCastle", type: ModType.Plugin);
        public static readonly ModKey HorseArmor = new ModKey("DLCHorseArmor", type: ModType.Plugin);
        public static readonly ModKey Orrery = new ModKey("DLCOrrery", type: ModType.Plugin);
        public static readonly ModKey Frostcrag = new ModKey("DLCFrostcrag", type: ModType.Plugin);
        public static readonly ModKey ThievesDen = new ModKey("DLCThievesDen", type: ModType.Plugin);
        public static readonly ModKey MehrunesRazor = new ModKey("DLCMehrunesRazor", type: ModType.Plugin);
        public static readonly ModKey VileLair = new ModKey("DLCVileLair", type: ModType.Plugin);
        public static readonly ModKey SpellTomes = new ModKey("DLCSpellTomes", type: ModType.Plugin);
        public static IEnumerable<ModKey> BaseGameModKeys => EnumerateBaseGameModKeys();

        public static readonly FormKey Player = new FormKey(Oblivion, id: 0x14);

        private static IEnumerable<ModKey> EnumerateBaseGameModKeys()
        {
            yield return Oblivion;
            yield return Knights;
            yield return ShiveringIsles;
            yield return BattlehornCastle;
            yield return HorseArmor;
            yield return Orrery;
            yield return Frostcrag;
            yield return ThievesDen;
            yield return MehrunesRazor;
            yield return VileLair;
            yield return SpellTomes;
        }
    }
}
