using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SixSense
{
    [BepInPlugin("com.drainlord.SixSense", "SixSense", "0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private static GameWorld gameWorld;
        private static Player localPlayer;
        private static Texture2D _staticRectTexture;
        private static GUIStyle _staticRectStyle;

        // config values
        private ConfigEntry<int> staticAmount;
        private ConfigEntry<Vector2> staticSize;
        private ConfigEntry<int> staticSpread;
        private ConfigEntry<float> staticTransparency;
        private ConfigEntry<bool> staticTransparencyFade;
        private ConfigEntry<bool> senseDistanceCheck;
        private ConfigEntry<int> senseDistance;
        private ConfigEntry<float> boxSize;
        private ConfigEntry<bool> modEnabled;
        private ConfigEntry<bool> showDistanceValue;

        private void Awake()
        {
            staticAmount = Config.Bind("Visual", "Amount of Static Cubes", 6, new ConfigDescription("Higher amounts will cause more lag", new AcceptableValueRange<int>(1, 20), Array.Empty<object>()));
            staticSize = Config.Bind("Visual", "Size of Static Cubes", new Vector2(5, 5), "Size of Static Cubes");
            staticSpread = Config.Bind("Visual", "Position Spread of Static Cubes", 55, new ConfigDescription("How spread out the cubes will be from eachother", new AcceptableValueRange<int>(0, 250), Array.Empty<object>()));
            staticTransparency = Config.Bind("Visual", "Opacity of Static Cubes", 0.5f, new ConfigDescription("How opaque the cubes will be", new AcceptableValueRange<float>(0f, 1f), Array.Empty<object>()));
            staticTransparencyFade = Config.Bind("Visual", "Increase transparency based on distance", true, "Further enemies will have more transparent static");

            senseDistanceCheck = Config.Bind("Functionality", "Check Enemy Distance", true, "Check distance of enemy before showing static");
            senseDistance = Config.Bind("Functionality", "Maximum Enemy Distance", 30, "How far an enemy can be before static disappears");

            boxSize = Config.Bind("Misc", "Player Box Size", 1.2f, new ConfigDescription("Affects static box size on players", new AcceptableValueRange<float>(0.1f, 2.5f), Array.Empty<object>()));
            modEnabled = Config.Bind("Misc", "Mod Enabled", true, "Turn mod off or on");
            showDistanceValue = Config.Bind("Misc", "Show Distance Value", false, "Show distance value on enemies");
        }

        void OnGUI()
        {
            if (!modEnabled.Value) {return;}

            //GUI.Label(new Rect(5, 0, 350, 30), "OFFICIAL DRAIN© LICENSED PRODUCT 2023 | gtb.sg");

            if (localPlayer)
            {
                foreach (var plr in gameWorld.AllAlivePlayersList)
                {
                    if (plr.HealthController is {IsAlive: true} && plr != localPlayer)
                    {
                        var distance = 0f;

                        if (senseDistanceCheck.Value)
                        {
                            distance = Vector3.Distance(Camera.main.transform.position, plr.Transform.position);
                            if (senseDistance.Value < distance)
                            {
                                continue;
                            }
                        }

                        var pos = Camera.main.WorldToScreenPoint(plr.Transform.position);
                        var scale = Screen.height / (float)Camera.main.scaledPixelHeight;
                        pos.y = Screen.height - pos.y * scale;
			            pos.x *= scale;

                        if (pos is { z: > 0.01f, x: > -5f, y: > -5f } && pos.x < Screen.width && pos.y < Screen.height)
                        {
                            var headPos = Camera.main.WorldToScreenPoint(plr.PlayerBones.Head.position);
                            headPos.y = Screen.height - headPos.y * scale;
			                headPos.x *= scale;

                            var boxHeight = Mathf.Abs(headPos.y - pos.y) * boxSize.Value;
				            var boxWidth = boxHeight * 0.62f;

                            var finalX = pos.x - boxWidth/2;
                            var finalY = pos.y - boxHeight;

                            if (showDistanceValue.Value)
                            {
                                GUI.Label(new Rect(pos.x, pos.y, 100, 30), distance.ToString());
                            }

                            for (int i = 0; i < staticAmount.Value; i++)
                            {
                                var randomX = UnityEngine.Random.Range(finalX - staticSpread.Value, finalX + boxWidth + staticSpread.Value);
                                var randomY = UnityEngine.Random.Range(finalY - staticSpread.Value, finalY + boxHeight + staticSpread.Value);
                                var randomXSize = staticSize.Value.x;
                                var randomYSize = staticSize.Value.y;
                                var randomGrey = UnityEngine.Random.Range(0, 10) * 0.1f; 
                                var transparency = staticTransparency.Value;
                                
                                if (staticTransparencyFade.Value)
                                {
                                    transparency = 1 - (distance / senseDistance.Value);
                                }

                                GUIDrawRect(new Rect(randomX, randomY, randomXSize, randomYSize), new Color(randomGrey, randomGrey, randomGrey, transparency));
                            }
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                localPlayer = null;
                return;
            }
            
            gameWorld = Singleton<GameWorld>.Instance;
            localPlayer = gameWorld.MainPlayer;
        }

        public static void GUIDrawRect(Rect position, Color color)
        {
            if(_staticRectTexture == null)
            {
                _staticRectTexture = new Texture2D(1, 1);
            }
 
            if(_staticRectStyle == null)
            {
                _staticRectStyle = new GUIStyle();
            }
 
            _staticRectTexture.SetPixel(0, 0, color);
            _staticRectTexture.Apply();
 
            _staticRectStyle.normal.background = _staticRectTexture;
 
            GUI.Box(position, GUIContent.none, _staticRectStyle);
        }
    }
}
