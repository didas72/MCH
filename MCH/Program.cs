using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DidasUtils.Logging;
using DidasUtils.Numerics;

using MCH.Render;
using MCH.Windows;

using Raylib_cs;

namespace MCH
{
    internal class Program
    {
        //Project plans:
        //PLANNED: [PRIORITY] Add Import button
        //PLANNED: [PRIORITY] Add full list for acceptable tags
        //PLANNED: Editor for factions
        //PLANNED: Editor for loot profiles
        //PLANNED: Editor for behaviour profiles
        //PLANNED: Editors for whatever is needed


        private static List<IRenderable> UI;
        public static readonly Stack<Holder> OpenPopups = new();
        private static Menu selectedMenu = Menu.Mod;
        private static Menu nextSelectedMenu = Menu.Mod;

        public static ModsWindow modW;
        public static SpawnGroupsWindow spawnGroupsW;
        public static PrefabsWindow prefabsW;



        private static void Main(string[] args)
        {
            try
            {
                if (!Init(args)) return;

                while (!Raylib.WindowShouldClose())
                {
                    try
                    {
                        Update();

                        Raylib.BeginDrawing();
                        Raylib.ClearBackground(Color.BLACK);
                        Draw();
                        Raylib.EndDrawing();
                    }
                    catch (Exception e)
                    {
                        Log.LogException("Unhandled exception.", "Main", e);
                        PopupUnhandledException(e);
                    }
                }

                Raylib.CloseWindow();
            }
            catch (Exception e)
            {
                Log.LogException("Fatal unhandled exception.", "Main", e);
                Environment.Exit(1);
            }
        }



        private static bool Init(string[] args)
        {
            Log.InitLog(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            if (!InitDirs()) return false;
            if (!InitWindow()) return false;
            if (!InitUI()) return false;

            EnableOnlyMenu(selectedMenu);

            PutWarnings();

            return true;
        }
        private static bool InitWindow()
        {
            try
            {
                Raylib.InitWindow(800, 600, "MCH");
                Raylib.SetTargetFPS(30);
                Raylib.SetExitKey(KeyboardKey.KEY_NULL);

                return true;
            }
            catch (Exception e)
            {
                Log.LogException("Exception initting window.", "Init", e);
                return false;
            }
        }
        private static bool InitUI()
        {
            UI = new List<IRenderable>();

            Holder pageHld = BuildMenuBar();

            modW = new(); pageHld.Add(modW.Window);
            spawnGroupsW = new(); pageHld.Add(spawnGroupsW.Window);
            prefabsW = new(); pageHld.Add(prefabsW.Window);

            return true;
        }
        private static bool InitDirs()
        {
            Directory.CreateDirectory(Globals.myDir);

            return true;
        }
        private static void PutWarnings()
        {
            PopupWarningMessage("Prototype version.\nMight be buggy and slow.");
        }



        private static void Update()
        {
            if (nextSelectedMenu != selectedMenu)
            { selectedMenu = nextSelectedMenu; EnableOnlyMenu(selectedMenu); }

            if (OpenPopups.Count == 0)
            {
                foreach (IRenderable rend in UI)
                {
                    if (rend is IUpdatable upd) upd.Update(Vector2i.Zero);
                }
            }
            else
                OpenPopups.Peek().Update(Vector2i.Zero);
        }
        private static void Draw()
        {
            foreach (IRenderable rend in UI)
            {
                rend.Render(Vector2i.Zero);
            }

            foreach (Holder holder in OpenPopups.Reverse())
                holder.Render(Vector2i.Zero);
        }



        #region Popups
        public static void ShowPopup(Holder pop) => OpenPopups.Push(pop);
        public static void PopupUnhandledException(Exception e)
        {
            Button btn;
            Holder pop = new(new(200, 175));
            pop.Add(new Panel(new(0, 0), new(400, 250), new Color(200, 0, 0, 255)));
            pop.Add(new TextBox("An unhandled exception occurred!", new(20, 12), new(360, 28)));
            pop.Add(new TextBox("Please contact the developer.", new(20, 42), new(360, 28)));
            pop.Add(new TextBox($"Exception type: {e.GetType().ToString().Replace("System.", string.Empty)}", new(20, 92), new(360, 28)));
            pop.Add(btn = new("Close", new(172, 212), new(56, 26))); btn.OnPressed = OnPopupClosePressed;

            OpenPopups.Push(pop);
        }
        public static void PopupErrorMessage(string message)
        {
            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(150, 50, 50, 255)));
            pop.Add(new TextBox("Error", new(10, 10), new(280, 20)));
            pop.Add(new TextBox(message, new(10, 35), new(280, 20)));
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed = OnPopupClosePressed;

            OpenPopups.Push(pop);
        }
        public static void PopupWarningMessage(string message)
        {
            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(250, 150, 50, 255)));
            pop.Add(new TextBox("Warning", new(10, 10), new(280, 20)));
            pop.Add(new TextBox(message, new(10, 35), new(280, 20)));
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed = OnPopupClosePressed;

            OpenPopups.Push(pop);
        }
        public static void PopupStatusMessage(string title, string message)
        {
            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(50, 50, 50, 255)));
            pop.Add(new TextBox(title, new(10, 10), new(280, 20)));
            pop.Add(new TextBox(message, new(10, 35), new(280, 20)));
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed = OnPopupClosePressed;

            OpenPopups.Push(pop);
        }
        public static void PopupChoiceMessage(string title, string opt1, string opt2, EventHandler<int> choiceCallback)
        {
            //0 = aborted
            //1 = first
            //2 = second

            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(50, 50, 50, 255)));
            pop.Add(new TextBox(title, new(10, 10), new(280, 20)));
            pop.Add(btn = new(opt1, new(10, 65), new(135, 20))); btn.OnPressed += (object sender, EventArgs _) => PopupChoicePressed(sender, 1, choiceCallback);
            pop.Add(btn = new(opt2, new(155, 65), new(135, 20))); btn.OnPressed += (object sender, EventArgs _) => PopupChoicePressed(sender, 2, choiceCallback);
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed += (object sender, EventArgs _) => PopupChoicePressed(sender, 0, choiceCallback);

            OpenPopups.Push(pop);
        }
        public static void OnPopupClosePressed(object sender, EventArgs e) => OpenPopups.Pop();
        public static void ClosePopup() => OpenPopups.Pop();
        public static Holder GetOpenPopup() => OpenPopups.Count != 0 ? OpenPopups.Peek() : null;


        private static void PopupChoicePressed(object sender, int code, EventHandler<int> callback)
        {
            ClosePopup();
            callback?.Invoke(sender, code);
        }
        #endregion



        #region Publics
        public static void SetOpenMenu(Menu menu)
        {
            //Delayed action
            nextSelectedMenu = menu;
        }
        #endregion



        #region Menus
        public enum Menu
        {
            Mod,
            SpawnGroups,
            Prefabs,
        }


        private static void EnableOnlyMenu(Menu menu)
        {
            nextSelectedMenu = menu;

            foreach (Holder hld in ((Holder)UI.First((IRenderable rend) => rend is Holder)).Children.Cast<Holder>())
            {
                hld.Enabled = menu.ToString() == hld.Id;
            }
        }


        private static void OnMenuModPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Mod;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuSpawnGroupsPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.SpawnGroups;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuPrefabsPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Prefabs;
            EnableOnlyMenu(selectedMenu);
        }


        private static void OnBugPressed(object sender, EventArgs e)
        {
            Process.Start("explorer", "https://github.com/didas72/MCH");
        }


        private static Holder BuildMenuBar()
        {
            Button btn; Holder hld;

            UI.Add(new Panel(Vector2i.Zero, new(800, 30), BaseWindow.Mid));

            UI.Add(btn = new("MOD", new(2, 2), new(88, 26))); btn.OnPressed += OnMenuModPressed;
            UI.Add(btn = new("SPWNGRP", new(92, 2), new(108, 26))); btn.OnPressed += OnMenuSpawnGroupsPressed;
            UI.Add(btn = new("PREFABS", new(202, 2), new(98, 26))); btn.OnPressed += OnMenuPrefabsPressed;
            //UI.Add(btn = new("", new(202, 2), new(98, 26))); btn.OnPressed += OnMenuSpawnGroupsPressed;

            //Help button
            UI.Add(btn = new("BUG", new(702, 2), new(96, 26)) { NormalColor = Color.RED }); btn.OnPressed += OnBugPressed;

            UI.Add(hld = new Holder(new(0, 30))); hld.Id_Name = "PAGE_HOLDER";

            return hld;
        }
        #endregion
    }
}