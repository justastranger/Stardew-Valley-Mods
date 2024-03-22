﻿namespace ForageFantasy
{
    using HarmonyLib;
    using StardewValley;
    using System;
    using StardewObject = StardewValley.Object;

    internal class OneClickShedReloaderCompatibility
    {
        private static ForageFantasy mod;

        internal static void ApplyPatches(ForageFantasy forageFantasy, Harmony harmony)
        {
            mod = forageFantasy;

            try
            {
                mod.DebugLog("This mod patches OneClickShedReloader. If you notice issues with OneClickShedReloader, make sure it happens without this mod before reporting it to the OneClickShedReloader page.");

                var handler = AccessTools.TypeByName("BitwiseJonMods.OneClickShedReloader.BuildingContentsHandler");
                var entry = AccessTools.TypeByName("BitwiseJonMods.OneClickShedReloader.ModEntry");

                harmony.Patch(
                   original: AccessTools.Method(handler, "TryAddItemToPlayerInventory"),
                   prefix: new HarmonyMethod(typeof(OneClickShedReloaderCompatibility), nameof(TryAddItemToPlayerInventory_Pre)));

                //harmony.Patch(
                //   original: AccessTools.Method(handler, "TryAddItemToPlayerInventory"),
                //   postfix: new HarmonyMethod(typeof(OneClickShedReloaderCompatibility), nameof(TryAddItemToPlayerInventory_Post)));

                harmony.Patch(
                   original: AccessTools.Method(entry, "HarvestAllItemsInBuilding"),
                   postfix: new HarmonyMethod(typeof(OneClickShedReloaderCompatibility), nameof(ReduceQualityAfterHarvest)));
            }
            catch (Exception e)
            {
                mod.ErrorLog($"Error while trying to patch OneClickShedReloader. Please report this to the mod page of {mod.ModManifest.Name}, not OneClickShedReloader:", e);
            }
        }

        public static void TryAddItemToPlayerInventory_Pre(Farmer player, Item item, StardewObject container)
        {
            if (mod.Config.MushroomBoxQuality && container.IsMushroomBox())
            {
                item.Quality = ForageFantasy.DetermineForageQuality(player);
            }
        }

        //public static void TryAddItemToPlayerInventory_Post(StardewObject container, ref bool __result)
        //{
        //    // I can't reduce the quality of a non successfully harvested box here,
        //    // because it doesn't get called if the method throws an inventoryfull exception
        //    if (__result && container.IsMushroomBox())
        //    {
        //        if (mod.Config.AutomationHarvestsGrantXP)
        //        {
        //            TapperAndMushroomQualityLogic.RewardMushroomBoxExp(mod.Config, Game1.player);
        //        }
        //    }
        //}

        public static void ReduceQualityAfterHarvest(GameLocation location)
        {
            // reduce quality of non successfully harvested items and reset in general
            if (!mod.Config.MushroomBoxQuality)
            {
                return;
            }

            foreach (var item in location.Objects.Values)
            {
                if (item.IsMushroomBox())
                {
                    if (item.heldObject.Value != null)
                    {
                        item.heldObject.Value.Quality = StardewObject.lowQuality;
                    }
                }
            }
        }
    }
}