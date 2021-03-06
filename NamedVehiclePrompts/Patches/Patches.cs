﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Oculus.Newtonsoft.Json;
using Logger = QModManager.Utility.Logger;

namespace NamedVehiclePrompts.Patches
{
    class Patches
    {
        [HarmonyPatch(typeof(DockedVehicleHandTarget), nameof(DockedVehicleHandTarget.OnHandHover))]
        public static class DockedVehicleHandTarget_OnHandHover_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref DockedVehicleHandTarget __instance, GUIHand hand)
            {
                Vehicle dockedVehicle = __instance.dockingBay.GetDockedVehicle();
                if (!(dockedVehicle != null))
                {
                    HandReticle.main.SetInteractInfo("NoVehicleDocked");
                    return false;
                }
                bool crushDanger = false;
                CrushDamage crushDamage = dockedVehicle.crushDamage;
                if (crushDamage != null)
                {
                    float crushDepth = crushDamage.crushDepth;
                    if (Ocean.main.GetDepthOf(Player.main.gameObject) > crushDepth)
                    {
                        crushDanger = true;
                    }
                }
                string text = (dockedVehicle is Exosuit) ? "EnterExosuit" : "EnterSeamoth";
                bool result;
                string prompt = "";
                string vehicleName = dockedVehicle.GetName();
                result = Main.TryGetVehiclePrompt(text, Language.main.GetCurrentLanguage(), dockedVehicle.GetName(), out prompt);
                //Logger.Log(Logger.Level.Debug, $"DockedVehicleHandTarget_OnHandHover_Patch.Prefix(): with docked vehicle {vehicleName}, called OnHandHover with text {text}; Main.TryGetVehiclePrompt({text}) returned prompt '{prompt}'", null, true);
                if (result)
                {
                    text = prompt;
                }
                if (crushDanger)
                {
                    HandReticle.main.SetInteractText(text, "DockedVehicleDepthWarning");
                    return false;
                }
                EnergyMixin component = dockedVehicle.GetComponent<EnergyMixin>();
                LiveMixin liveMixin = dockedVehicle.liveMixin;
                if (component.charge < component.capacity)
                {
                    string format = Language.main.GetFormat<float, float>("VehicleStatusFormat", liveMixin.GetHealthFraction(), component.GetEnergyScalar());
                    HandReticle.main.SetInteractText(text, format, true, false, HandReticle.Hand.Left);
                }
                else
                {
                    string format2 = Language.main.GetFormat<float>("VehicleStatusChargedFormat", liveMixin.GetHealthFraction());
                    HandReticle.main.SetInteractText(text, format2, true, false, HandReticle.Hand.Left);
                }
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);

                return false;
            }
        }

        /*[HarmonyPatch(typeof(CinematicModeTrigger), nameof(CinematicModeTrigger.OnHandHover))]
        public static class CinematicModeTrigger_OnHandHover_Patch
        {
            static int timer = 0;

            [HarmonyPrefix]
            public static bool Prefix(ref UseableDiveHatch __instance, GUIHand hand)
            {
            }
        }*/

        /*[HarmonyPatch(typeof(UseableDiveHatch), nameof(UseableDiveHatch.OnHandHover))]
        public static class UseableDiveHatch_OnHandHover_Patch
        {
            static int timer = 0;

            [HarmonyPrefix]
            public static bool Prefix(ref UseableDiveHatch __instance, GUIHand hand)
            {
                if (timer > 0)
                {
                    timer--;
                }
                else
                {
                    Logger.Log(Logger.Level.Debug, $"UseableDiveHatch_OnHandHover_Patch.Prefix() executing", null, true);
                    timer = 30;
                }

                if(!__instance.enabled)
                {
                    return false;
                }
                string interactText = (Player.main.IsInsideWalkable() && Player.main.currentWaterPark == null) ? __instance.exitCustomText : __instance.enterCustomText;
                if (__instance.enterOnly)
                {
                    interactText = __instance.enterCustomText;
                }
                string result = "";
                SubRoot currentSub = Player.main.GetCurrentSub();
                string subName = "";
                if(currentSub != null)
                {
                    subName = currentSub.GetSubName();
                    if (Main.TryGetVehiclePrompt(interactText, Language.main.GetCurrentLanguage(), subName, out result))
                    {
                        interactText = result;
                    }
                    else if (timer == 30)
                    {
                        Logger.Log(Logger.Level.Debug, $"Couldn't retrieve text value for key {interactText}", null, true);
                    }
                }
                HandReticle.main.SetInteractText(interactText);
                HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);

                return false;
            }
        }*/

        [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnHandHover))]
        public static class Vehicle_OnHandHover_Patch
        {
            private static int timer = 0;

            [HarmonyPrefix]
            public static bool Prefix(Vehicle __instance, GUIHand hand)
            {
                string vehicleName = __instance.GetName();
                string handLabel = __instance.handLabel;
                string vehicleClass = handLabel;//.Replace("Enter", "");
                string result = "";
                bool success = Main.TryGetVehiclePrompt(vehicleClass, Language.main.GetCurrentLanguage(), vehicleName, out result);
                //Language.main.TryGet(vehicleClass, out result);
                
                if (timer == 0)
                {
                    timer = 15;
                    //Logger.Log(Logger.Level.Debug, $"Vehicle_OnHandHover_Patch.Prefix(): Vehicle {vehicleName} called OnHandHover with handLabel {handLabel}; Language.TryGet('{vehicleClass}') returned result '{result}'", null, true);
                    //Logger.Log(Logger.Level.Debug, $"Vehicle_OnHandHover_Patch.Prefix(): Vehicle {vehicleName} called OnHandHover with handLabel {handLabel}; Main.TryGetVehiclePrompt({vehicleClass}) returned result '{result}'", null, true);
                }
                else
                {
                    timer--;
                }
                if (success)
                {
                    if (!__instance.GetPilotingMode() /*&& __instance.GetEnabled()*/)
                    {
                        HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
                        HandReticle.main.SetInteractText(result);
                    }
                    return false;
                }

                return true;
            }
        }

    }
}
