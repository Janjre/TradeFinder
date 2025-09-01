using OnixRuntime.Api;
using OnixRuntime.Api.Entities;
using OnixRuntime.Api.Inputs;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Options;
using OnixRuntime.Plugin;
using OnixRuntime.Api.Rendering;
using OnixRuntime.Api.UI;
using OnixRuntime.Api.World;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace TradeFinder {
    using System;
    using System.Collections.Generic;

    public class MyTask
    {
        private long startTime;
        private long endTime;

        public int Duration { get; private set; } // in ms
        public Action OnTimeReached { get; private set; }
        public bool Active { get; private set; }

        public MyTask(Action func, int duration)
        {
            startTime = CurrentMillis();
            endTime = startTime + duration;
            Duration = duration;
            Active = true;
            OnTimeReached = func;
        }

        public void Update()
        {
            if (Active && endTime <= CurrentMillis())
            {
                Active = false;
                OnTimeReached?.Invoke();
            }
        }

        private static long CurrentMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public static class MyTaskScheduler
    {
        private static readonly List<MyTask> ActiveTasks = new List<MyTask>();

        public static void Update()
        {
            foreach (var t in ActiveTasks)
            {
                if (t.Active)
                {
                    t.Update();
                }
            }
        }

        public static void AddTask(MyTask task)
        {
            ActiveTasks.Add(task);
        }
    }

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
            microStage = "stop";

            Console.WriteLine("Please run $select on the workstation");
            stage = "asking block";
        }
        
        public TradeSettings(string enchant, int level) {
            Enchant = enchant;
            Level = level;
            microStage = "stop";
            
            Console.WriteLine("Please run $select on the workstation");
            stage = "asking block";
        }
        
        public TradeSettings(string enchant, int level, int maxPrice) {
            Enchant = enchant;
            Level = level;
            MaxPrice = maxPrice;
            microStage = "stop";
            
            Console.WriteLine("Please run $select on the workstation");
            stage = "asking block";
        }

        public void begin() {
            going = true;
            microStage = "break";
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

        public void lookAtWork() {
            Onix.LocalPlayer.SetFacingXYZ(workBlock);

        }

    }
    public static class Globals {
        public static TradeSettings currentSearch;
        public static void CubeFrame(RendererWorld gfx,float x, float y, float z, float s)
        {
            // shift to center
            float half = s / 2f;
            float x0 = x - half, x1 = x + half;
            float y0 = y - half, y1 = y + half;
            float z0 = z - half, z1 = z + half;

            // bottom square
            gfx.DrawLine(new Vec3(x0, y0, z0), new Vec3(x1, y0, z0),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x1, y0, z0), new Vec3(x1, y1, z0),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x1, y1, z0), new Vec3(x0, y1, z0),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x0, y1, z0), new Vec3(x0, y0, z0),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x0, y0, z1), new Vec3(x1, y0, z1),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x1, y0, z1), new Vec3(x1, y1, z1),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x1, y1, z1), new Vec3(x0, y1, z1),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x0, y1, z1), new Vec3(x0, y0, z1),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x0, y0, z0), new Vec3(x0, y0, z1),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x1, y0, z0), new Vec3(x1, y0, z1),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x1, y1, z0), new Vec3(x1, y1, z1),ColorF.Cyan,ColorF.Cyan);
            gfx.DrawLine(new Vec3(x0, y1, z0), new Vec3(x0, y1, z1),ColorF.Cyan,ColorF.Cyan);
        }

        public static void exploreChildren(GameUIElement root,int tabbage) {
            foreach (GameUIElement child in root.Children) {
                
                Console.WriteLine($" {string.Concat(Enumerable.Repeat("  ",tabbage ))}Found {child.Name}");
                exploreChildren(child, tabbage+1);
                
            }
        }
        
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
            Onix.Events.Input.Input += OnKey;
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
            Onix.Events.Input.Input -= OnKey;
            
        }

        private bool OnKey(InputKey key, bool isDown) {
            if (key == InputKey.Type.P) {
                Globals.exploreChildren(Onix.Gui.RootUiElement,0);
            }
            return false;
        }

        private void OnTick() {
            // Console.WriteLine(Onix.Gui.ScreenName);
            MyTaskScheduler.Update();
            if (Globals.currentSearch != null) {
                if (Globals.currentSearch.going) {
                    switch (Globals.currentSearch.microStage) {
                        case "break":
                            if (Onix.Region.GetBlock(new BlockPos(Globals.currentSearch.workBlock)).Name == "lectern") {
                                Globals.currentSearch.lookAtWork();
                                Onix.LocalPlayer.BreakBlock();
                            } else {
                                
                                Globals.currentSearch.microStage = "place";
                            }
                            break;
                        case "place":
                            Onix.LocalPlayer.BuildBlock(new BlockPos((int)Globals.currentSearch.workBlock.X,
                                (int)Globals.currentSearch.workBlock.Y, (int)Globals.currentSearch.workBlock.Z), BlockFace.Bottom);
                            Globals.currentSearch.microStage = "check";

                            break;
                        case "check":
                            // Onix.LocalPlayer.Interact(Globals.currentSearch.villager);
                            if (Onix.Gui.ScreenName != "trade_screen") {
                                Onix.LocalPlayer.Interact(Globals.currentSearch.villager);
                            } else { // you are in the trade screen!, must get data then close it
                                
                                Onix.Gui.CloseCurrentScreen();
                            }
                            
                            Globals.currentSearch.microStage = "break";
                            
                            
                            break;
                    
                    }
                }
            }
            
        }

        private void OnHudRender(RendererCommon2D gfx, float delta) {
            // if (Globals.currentSearch != null) {
            //     gfx.RenderText(new Vec2(0,0),ColorF.White,Globals.currentSearch.going.ToString());
            //     gfx.RenderText(new Vec2(0,10),ColorF.White, Globals.currentSearch.microStage);    
            // }
            
            
            
            
            
        }
        
        private void OnHudRenderGame(RendererGame gfx, float delta) {
            
        }

        private void OnWorldRender(RendererWorld gfx, float delta) {
            if (Globals.currentSearch != null) {
                if (Globals.currentSearch.workBlock != null) {
                    Globals.CubeFrame(gfx,Globals.currentSearch.workBlock.X,Globals.currentSearch.workBlock.Y,Globals.currentSearch.workBlock.Z,1);
                }
            }
            
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