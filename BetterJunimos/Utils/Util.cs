﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace BetterJunimos.Utils {
    public class Util {
        private const int UnpaidRadius = 3;
        public const int CoffeeId = 433;

        private const int GemCategory = -2;
        private const int MineralCategory = -12;

        public const int ForageCategory = -81;
        public const int FlowerCategory = -80;
        public const int FruitCategory = -79;
        public const int WineCategory = -26;

        internal static IReflectionHelper Reflection;
        internal static JunimoAbilities Abilities;
        internal static JunimoPayments Payments;
        internal static JunimoProgression Progression;

        public static int CurrentWorkingRadius {
            get {
                if (!BetterJunimos.Config.JunimoPayment.WorkForWages) return BetterJunimos.Config.JunimoHuts.MaxRadius;
                if (Payments.WereJunimosPaidToday) return BetterJunimos.Config.JunimoHuts.MaxRadius;
                return UnpaidRadius;
            }
        }

        public static List<JunimoHut> GetAllHuts() {
            return Game1.getFarm().buildings.OfType<JunimoHut>().ToList();
        }

        public static Guid GetHutIdFromHut(JunimoHut hut) {
            return Game1.getFarm().buildings.GuidOf(hut);
        }

        public static JunimoHut GetHutFromId(Guid id) {
            return Game1.getFarm().buildings[id] as JunimoHut;
        }

        public static void AddItemToChest(Farm farm, Chest chest, SObject item) {
            Item obj = chest.addItem(item);
            if (obj == null)
                return;
            Vector2 pos = chest.TileLocation;
            for (int index = 0; index < obj.Stack; ++index)
                Game1.createObjectDebris(item.ParentSheetIndex, (int) pos.X + 1, (int) pos.Y + 1, -1, item.Quality, 1f,
                    farm);
        }

        public static void RemoveItemFromChest(Chest chest, Item item, int count = 1) {
            if (BetterJunimos.Config.FunChanges.InfiniteJunimoInventory) {
                return;
            }

            item.Stack -= count;
            if (item.Stack <= 0) {
                chest.items.Remove(item);
            }
        }

        public static void SpawnJunimoAtHut(JunimoHut hut) {
            Vector2 pos = new Vector2((float) hut.tileX.Value + 1, (float) hut.tileY.Value + 1) * 64f +
                          new Vector2(0.0f, 32f);
            SpawnJunimoAtPosition(pos, hut, hut.getUnusedJunimoNumber());
        }

        public static void SpawnJunimoAtPosition(Vector2 pos, JunimoHut hut, int junimoNumber) {
            if (hut == null) return;
            Farm farm = Game1.getFarm();
            /*
             * Added by Mizzion. This will set the color of the junimos based on what gem is inside the hut.
             */
            bool isPrismatic = false;
            Color?
                gemColor = GetGemColor(ref isPrismatic,
                    hut); //Reflection.GetMethod(hut, "getGemColor").Invoke<Color>(isPrismatic);
            /*
             * End added By Mizzion
             */

            JunimoHarvester junimoHarvester = new JunimoHarvester(pos, hut, junimoNumber, gemColor);
            junimoHarvester.isPrismatic.Value = isPrismatic; //Added by Mizzion, Fixes the Prismatic Junimos.
            farm.characters.Add(junimoHarvester);
            hut.myJunimos.Add(junimoHarvester);

            if (Game1.isRaining) {
                var alpha = Reflection.GetField<float>(junimoHarvester, "alpha");
                alpha.SetValue(BetterJunimos.Config.FunChanges.RainyJunimoSpiritFactor);
            }

            if (!Utility.isOnScreen(Utility.Vector2ToPoint(pos), 64, farm))
                return;
            farm.playSound("junimoMeep1");
        }

/*
 * Added by Mizzion. This method is used to get the gem color, so the junimos can be colored
 * I ripped this from SDV and edited it to work with this mod.
*/
        private static Color? GetGemColor(ref bool isPrismatic, JunimoHut hut) {
            var colorList = new List<Color>();
            var chest = hut.output.Value;
            foreach (Item dyeObject in chest.items) {
                if (dyeObject != null &&
                    (dyeObject.Category == MineralCategory || dyeObject.Category == GemCategory)) {
                    Color? dyeColor = TailoringMenu.GetDyeColor(dyeObject);
                    if (dyeObject.Name == "Prismatic Shard")
                        isPrismatic = true;
                    if (dyeColor.HasValue)
                        colorList.Add(dyeColor.Value);
                }
            }

            if (colorList.Count > 0)
                return colorList[Game1.random.Next(colorList.Count)];
            return new Color?();
        }

        public static void SendMessage(string msg) {
            if (!BetterJunimos.Config.Other.ReceiveMessages) return;

            Game1.addHUDMessage(new HUDMessage(msg, 3) {
                noIcon = true,
                timeLeft = HUDMessage.defaultTime
            });

            // try {
            //     var multiplayer = Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            //     multiplayer.broadcastGlobalMessage("Strings\\StringsFromCSFiles:"+msg);
            // }
            // catch (InvalidOperationException) {
            //     BetterJunimos.SMonitor.Log($"SendMessage: multiplayer unavailable", LogLevel.Error);
            // }
        }

        public static void SpawnParticles(Vector2 pos) {
            Multiplayer multiplayer = Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[] {
                new TemporaryAnimatedSprite(17, new Vector2(pos.X * 64f, pos.Y * 64f), Color.White, 7,
                    Game1.random.NextDouble() < 0.5, 125f)
            });
            multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[] {
                new TemporaryAnimatedSprite(14, new Vector2(pos.X * 64f, pos.Y * 64f), Color.White, 7,
                    Game1.random.NextDouble() < 0.5, 50f)
            });
        }
    }
}