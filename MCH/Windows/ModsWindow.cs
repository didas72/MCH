using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using DidasUtils.Numerics;

using Raylib_cs;

using MCH.Data;
using MCH.Render;
using System.Diagnostics;

namespace MCH.Windows
{
    internal class ModsWindow : BaseWindow
    {
        private readonly TextList modList;
        private readonly InputField modNameFld;

        private readonly List<Mod> mods;



        public ModsWindow()
        {
            Window = new(Vector2i.Zero, "Mod") { Id_Name = "MOD" };

            Holder modListHld, modManageHld; Button btn;

            Window.Add(modListHld = new(Vector2i.Zero, "Mod List") { Id_Name = "MODLIST" });
            Window.Add(modManageHld = new(new(401, 0), "Mod Manage") { Id_Name = "MODMANAGE" });

            //divider
            Window.Add(new Panel(new(399, 0), new(2, 570), Mid));

            //Left side is mod list
            modListHld.Add(new TextBox("Existing mods:", 30, new(4, 1), new(350, 30), Highlights));
            modListHld.Add(modList = new(new(2, 30), new(395, 514))); modList.AllowSelect = true;
            modList.HighlightColor = Color.LIGHTGRAY; modList.SelectColor = Color.YELLOW;
            modList.OnSelected += TryLoadModSettingsUI;
            modListHld.Add(btn = new("Open", new(2, 548), new(96, 20))); btn.OnPressed += TryOpenMod;
            modListHld.Add(btn = new("Delete", new(102, 548), new(96, 20))); btn.OnPressed += TryDeleteMod;
            btn.NormalColor = Color.RED;

            //Right side is mod manage panel
            modManageHld.Add(new TextBox("Manage mods:", 30, new(4, 1), new(350, 34), Highlights));
            modManageHld.Add(new TextBox("Name:", 20, new(2, 42), new(200, 22), Text));
            modManageHld.Add(modNameFld = new(new(2, 66), new(300, 22)));
            modManageHld.Add(btn = new("Create", new(2, 90), new(96, 22))); btn.OnPressed += TryCreateMod;
            modManageHld.Add(btn = new("Update", new(102, 90), new(96, 22))); btn.OnPressed += TryUpdateMod;
            modManageHld.Add(btn = new("Export", new(301, 548), new(96, 20))); btn.OnPressed += TryExportMod;
            modManageHld.Add(btn = new("Mod dir", new(201, 548), new(96, 20))); btn.OnPressed += (object sender, EventArgs e) =>
                Process.Start("Explorer", Globals.ModDirectory);

            mods = new();
            RefreshModList();
        }



        #region Button handles
        private void TryCreateMod(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(modNameFld.Text) || mods.Any((Mod mod) => mod.Name == modNameFld.Text))
            {
                Program.PopupErrorMessage("Invalid/repeated mod name.");
                return;
            }

            AddMod(modNameFld.Text);
        }
        private void TryUpdateMod(object sender, EventArgs e)
        {
            if (modList.Selected == -1 || modList.Selected >= mods.Count)
            {
                Program.PopupErrorMessage("No/invalid mod selected.");
                return;
            }

            Mod sMod = mods[modList.Selected];

            if (string.IsNullOrWhiteSpace(modNameFld.Text) || mods.Any((Mod mod) => sMod != mod && mod.Name == modNameFld.Text))
            {
                Program.PopupErrorMessage("Invalid/repeated mod name.");
                return;
            }

            UpdateMod(sMod, modNameFld.Text);

            //FIXME: Renaming mods doesn't update name in Mods folder
        }
        private void TryDeleteMod(object sender, EventArgs e)
        {
            if (modList.Selected == -1 || modList.Selected >= mods.Count)
            {
                Program.PopupErrorMessage("No/invalid mod selected.");
                return;
            }

            Program.PopupChoiceMessage("Delete mod?", "Yes", "No", (object sender, int choice) => { if (choice == 1) DeleteMod(mods[modList.Selected]); });
        }
        private void TryOpenMod(object sender, EventArgs e)
        {
            if (modList.Selected == -1 || modList.Selected >= mods.Count)
            {
                Program.PopupErrorMessage("No/invalid mod selected.");
                return;
            }

            Globals.modId = mods[modList.Selected].internalId;
            Program.SetOpenMenu(Program.Menu.SpawnGroups);

            //ASNEEDED: Add other window refreshes
            Program.spawnGroupsW.RefreshAll();
            Program.prefabsW.RefreshAll();
        }
        private void TryLoadModSettingsUI(object sender, EventArgs e)
        {
            LoadModSettingsUI(mods[modList.Selected]);
        }
        private void TryExportMod(object sender, EventArgs e)
        {
            if (modList.Selected == -1 || modList.Selected >= mods.Count)
            {
                Program.PopupErrorMessage("No/invalid mod selected.");
                return;
            }

            Mod sMod = mods[modList.Selected];

            Program.PopupChoiceMessage("Export mod?\nWill overwrite old versions!", "Yes", "No", (object sender, int choice) => { if (choice == 1) ExportMod(sMod); });
        }
        #endregion



        private void AddMod(string name)
        {
            uint id;
            
            try
            {
                id = mods.Max((Mod mod) => mod.internalId) + 1;
            } 
            catch (InvalidOperationException)
            {
                id = 0;
            }

            mods.Add(new(id, name));

            FlushModList();
            ApplyUIModList();
            Directory.CreateDirectory(Path.Combine(Globals.myDir, id.ToString()));
            Directory.CreateDirectory(Path.Combine(Globals.myDir, id.ToString(), "Prefabs"));
        }
        private void UpdateMod(Mod mod, string name)
        {
            mod.Name = name;

            FlushModList();
            ApplyUIModList();
        }
        private void DeleteMod(Mod mod)
        {
            mods.Remove(mod);
            FlushModList();
            ApplyUIModList();

            //Delete all mod files
            Directory.Delete(Path.Combine(Globals.myDir, mod.internalId.ToString()), true);
        }
        private void LoadModSettingsUI(Mod mod)
        {
            modNameFld.Text = mod.Name;
        }
        private void ApplyUIModList()
        {
            modList.Lines.Clear();
            modList.Lines.Capacity = mods.Count;

            for (int i = 0; i < mods.Count; i++)
            {
                modList.Lines.Add(mods[i].Name);
            }
        }
        private void ExportMod(Mod mod)
        {
            //File structure copied from Corruption: PvE Combat by MeridiusX
            Globals.modId = mods[modList.Selected].internalId;
            Program.spawnGroupsW.RefreshAll();
            Program.prefabsW.RefreshAll();

            string modDir = Path.Combine(Globals.ModDirectory, mod.Name);
            string dataDir = Path.Combine(modDir, "Data");
            string prefabDir = Path.Combine(dataDir, "Prefabs");
            string spawnGroupsPath = Path.Combine(dataDir, "SpawnGroups.sbc");

            if (Directory.Exists(modDir))
                Directory.Delete(modDir, true);

            Directory.CreateDirectory(modDir);
            Directory.CreateDirectory(dataDir);

            Utils.CopyFilesRecursive(Path.Combine(Globals.myDir, mod.internalId.ToString(), "Prefabs"), prefabDir);

            //ASNEEDED: Remove other local mod files (or keep prefabs in own folder)
            File.Delete(Path.Combine(prefabDir, "mySpawnGroups.lst"));

            File.WriteAllText(spawnGroupsPath, SpawnGroup.BuildSpawnGroupsFile(Program.spawnGroupsW.GetAllSpawnGroups()));
        }



        #region File IO
        private void RefreshModList()
        {
            string path = Path.Combine(Globals.myDir, "myMods.lst");

            if (!File.Exists(path))
                File.Create(path).Dispose();

            string[] lines = File.ReadAllLines(path);

            mods.Clear();
            mods.Capacity = lines.Length;

            for (int i = 0; i < lines.Length; i++)
            {
                if (Mod.TryFromString(lines[i], out Mod mod))
                    mods.Add(mod);
            }

            ApplyUIModList();
        }
        private void FlushModList()
        {
            string path = Path.Combine(Globals.myDir, "myMods.lst");

            if (!File.Exists(path))
                File.Create(path).Dispose();

            string[] lines = new string[mods.Count];

            for (int i = 0; i < lines.Length; i++)
                lines[i] = mods[i].ToString();

            File.WriteAllLines(path, lines);
        }
        #endregion
    }
}
