using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using DidasUtils.Numerics;

using Raylib_cs;

using MCH.Data;
using MCH.Render;

namespace MCH.Windows
{
    internal class SpawnGroupsWindow : BaseWindow
    {
        private readonly TextList spawnGroupList, prefabList, tagList;
        private readonly InputField subtypeIdFld, frequencyFld;

        private readonly List<SpawnGroup> spawnGroups;



        public SpawnGroupsWindow()
        {
            Window = new(Vector2i.Zero, "SpawnGroups") { Id_Name = "SPAWNGROUPS" };

            Holder spwnGrpLstHld, spwnGrpEditHld, prefabsHld, tagsHld; Button btn;

            //Holders
            Window.Add(spwnGrpLstHld = new(Vector2i.Zero, "SpawnGroup List") { Id_Name = "SPAWNGROUPLIST" });
            Window.Add(spwnGrpEditHld = new(new(351, 0), "SpawnGroup Edit") { Id_Name = "SPAWNGROUPEDIT" });

            //Divider
            Window.Add(new Panel(new(349, 0), new(2, 570), Mid));

            //Left side is SpawnGroup list
            spwnGrpLstHld.Add(new TextBox("Existing spawngroups:", 30, new(4, 1), new(350, 30), Highlights));
            spwnGrpLstHld.Add(spawnGroupList = new(new(2, 30), new(345, 514))); spawnGroupList.AllowSelect = true;
            spawnGroupList.HighlightColor = Color.LIGHTGRAY; spawnGroupList.SelectColor = Color.YELLOW;
            spawnGroupList.OnSelected += TryLoadSpawnGroupSettingsUI;
            spwnGrpLstHld.Add(btn = new("Remove", new(2, 548), new(100, 20))); btn.OnPressed += TryDeleteSpawnGroup;
            btn.NormalColor = Color.RED;


            //Right side is SpawnGroup edit panel
            spwnGrpEditHld.Add(new TextBox("Edit spawn group:", 30, new(4, 1), new(350, 34), Highlights));
            spwnGrpEditHld.Add(new TextBox("SubtypeId (Name):", 20, new(2, 42), new(200, 22), Text));
            spwnGrpEditHld.Add(subtypeIdFld = new(new(2, 66), new(296, 22)));
            spwnGrpEditHld.Add(new TextBox("Frequency:", 20, new(2, 90), new(146, 22), Text));
            spwnGrpEditHld.Add(frequencyFld = new(new(152, 90), new(146, 22)));
            spwnGrpEditHld.Add(btn = new("Add", new(352, 90), new(96, 22))); btn.OnPressed += TryAddSpawnGroup;
            spwnGrpEditHld.Add(btn = new("Update", new(352, 66), new(96, 22))); btn.OnPressed += TryUpdateSpawnGroup;

            //List separators
            spwnGrpEditHld.Add(new Panel(new(0, 114), new(449, 2), Mid));
            spwnGrpEditHld.Add(new Panel(new(199, 116), new(2, 484), Mid));

            //Prefabs
            spwnGrpEditHld.Add(prefabsHld = new(new(0, 116), "Prefabs") { Id_Name = "PREFABS"});
            prefabsHld.Add(new TextBox("Prefabs:", 20, new(2, 2), new(140, 22), Text));
            prefabsHld.Add(prefabList = new(new(2, 28), new(195, 404))); prefabList.AllowSelect = true;
            prefabList.HighlightColor = Color.LIGHTGRAY; prefabList.SelectColor = Color.YELLOW;
            prefabsHld.Add(btn = new("Add", new(2, 434), new(96, 20))); btn.OnPressed += TryAddPrefab;
            prefabsHld.Add(btn = new("Remove", new(102, 434), new(96, 20))); btn.OnPressed += TryRemovePrefab;
            btn.NormalColor = Color.RED;

            //Tags
            spwnGrpEditHld.Add(tagsHld = new(new(201, 116), "Tags") { Id_Name = "TAGS" });
            tagsHld.Add(new TextBox("Tags:", 20, new(2, 2), new(140, 22), Text));
            tagsHld.Add(tagList = new(new(2, 28), new(245, 404))); tagList.AllowSelect = true;
            tagList.HighlightColor = Color.LIGHTGRAY; tagList.SelectColor = Color.YELLOW;
            tagsHld.Add(btn = new("Add", new(2, 434), new(66, 20))); btn.OnPressed += TryAddTag;
            tagsHld.Add(btn = new("Common", new(72, 434), new(86, 20))); btn.OnPressed += TryAddCommonTag;
            tagsHld.Add(btn = new("Remove", new(162, 434), new(86, 20))); btn.OnPressed += TryRemoveTag;
            btn.NormalColor = Color.RED;

            spawnGroups = new();
        }



        #region Publics
        public void RefreshAll()
        {
            spawnGroupList.ClearSelection();
            prefabList.ClearSelection();
            tagList.ClearSelection();
            subtypeIdFld.Text = string.Empty;
            frequencyFld.Text = string.Empty;
            prefabList.Lines.Clear();
            tagList.Lines.Clear();
            RefreshSpawnGroupList(Globals.modId);
        }
        public bool PeekRemovePrefab(string subtypeId)
        {
            return spawnGroups.Any((SpawnGroup sbc) => sbc.Prefabs.Any((Prefab pre) => pre.SubtypeId == subtypeId));
        }
        public void RemovePrefab(string subtypeId)
        {
            spawnGroups.ForEach((SpawnGroup sbc) => sbc.Prefabs.RemoveAll((Prefab pre) => pre.SubtypeId == subtypeId));
        }
        public SpawnGroup[] GetAllSpawnGroups()
        {
            return spawnGroups.ToArray();
        }
        #endregion



        #region Button handles
        private void TryAddSpawnGroup(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(subtypeIdFld.Text) || spawnGroups.Any((SpawnGroup sbc) => sbc.SubtypeId == subtypeIdFld.Text))
            {
                Program.PopupErrorMessage("Invalid/repeated spawn\ngroup name.");
                return;
            }
            if (subtypeIdFld.Text.Contains(' '))
            {
                Program.PopupErrorMessage("Spawn group name has spaces.");
                return;
            }
            if (string.IsNullOrWhiteSpace(frequencyFld.Text) || !double.TryParse(frequencyFld.Text, out double freq))
            {
                Program.PopupErrorMessage("Invalid/empty spawn group\nfrequency.");
                return;
            }

            AddSpawnGroup(subtypeIdFld.Text, freq);
        }
        private void TryUpdateSpawnGroup(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (spawnGroupList.Selected == -1 || spawnGroupList.Selected >= spawnGroups.Count)
            {
                Program.PopupErrorMessage("No/invalid spawn group selected.");
                return;
            }

            SpawnGroup sSbc = spawnGroups[spawnGroupList.Selected];

            if (string.IsNullOrWhiteSpace(subtypeIdFld.Text) || spawnGroups.Any((SpawnGroup sbc) => sSbc != sbc && sbc.SubtypeId == subtypeIdFld.Text))
            {
                Program.PopupErrorMessage("Invalid/repeated spawn\ngroup name.");
                return;
            }
            if (string.IsNullOrWhiteSpace(frequencyFld.Text) || !double.TryParse(frequencyFld.Text, out double freq))
            {
                Program.PopupErrorMessage("Invalid/empty spawn group\nfrequency.");
                return;
            }

            UpdateSpawnGroup(sSbc, subtypeIdFld.Text, freq);
        }
        private void TryDeleteSpawnGroup(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (spawnGroupList.Selected == -1 || spawnGroupList.Selected >= spawnGroups.Count)
            {
                Program.PopupErrorMessage("No/invalid spawn group selected.");
                return;
            }

            Program.PopupChoiceMessage("Delete spawn group?", "Yes", "No", (object sender, int choice) => { if (choice == 1) DeleteSpawnGroup(spawnGroups[spawnGroupList.Selected]); });
        }
        private void TryLoadSpawnGroupSettingsUI(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            SpawnGroup sbc = spawnGroups[spawnGroupList.Selected];

            prefabList.ClearSelection();
            tagList.ClearSelection();

            LoadSpawnGroupSettingsUI(sbc);
            ApplyUIPrefabList(sbc);
            ApplyUITagList(sbc);
        }
        private void TryAddPrefab(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (spawnGroupList.Selected == -1 || spawnGroupList.Selected >= spawnGroups.Count)
            {
                Program.PopupErrorMessage("No/invalid spawn group selected.");
                return;
            }

            SpawnGroup spawnGroup = spawnGroups[spawnGroupList.Selected];

            Holder pop = new(new(150, 150)); Button btn; InputField xFld, yFld, zFld, speedFld, behaviourFld; TextList lst;

            pop.Add(new Panel(new(0, 0), new(500, 300), new Color(50, 50, 50, 255)));

            pop.Add(lst = new(new(20, 20), new(460, 150))); lst.AllowSelect = true;
            lst.HighlightColor = Color.LIGHTGRAY; lst.SelectColor = Color.YELLOW;
            lst.Lines.AddRange(Program.prefabsW.GetAllPrefabs());

            pop.Add(new TextBox("x", 20, new(20, 170), new(50, 20), Text));
            pop.Add(xFld = new(new(20, 192), new(96, 20))); xFld.Text = "0.0";
            pop.Add(new TextBox("y", 20, new(120, 170), new(50, 20), Text));
            pop.Add(yFld = new(new(120, 192), new(96, 20))); yFld.Text = "0.0";
            pop.Add(new TextBox("z", 20, new(220, 170), new(50, 20), Text));
            pop.Add(zFld = new(new(220, 192), new(96, 20))); zFld.Text = "0.0";

            pop.Add(new TextBox("Speed", 20, new(20, 214), new(150, 20), Text));
            pop.Add(speedFld = new(new(20, 236), new(96, 20))); speedFld.Text = "25.0";

            pop.Add(new TextBox("Behaviour (unchecked)", 20, new(120, 214), new(250, 20), Text));
            pop.Add(behaviourFld = new(new(120, 236), new(196, 20)));

            pop.Add(btn = new("Add", new(20, 264), new(96, 20))); btn.OnPressed += (object sender, EventArgs e) =>
                AddPrefab_Add(spawnGroup, lst.Lines, lst.Selected, xFld.Text, yFld.Text, zFld.Text, speedFld.Text, behaviourFld.Text);
            pop.Add(btn = new("Cancel", new(120, 264), new(96, 20))); btn.OnPressed += (object sender, EventArgs e) => Program.ClosePopup();

            Program.ShowPopup(pop);
        }
        private void TryRemovePrefab(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (spawnGroupList.Selected == -1 || spawnGroupList.Selected >= spawnGroups.Count)
            {
                Program.PopupErrorMessage("No/invalid spawn group selected.");
                return;
            }
            if (prefabList.Selected == -1 || prefabList.Selected >= prefabList.Lines.Count)
            {
                Program.PopupErrorMessage("No/invalid prefab selected.");
                return;
            }

            SpawnGroup sbc = spawnGroups[spawnGroupList.Selected];

            Program.PopupChoiceMessage("Remove prefab?", "Yes", "No", (object sender, int choice) => { if (choice == 1) RemovePrefabFromSbc(sbc, prefabList.Selected); });
        }
        private void TryAddTag(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (spawnGroupList.Selected == -1 || spawnGroupList.Selected >= spawnGroups.Count)
            {
                Program.PopupErrorMessage("No/invalid spawn group selected.");
                return;
            }

            SpawnGroup spawnGroup = spawnGroups[spawnGroupList.Selected];

            Holder pop = new(new(230, 220)); Button btn; InputField tagFld, valueFld;

            pop.Add(new Panel(new(0, 0), new(340, 160), new Color(50, 50, 50, 255)));

            pop.Add(new TextBox("Tag:", 20, new(20, 20), new(100, 20), Text));
            pop.Add(tagFld = new(new(20, 42), new(148, 20)));
            pop.Add(new TextBox("Value:", 20, new(172, 20), new(100, 20), Text));
            pop.Add(valueFld = new(new(172, 42), new(148, 20)));

            pop.Add(btn = new("Add", new(20, 120), new(98, 20))); btn.OnPressed += (object sender, EventArgs e) =>
                AddTag_Add(spawnGroup, tagFld.Text, valueFld.Text);
            pop.Add(btn = new("Cancel", new(222, 120), new(98, 20))); btn.OnPressed += (object sender, EventArgs e) => Program.ClosePopup();

            Program.ShowPopup(pop);
        }
        private void TryAddCommonTag(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (spawnGroupList.Selected == -1 || spawnGroupList.Selected >= spawnGroups.Count)
            {
                Program.PopupErrorMessage("No/invalid spawn group selected.");
                return;
            }

            SpawnGroup spawnGroup = spawnGroups[spawnGroupList.Selected];

            string[] commons = File.ReadAllLines("./CommonTags.txt");
            List<string> commonValues = new(), commonTags = new();

            for (int i = 0; i < commons.Length; i++)
            {
                if (commons[i].StartsWith("//"))
                    continue;

                commonValues.Add(commons[i].Split(':')[1]);
                commonTags.Add(commons[i].Split(':')[0]);
            }

            Holder pop = new(new(180, 150)); Button btn; InputField valueFld; TextList tagLst;

            pop.Add(new Panel(new(0, 0), new(440, 300), new Color(50, 50, 50, 255)));

            pop.Add(new TextBox("Tag:", 20, new(20, 20), new(96, 20), Text));
            pop.Add(tagLst = new(new(20, 42), new(400, 174))); tagLst.Lines.AddRange(commonTags); tagLst.AllowSelect = true;
            tagLst.HighlightColor = Color.LIGHTGRAY; tagLst.SelectColor = Color.YELLOW;

            pop.Add(new TextBox("Value (unchecked):", 20, new(20, 216), new(400, 20), Text));
            pop.Add(valueFld = new(new(20, 238), new(400, 20)));
            pop.Add(btn = new("Add", new(20, 260), new(98, 20))); btn.OnPressed += (object sender, EventArgs e) =>
                AddCommonTag_Add(spawnGroup, tagLst.Lines, tagLst.Selected, valueFld.Text);
            pop.Add(btn = new("Cancel", new(322, 260), new(98, 20))); btn.OnPressed += (object sender, EventArgs e) => Program.ClosePopup();

            tagLst.OnSelected += (object sender, EventArgs e) => { valueFld.Text = commonValues[tagLst.Selected]; };

            Program.ShowPopup(pop);
        }
        private void TryRemoveTag(object sender, EventArgs e)
        {
            if (Globals.NoModSelected())
            {
                Program.PopupErrorMessage("No mod selected.");
                return;
            }

            if (spawnGroupList.Selected == -1 || spawnGroupList.Selected >= spawnGroups.Count)
            {
                Program.PopupErrorMessage("No/invalid spawn group selected.");
                return;
            }
            if (tagList.Selected == -1 || tagList.Selected >= tagList.Lines.Count)
            {
                Program.PopupErrorMessage("No/invalid tag selected.");
                return;
            }

            SpawnGroup sbc = spawnGroups[spawnGroupList.Selected];

            Program.PopupChoiceMessage("Remove tag?", "Yes", "No", (object sender, int choice) => { if (choice == 1) RemoveTagFromSbc(sbc, tagList.Selected); });
        }

        private void AddPrefab_Add(SpawnGroup sbc, List<string> prefabs, int selIndex, string x, string y, string z, string spd, string behaviour)
        {
            if(selIndex == -1 || selIndex > prefabs.Count)
            {
                Program.PopupErrorMessage("No/invalid prefab selected.");
                return;
            }
            if (!double.TryParse(x, out double X) || !double.TryParse(y, out double Y) || !double.TryParse(z, out double Z))
            {
                Program.PopupErrorMessage("Invalid coordinates.");
                return;
            }
            if (!double.TryParse(spd, out double speed))
            {
                Program.PopupErrorMessage("Invalid speed.");
                return;
            }

            AddPrefabToSbc(sbc, prefabs[selIndex], X, Y, Z, speed, behaviour);

            Program.ClosePopup();
        }
        private void AddTag_Add(SpawnGroup sbc, string tag, string value)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                Program.PopupErrorMessage("No/empty tag.");
                return;
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                Program.PopupErrorMessage("No/empty value.");
                return;
            }

            AddTagToSbc(sbc, tag, value);

            Program.ClosePopup();
        }
        private void AddCommonTag_Add(SpawnGroup sbc, List<string> tags, int selIndex, string value)
        {
            if (selIndex == -1 || selIndex >= tags.Count)
            {
                Program.PopupErrorMessage("No/invalid tag selected.");
                return;
            }

            string tag = tags[selIndex];

            if (string.IsNullOrWhiteSpace(value))
            {
                Program.PopupErrorMessage("No/empty value.");
                return;
            }

            AddTagToSbc(sbc, tag, value);

            Program.ClosePopup();
        }
        #endregion



        private void AddSpawnGroup(string subtypeId, double frequency)
        {
            uint id;

            try
            {
                id = spawnGroups.Max((SpawnGroup sbc) => sbc.internalId) + 1;
            }
            catch (InvalidOperationException)
            {
                id = 0;
            }

            spawnGroups.Add(new(id, subtypeId, frequency));
            FlushSpawnGroupList(Globals.modId);
            ApplyUISbcList();
            spawnGroupList.Selected = spawnGroupList.Lines.Count - 1;
        }
        private void UpdateSpawnGroup(SpawnGroup sbc, string subtypeId, double frequency)
        {
            sbc.SubtypeId = subtypeId;
            sbc.Frequency = frequency;
        }
        private void DeleteSpawnGroup(SpawnGroup sbc)
        {
            spawnGroups.Remove(sbc);
            FlushSpawnGroupList(Globals.modId);
            ApplyUISbcList();
        }
        private void LoadSpawnGroupSettingsUI(SpawnGroup sbc)
        {
            subtypeIdFld.Text = sbc.SubtypeId;
            frequencyFld.Text = sbc.Frequency.ToString();
        }
        private void ApplyUIPrefabList(SpawnGroup sbc)
        {
            prefabList.Lines.Clear();
            prefabList.Lines.Capacity = sbc.Prefabs.Count;

            for (int i = 0; i < sbc.Prefabs.Count; i++)
                prefabList.Lines.Add(sbc.Prefabs[i].SubtypeId);
        }
        private void ApplyUITagList(SpawnGroup sbc)
        {
            tagList.Lines.Clear();
            tagList.Lines.Capacity = sbc.DescriptionTags.Count;

            for (int i = 0; i < sbc.DescriptionTags.Count; i++)
                tagList.Lines.Add(sbc.DescriptionTags[i].ToString());
        }
        private void ApplyUISbcList()
        {
            spawnGroupList.Lines.Clear();
            spawnGroupList.Lines.Capacity = spawnGroups.Count;

            for (int i = 0; i < spawnGroups.Count; i++)
            {
                spawnGroupList.Lines.Add(spawnGroups[i].SubtypeId);
            }
        }
        private void AddPrefabToSbc(SpawnGroup sbc, string subtypeId, double x, double y, double z, double speed, string behaviour)
        {
            Prefab prefab = new(subtypeId, x, y, z, speed, behaviour);
            sbc.Prefabs.Add(prefab);

            FlushSpawnGroupList(Globals.modId);
            ApplyUIPrefabList(sbc);
        }
        private void RemovePrefabFromSbc(SpawnGroup sbc, int prefabIndex)
        {
            sbc.Prefabs.RemoveAt(prefabIndex);

            FlushSpawnGroupList(Globals.modId);
            ApplyUIPrefabList(sbc);
        }
        private void AddTagToSbc(SpawnGroup sbc, string tag, string value)
        {
            DescriptionTag lTag = new(tag, value);
            sbc.DescriptionTags.Add(lTag);

            FlushSpawnGroupList(Globals.modId);
            ApplyUITagList(sbc);
        }
        private void RemoveTagFromSbc(SpawnGroup sbc, int tagIndex)
        {
            sbc.DescriptionTags.RemoveAt(tagIndex);

            FlushSpawnGroupList(Globals.modId);
            ApplyUITagList(sbc);
        }



        #region File IO
        private void RefreshSpawnGroupList(uint modId)
        {
            string path = Path.Combine(Globals.myDir, modId.ToString(),  $"mySpawnGroups.lst");

            if (!File.Exists(path))
                File.Create(path).Dispose();

            string[] lines = File.ReadAllLines(path);

            spawnGroups.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                if (SpawnGroup.TryFromString(lines[i], out SpawnGroup spawnGroup))
                    spawnGroups.Add(spawnGroup);
            }
            spawnGroups.TrimExcess();

            ApplyUISbcList();
        }
        private void FlushSpawnGroupList(uint modId)
        {
            string path = Path.Combine(Globals.myDir, modId.ToString(), $"mySpawnGroups.lst");

            if (!File.Exists(path))
                File.Create(path).Dispose();

            string[] lines = new string[spawnGroups.Count];

            for (int i = 0; i < lines.Length; i++)
                lines[i] = spawnGroups[i].ToString();

            File.WriteAllLines(path, lines);
        }
        #endregion
    }
}
