using System;
using System.IO;
using System.Linq;

using DidasUtils.Numerics;

using Raylib_cs;

using MCH.Render;
using System.Diagnostics;

namespace MCH.Windows
{
    internal class PrefabsWindow : BaseWindow
    {
        private readonly TextList prefabList, exportSbcList;



        public PrefabsWindow()
        {
            Window = new(Vector2i.Zero, "Prefabs") { Id_Name = "PREFABS" };

            Holder prefabListHld, prefabManageHld/*, localHld, cloudHld*/; Button btn;

            //Holders
            Window.Add(prefabListHld = new(Vector2i.Zero, "Prefab List") { Id_Name = "PREFABLIST" });
            Window.Add(prefabManageHld = new(new(401, 0), "Prefab Manage") { Id_Name = "PREFABEDIT" });

            //Divider
            Window.Add(new Panel(new(399, 0), new(2, 570), Mid));

            //Left side is prefab list
            prefabListHld.Add(new TextBox("Added prefabs:", 30, new(4, 1), new(300, 30), Highlights));
            prefabListHld.Add(prefabList = new(new(2, 30), new(395, 514))); prefabList.AllowSelect = true;
            prefabList.HighlightColor = Color.LIGHTGRAY; prefabList.SelectColor = Color.YELLOW;
            prefabListHld.Add(btn = new("Remove", new(2, 548), new(96, 20))); btn.OnPressed += TryRemovePrefab;
            btn.NormalColor = Color.RED;

            //Right side is prefab manage panel
            prefabManageHld.Add(new TextBox("Add prefabs:", 30, new(4, 1), new(350, 30), Highlights));

            prefabManageHld.Add(new TextBox("Exported:", 20, new(2, 32), new(140, 22), Text));
            prefabManageHld.Add(exportSbcList = new(new(2, 58), new(395, 486))); exportSbcList.AllowSelect = true;
            exportSbcList.HighlightColor = Color.LIGHTGRAY; exportSbcList.SelectColor = Color.YELLOW;
            prefabManageHld.Add(btn = new("Add", new(2, 544), new(96, 20))); btn.OnPressed += TryAddExportPrefab;
            prefabManageHld.Add(btn = new("Refresh", new(102, 544), new(96, 20))); btn.OnPressed += TryRefreshUIExportList;
            prefabManageHld.Add(btn = new("Open Dir", new(202, 544), new(96, 20))); btn.OnPressed += (object sender, EventArgs e) => Process.Start("explorer", Globals.BP_exportDir);


            //List separators
            /*prefabManageHld.Add(new Panel(new(0, 32), new(499, 2), Mid));
            prefabManageHld.Add(new Panel(new(249, 34), new(2, 566), Mid));*/

            //Local prefabs
            /*prefabManageHld.Add(localHld = new(new(0, 34), "Locals") { Id_Name = "LOCALS" });
            localHld.Add(new TextBox("Local:", 20, new(2, 2), new(140, 22), Text));
            localHld.Add(localSbcList = new(new(2, 28), new(245, 486))); localSbcList.AllowSelect = true;
            localSbcList.HighlightColor = Color.LIGHTGRAY; localSbcList.SelectColor = Color.YELLOW;
            localHld.Add(btn = new("Add", new(2, 514), new(96, 20))); btn.OnPressed += TryAddLocalPrefab;
            localHld.Add(btn = new("Refresh", new(102, 514), new(96, 20))); btn.OnPressed += TryRefreshUILocalList;*/

            //Cloud prefabs
            /*prefabManageHld.Add(cloudHld = new(new(251, 34), "Cloud") { Id_Name = "CLOUD" });
            cloudHld.Add(new TextBox("Cloud:", 20, new(2, 2), new(140, 22), Text));
            cloudHld.Add(cloudSbcList = new(new(2, 28), new(245, 486))); cloudSbcList.AllowSelect = true;
            cloudSbcList.HighlightColor = Color.LIGHTGRAY; cloudSbcList.SelectColor = Color.YELLOW;
            cloudHld.Add(btn = new("Add", new(2, 514), new(96, 20))); btn.OnPressed += TryAddCloudPrefab;
            cloudHld.Add(btn = new("Refresh", new(102, 514), new(96, 20))); btn.OnPressed += TryRefreshUICloudList;*/
        }



        #region Publics
        public void RefreshAll()
        {
            prefabList.ClearSelection();
            /*localSbcList.ClearSelection();
            cloudSbcList.ClearSelection();*/
            exportSbcList.ClearSelection();
            /*RefreshUICloudList();
            RefreshUILocalList();*/
            RefreshUIExportList();
            RefreshUIPrefabsList(Globals.modId);
        }
        public string[] GetAllPrefabs()
        {
            return prefabList.Lines.ToArray();
        }
        #endregion



        #region Button handles
        /*private void TryAddLocalPrefab(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (localSbcList.Selected == -1 || localSbcList.Selected >= localSbcList.Lines.Count)
            {
                Program.PopupErrorMessage("No/invalid prefab selected.");
                return;
            }

            string name = localSbcList.Lines[localSbcList.Selected];

            if (prefabList.Lines.Any((string line) => line == name))
            {
                Program.PopupErrorMessage("Prefab already added.\nCheck update.");
                return;
            }

            CopyLocalPrefab(Globals.modId, name);
            RefreshUIPrefabsList(Globals.modId);
        }
        private void TryAddCloudPrefab(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (cloudSbcList.Selected == -1 || cloudSbcList.Selected >= cloudSbcList.Lines.Count)
            {
                Program.PopupErrorMessage("No/invalid prefab selected.");
                return;
            }

            string name = cloudSbcList.Lines[cloudSbcList.Selected];

            if (prefabList.Lines.Any((string line) => line == name))
            {
                Program.PopupErrorMessage("Prefab already added.\nCheck update.");
                return;
            }

            CopyCloudPrefab(Globals.modId, name);
            RefreshUIPrefabsList(Globals.modId);
        }*/
        /*private void TryRefreshUILocalList(object sender, EventArgs e)
        {
            RefreshUILocalList();
        }
        private void TryRefreshUICloudList(object sender, EventArgs e)
        {
            RefreshUICloudList();
        }*/
        private void TryAddExportPrefab(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (exportSbcList.Selected == -1 || exportSbcList.Selected >= exportSbcList.Lines.Count)
            {
                Program.PopupErrorMessage("No/invalid prefab selected.");
                return;
            }

            string name = exportSbcList.Lines[exportSbcList.Selected];

            if (prefabList.Lines.Any((string line) => line == name))
            {
                Program.PopupErrorMessage("Prefab already added.\nTry update.");
                return;
            }

            CopyExportPrefab(Globals.modId, name);
            RefreshUIPrefabsList(Globals.modId);
        }
        private void TryRefreshUIExportList(object sender, EventArgs e)
        {
            RefreshUIExportList();
        }
        private void TryRemovePrefab(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (prefabList.Selected == -1 || prefabList.Selected >= prefabList.Lines.Count)
            {
                Program.PopupErrorMessage("No/invalid prefab selected.");
                return;
            }

            Program.PopupChoiceMessage("Remove prefab?", "Yes", "No", (object sender, int choice) => { if (choice == 1) CheckRemovePrefab(prefabList.Lines[prefabList.Selected]); });
        }
        #endregion



        /*private void RefreshUILocalList()
        {
            localSbcList.Lines.Clear();

            foreach (string path in Directory.GetDirectories(Globals.BP_localDir))
                localSbcList.Lines.Add(Path.GetFileName(path));

            localSbcList.Lines.TrimExcess();

            localSbcList.ClearSelection();
        }
        private void RefreshUICloudList()
        {
            cloudSbcList.Lines.Clear();

            foreach (string path in Directory.GetDirectories(Globals.BP_cloudDir))
                cloudSbcList.Lines.Add(Path.GetFileName(path));

            cloudSbcList.Lines.TrimExcess();

            cloudSbcList.ClearSelection();
        }*/
        private void RefreshUIExportList()
        {
            exportSbcList.Lines.Clear();

            foreach (string path in Directory.GetFiles(Globals.BP_exportDir))
                exportSbcList.Lines.Add(Path.GetFileNameWithoutExtension(path));

            exportSbcList.Lines.TrimExcess();
            exportSbcList.ClearSelection();
        }
        private void RefreshUIPrefabsList(uint modId)
        {
            prefabList.Lines.Clear();

            string path = Path.Combine(Globals.myDir, modId.ToString(), "Prefabs");

            foreach (string lPath in Directory.GetFiles(path))
                prefabList.Lines.Add(Path.GetFileNameWithoutExtension(lPath));

            prefabList.Lines.TrimExcess();
        }
        private void CheckRemovePrefab(string name)
        {
            if (Program.spawnGroupsW.PeekRemovePrefab(name))
            {
                Program.PopupChoiceMessage("Prefab in use.\nStill remove?", "Yes", "No", (object sender, int choice) => { if (choice == 1) RemovePrefab(name); });
            }

            RemovePrefab(name);
        }
        private void RemovePrefab(string name)
        {
            RemovePrefabFile(Globals.modId, name);
            RefreshUIPrefabsList(Globals.modId);
            Program.spawnGroupsW.RemovePrefab(name);
        }



        #region File IO
        /*private void CopyLocalPrefab(uint modId, string name)
        {
            string tgtDir = Path.Combine(Globals.myDir, modId.ToString(), name);
            string srcDir = Path.Combine(Globals.BP_localDir, name);

            Utils.CopyFilesRecursive(srcDir, tgtDir);
        }
        private void CopyCloudPrefab(uint modId, string name)
        {
            string tgtDir = Path.Combine(Globals.myDir, modId.ToString(), name);
            string srcDir = Path.Combine(Globals.BP_cloudDir, name);

            Utils.CopyFilesRecursive(srcDir, tgtDir);
        }*/
        private void CopyExportPrefab(uint modId, string name)
        {
            string tgtPath = Path.Combine(Globals.myDir, modId.ToString(), "Prefabs", name+".sbc");
            string srcPath = Path.Combine(Globals.BP_exportDir, name+".sbc");

            File.Copy(srcPath, tgtPath);
        }
        private void RemovePrefabFile(uint modId, string name)
        {
            string path = Path.Combine(Globals.myDir, modId.ToString(), "Prefabs", name + ".sbc");
            File.Delete(path);
        }
        #endregion
    }
}
