//
// Copyright (c) 2021, Aaron Shumate
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE.txt file in the root directory of this source tree. 
//
// Dyson Sphere Program is developed by Youthcat Studio and published by Gamera Game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace DSPGodModeButton
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("DSPGAME.exe")]
    public class DSPGodModeButton : BaseUnityPlugin
    {
        public const string pluginGuid = "greyhak.dysonsphereprogram.godmodebutton";
        public const string pluginName = "DSP God Mode Button";
        public const string pluginVersion = "1.0.0";
        new internal static ManualLogSource Logger;
        Harmony harmony;
        public static bool initialCheckFlag = true;
        public static bool valueAtLastCheck = true;  // This initial value won't matter.

        public void Awake()
        {
            Logger = base.Logger;  // "C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\BepInEx\LogOutput.log"

            harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(DSPGodModeButton));

            enabledSprite = GetSprite(new Color(0, 1, 0));  // Bright Green
            disabledSprite = GetSprite(new Color(0.5f, 0.5f, 0.5f));  // Medium Grey

            Logger.LogInfo("Initialization complete.");
        }

        public static RectTransform enableDisableButton = null;
        public static Sprite enabledSprite;
        public static Sprite disabledSprite;

        [HarmonyPrefix, HarmonyPatch(typeof(GameMain), "Begin")]
        public static void GameMain_Begin_Prefix()
        {
            if (GameMain.instance != null && GameObject.Find("Game Menu/button-1-bg") && enableDisableButton == null)
            {
                RectTransform parent = GameObject.Find("Game Menu").GetComponent<RectTransform>();
                RectTransform prefab = GameObject.Find("Game Menu/button-1-bg").GetComponent<RectTransform>();
                Vector3 referencePosition = GameObject.Find("Game Menu/button-1-bg").GetComponent<RectTransform>().localPosition;
                enableDisableButton = GameObject.Instantiate<RectTransform>(prefab);
                enableDisableButton.gameObject.name = "greyhak-god-mode-button";
                UIButton uiButton = enableDisableButton.GetComponent<UIButton>();
                uiButton.tips.delay = 0f;
                enableDisableButton.SetParent(parent);
                enableDisableButton.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                enableDisableButton.localPosition = new Vector3(referencePosition.x + 23.5f, referencePosition.y + 161f, referencePosition.z);
                uiButton.OnPointerDown(null);
                uiButton.OnPointerEnter(null);
                uiButton.button.onClick.AddListener(() =>
                {
                    PlayerController.operationWhenBuild = !PlayerController.operationWhenBuild;  // This is the same change made by the Toggle God Mod mod
                    DSPGame.globalOption.buildingViewMode = PlayerController.operationWhenBuild ? 1 : 0;  // This is not changed by the Toggle God Mod mod
                });
            }
        }

        public static Sprite GetSprite(Color color)
        {
            Texture2D tex = new Texture2D(48, 48, TextureFormat.RGBA32, false);

            ulong[] godEye = new ulong[]
            { 0x1F0,0x3F0,0xFF0,0x3FF0,0x7FF0,0x1FFF0,0x7FFF0,0xFFFF0,0x3FFFF0,0xFFFFF0,0x1FFFFF0,0x7FFFFF0,0x1FFFFFF0,0x3FFFFFF0,0xFFF87FF0,0x3FFE01FF0,0x7FF8007F0,0x1FFE0001F0,0x7FFC0000F0,0xFFFC0780F0,0x3FFF80FC070,0xFFFF81FE070,0x3FFFF83FF070,0xFFFFF83FF070,0xFFFFF83FF070,0x3FFFF83FF070,0xFFFF81FE070,0x3FFF80FC070,0xFFFC0780F0,0x7FFC0000F0,0x1FFE0001F0,0x7FF8007F0,0x3FFE01FF0,0xFFF87FF0,0x3FFFFFF0,0x1FFFFFF0,0x7FFFFF0,0x1FFFFF0,0xFFFFF0,0x3FFFF0,0xFFFF0,0x7FFF0,0x1FFF0,0x7FF0,0x3FF0,0xFF0,0x3F0,0x1F0 };

            for (int x = 0; x < 48; x++)
            {
                for (int y = 0; y < 48; y++)
                {
                    tex.SetPixel(x, y, ((godEye[x] >> y) & 1) == 1 ? color : new Color(0, 0, 0, 0));
                }
            }

            tex.name = "greyhak-god-mode-icon";
            tex.Apply();

            return Sprite.Create(tex, new Rect(0f, 0f, 48f, 48f), new Vector2(0f, 0f), 1000);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameData), "GameTick")]
        public static void GameData_GameTick_Postfix()
        {
            if ((enableDisableButton != null) && (initialCheckFlag || valueAtLastCheck != PlayerController.operationWhenBuild))
            {
                initialCheckFlag = false;
                valueAtLastCheck = PlayerController.operationWhenBuild;
                DSPGame.globalOption.buildingViewMode = PlayerController.operationWhenBuild ? 1 : 0;

                bool tiggleGodModeModInstalledFlag = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("doobist.dsp.godmodetoggle");

                UIButton uiButton = enableDisableButton.GetComponent<UIButton>();
                uiButton.tips.tipTitle = PlayerController.operationWhenBuild ? "God Mode Disabled" : "God Mode Enabled";
                uiButton.tips.tipText = PlayerController.operationWhenBuild ? "Click to enable god build mode." : "Click to disable god build mode.";
                if (tiggleGodModeModInstalledFlag)
                    uiButton.tips.tipText += "\nOr press \\ to toggle mode.";
                enableDisableButton.transform.Find("button-1/icon").GetComponent<Image>().sprite =
                    PlayerController.operationWhenBuild ? disabledSprite : enabledSprite;
                uiButton.UpdateTip();
            }
        }
    }
}
