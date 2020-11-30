using AcidProofSuit.Module;
using System.Reflection;
using HarmonyLib;
using QModManager.API.ModLoading;
using System.IO;
namespace AcidProofSuit
{
    [QModCore]
    public static class Main
    {
        public static bool bInAcid = false; // Whether or not the player is currently immersed in acid

        // This function was stol*cough*take*cough*nicked wholesale from FCStudios
        public static object GetPrivateField<T>(this T instance, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
        {
            return typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | bindingFlags).GetValue(instance);
        }

        internal struct DamageInfo
        {
            public DamageType damageType;
            public float damageMult;

            public DamageInfo(DamageType t, float m)
            {
                this.damageType = t;
                this.damageMult = m;
            }
        }

        internal struct DamageResistance
        {
            public TechType TechType;
            public DamageInfo[] damageInfoList;

            public DamageResistance(TechType tt, DamageInfo[] dil)
            {
                this.TechType = tt;
                this.damageInfoList = dil;
            }
        }

        // This particular system is not that useful, but it could be expanded to allow any sort of equipment type to reduce damage.
        // For example, you could add a chip that projects a sort of shield that protects from environmental damage, such as Acid, Radiation, Heat, Poison, or others.
        // Although the system would need to be extended to allow, say, a shield that drains a battery when resisting damage.
        internal static DamageResistance[] DamageResistances;
        public static float ModifyDamage(TechType tt, float damage, DamageType type)
        {
            float baseDamage = damage;
            float damageMod = 0;
            //Logger.Log(Logger.Level.Debug, $"Main.ModifyDamage called: tt = {tt.ToString()}, damage = {damage}; DamageType = {type}");
            foreach (DamageResistance r in DamageResistances)
            {
                //Logger.Log(Logger.Level.Debug, $"Found DamageResistance with TechType: {r.TechType.ToString()}");
                if (r.TechType == tt)
                {
                    foreach (DamageInfo d in r.damageInfoList)
                    {
                        if (d.damageType == type)
                        {
                            damageMod += baseDamage * d.damageMult;
                            //Logger.Log(Logger.Level.Debug, $"Player has equipped armour of TechType {tt.ToString()}, base damage = {baseDamage}, type = {type}, modifying damage by {d.damageMult}x with result of {damageMod}");
                        }
                    }
                }
            }
            return damageMod;
        }
        private static Assembly myAssembly = Assembly.GetExecutingAssembly();
        private static string modPath = Path.GetDirectoryName(myAssembly.Location);
        internal static string AssetsFolder = Path.Combine(modPath, "Assets");
        [QModPatch]
        public static void Load()
        {
            SMLHelper.V2.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, "BodyMenu", "Suit Upgrades", SpriteManager.Get(TechType.Stillsuit));

            var glovesPrefab = new AcidGlovesPrefab();
            glovesPrefab.Patch();
            var helmetPrefab = new AcidHelmetPrefab();
            helmetPrefab.Patch();
            // The gloves and helmet are used in the Suit recipe as Linked Items, and they must be patched before the suit.
            var suitPrefab = new AcidSuitPrefab();
            suitPrefab.Patch();
            var bpOnlyRadSuit = new bpSupplemental_OnlyRadSuit();
            bpOnlyRadSuit.Patch();
            var bpOnlyRebreather = new bpSupplemental_OnlyRebreather();
            bpOnlyRebreather.Patch();
            var bpOnlyReinforced = new bpSupplemental_OnlyReinforcedSuit();
            bpOnlyReinforced.Patch();
            var bpSuits = new bpSupplemental_Suits();
            bpSuits.Patch();
            var bpRebreatherRad = new bpSupplemental_OnlyRebreather();
            bpRebreatherRad.Patch();
            var bpRebReinf = new bpSupplemental_RebreatherReinforced();
            bpRebReinf.Patch();
            var bpRadReinf = new bpSupplemental_RadReinforced();
            bpRadReinf.Patch();

            Main.DamageResistances = new DamageResistance[3] {
            // Gloves
                new DamageResistance(
                    glovesPrefab.TechType,
                    new DamageInfo[] {
                        new DamageInfo(DamageType.Acid, -0.15f)/*,
                        new DamageInfo(DamageType.Radiation, -0.10f)*/
                    }),


            // Helmet
                new DamageResistance(
                    helmetPrefab.TechType,
                    new DamageInfo[] {
                        new DamageInfo(DamageType.Acid, -0.25f)/*,
                        new DamageInfo(DamageType.Radiation, -0.20f)*/
                    }),


            // Suit
                new DamageResistance(
                    suitPrefab.TechType,
                    new DamageInfo[] {
                        new DamageInfo(DamageType.Acid, -0.6f)/*,
                        new DamageInfo(DamageType.Radiation, -0.70f)*/
                    })
            };
            Harmony.CreateAndPatchAll(myAssembly, $"DaWrecka_{myAssembly.GetName().Name}");
        }
    }
}
