using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Options;
using OnixRuntime.Plugin;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.World;
using System.Runtime.InteropServices;

namespace TradeFinder {

    public class TradeSettings {
        public string Enchant;
        public int Level;
        public int MaxPrice;
        public bool going = false;
        public Vec3 workBlock;
        public string stage;
        public Entity villager;
        public string microStage;

        public TradeSettings(string enchant) {
            Enchant = enchant;

            Console.WriteLine("Please run $select on the workstation");
            stage = "asking block";
        }
        
        public TradeSettings(string enchant, int level) {
            Enchant = enchant;
            Level = level;
            
            Console.WriteLine("Please run $select on the workstation");
            stage = "asking block";
        }
        
        public TradeSettings(string enchant, int level, int maxPrice) {
            Enchant = enchant;
            Level = level;
            MaxPrice = maxPrice;
            
            Console.WriteLine("Please run $select on the workstation");
            stage = "asking block";
        }

        public void begin() {
            going = true;
        }
        
        public void stop() {
            going = false;
        }

        public void selectWorkstation() {
            workBlock = Onix.LocalPlayer.Raycast.HitPosition;
            if (Onix.Region.GetBlock(new BlockPos((int)workBlock.X, (int)workBlock.Y, (int)workBlock.Z)).Name ==
                "lectern") {
                Console.WriteLine("Valid block selected");
                stage = "asking villager";
            }
        }
        public void selectVillager() {
            var entities = Onix.World.Entities;
            foreach (var entity in entities) {
                if (entity.BlockPosition == Onix.LocalPlayer.BlockPosition) {
                    if (entity.TypeNameFull == "minecraft:villager_v2") {
                        // Console.WriteLine("FOUND VILLAGER!");
                        villager = entity;
                        Console.WriteLine("COMPLETE - beginning cycles");
                        begin();
                        break;
                    }
                    
                }
            }
        }

    }
    public static class Globals {
        public static TradeSettings currentSearch;
    }
    public class TradeFinder : OnixPluginBase {
        public static TradeFinder Instance { get; private set; } = null!;
        public static TradeFinderConfig Config { get; private set; } = null!;

        public TradeFinder(OnixPluginInitInfo initInfo) : base(initInfo) {
            Instance = this;
            // If you can clean up what the plugin leaves behind manually, please do not unload the plugin when disabling.
            base.DisablingShouldUnloadPlugin = false;
#if DEBUG
           // base.WaitForDebuggerToBeAttached();
#endif
        }

        protected override void OnLoaded() {
            Console.WriteLine($"Plugin {CurrentPluginManifest.Name} loaded!");
            Config = new TradeFinderConfig(PluginDisplayModule, true);
            Onix.Events.Common.Tick += OnTick;
            Onix.Events.Common.HudRender += OnHudRender;
            Onix.Events.Common.HudRenderGame += OnHudRenderGame;
            Onix.Events.Common.WorldRender += OnWorldRender;
            Onix.Events.Session.Chat.ChatScreenMessageAboutToBeSent += OnChat;
        }

        protected override void OnEnabled() {

        }

        protected override void OnDisabled() {

        }

        protected override void OnUnloaded() {
            // Ensure every task or thread is stopped when this function returns.
            // You can give them base.PluginEjectionCancellationToken which will be cancelled when this function returns. 
            Console.WriteLine($"Plugin {CurrentPluginManifest.Name} unloaded!");
            Onix.Events.Common.Tick -= OnTick;
            Onix.Events.Common.HudRender -= OnHudRender;
            Onix.Events.Common.HudRenderGame -= OnHudRenderGame;
            Onix.Events.Common.WorldRender -= OnWorldRender;
            Onix.Events.Session.Chat.ChatScreenMessageAboutToBeSent -= OnChat;
        }

        private void OnTick() {
            if (Globals.currentSearch.going) {
                //break workstation
                
                // Onix.LocalPlayer.BreakBlock();
            }
        }

        private void OnHudRender(RendererCommon2D gfx, float delta) {
        }
        
        private void OnHudRenderGame(RendererGame gfx, float delta) {
        }

        private void OnWorldRender(RendererWorld gfx, float delta) {
            
        }
        
        private string  OnChat(string message) { // very basic command system
            if (message.StartsWith('$')) { // its a command!
                
                var arguments = message.Split(' ');
                if (arguments.Length >= 1) {
                    switch (arguments[0].Substring(1)) {
                        case "begin":
                            Console.WriteLine("Starting");
                            string enchantment = "";
                            int level = 0;
                            int maxEmeralds = 1000;
                            
                            if (arguments.Length >= 2) {
                                enchantment = arguments[1];
                            } else if (arguments.Length >= 3) {
                                level = int.Parse(arguments[2]);
                            } else if(arguments.Length >= 4) {
                                maxEmeralds = int.Parse(arguments[3]);
                            } else {
                                Console.WriteLine("Too many/not enough arguments");
                                return "";
                            }


                            Globals.currentSearch = new TradeSettings(enchantment, level, maxEmeralds);

                            Console.WriteLine($"Began looking for {enchantment} of level {level} for less the {maxEmeralds}");
                            
                            break;

                        case "select":
                            if (Globals.currentSearch.stage == "asking block") {
                                Console.WriteLine("Got block");
                                Globals.currentSearch.selectWorkstation();
                            } else if (Globals.currentSearch.stage == "asking villager"){
                                Console.WriteLine("Asking villager");
                                Globals.currentSearch.selectVillager();   
                            }

                            break;
                        
                            
                        case "pause":
                            Console.WriteLine("Ceased!");
                            Globals.currentSearch.stop();
                            break;
                        case "play":
                            Console.WriteLine("Began!");
                            Globals.currentSearch.begin();
                            break;
                        case "odds":
                            Console.WriteLine("odds calculator coming soon!");
                            break;
                        
                    }
                }
                return ""; // cancel message
            }
            return message; // let message through
        }
    }
}