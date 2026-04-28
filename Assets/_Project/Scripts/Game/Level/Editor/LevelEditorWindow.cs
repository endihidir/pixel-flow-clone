using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Game.Level.Data;

namespace Game.Level.EditorTools
{
    public sealed class LevelEditorWindow : EditorWindow
    {
        private const int LaneMin = 3;
        private const int LaneMax = 5;

        private enum Tab { Generate, Load, Edit }
        private enum SaveMode { NewFile, Overwrite }

        // Shared
        private LevelEditorConfigSO _config;
        private Tab _tab = Tab.Generate;

        // Generate tab
        private List<Texture2D> _pngs = new();
        private int _laneCount = 3;
        private int _difficultyIndex = 1;
        private string _fileNamePrefix = "level";
        private int _startIndex = 1;
        private Vector2 _genScroll;
        private string _lastReport;
        private bool _laneCountInitialized;

        // Load tab
        private TextAsset _loadJson;

        // Edit tab
        private LevelDefinition _editLevel;
        private string _editSourcePath;          // for Overwrite mode
        private SaveMode _editSaveMode;
        private Vector2 _editScroll;
        private int _selectedLane = -1;
        private int _selectedPig = -1;
        private string _editStatus;

        [MenuItem("Game/Level/Level Editor")]
        public static void Open()
        {
            var w = GetWindow<LevelEditorWindow>("Level Editor");
            w.minSize = new Vector2(520, 480);
            w.Show();
        }

        private void OnGUI()
        {
            DrawConfigField();
            if (_config == null) return;

            EnsureLaneInit();
            DrawTabs();

            EditorGUILayout.Space();
            switch (_tab)
            {
                case Tab.Generate: DrawGenerateTab(); break;
                case Tab.Load:     DrawLoadTab();     break;
                case Tab.Edit:     DrawEditTab();     break;
            }
        }

        // ---------- Common ----------

        private void DrawConfigField()
        {
            _config = (LevelEditorConfigSO)EditorGUILayout.ObjectField(
                "Editor Config", _config, typeof(LevelEditorConfigSO), false);
            if (_config == null)
                EditorGUILayout.HelpBox("Assign a LevelEditorConfigSO to begin.", MessageType.Info);
        }

        private void EnsureLaneInit()
        {
            if (_laneCountInitialized) return;
            _laneCount = Mathf.Clamp(_config.DefaultLaneCount, LaneMin, LaneMax);
            _laneCountInitialized = true;
        }

        private void DrawTabs()
        {
            EditorGUILayout.Space();
            int idx = (int)_tab;
            int newIdx = GUILayout.Toolbar(idx, new[] { "Generate", "Load", "Edit" });
            if (newIdx != idx) _tab = (Tab)newIdx;
        }

        // ---------- Generate Tab ----------

        private void DrawGenerateTab()
        {
            EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
            _laneCount = EditorGUILayout.IntSlider("Lane Count", _laneCount, LaneMin, LaneMax);

            var presets = _config.DifficultyPresets;
            if (presets != null && presets.Length > 0)
            {
                var names = new string[presets.Length];
                for (int i = 0; i < presets.Length; i++)
                    names[i] = string.IsNullOrEmpty(presets[i].Name) ? $"Preset {i}" : presets[i].Name;
                _difficultyIndex = EditorGUILayout.Popup("Difficulty", _difficultyIndex, names);
            }
            else
            {
                EditorGUILayout.HelpBox("No DifficultyPresets configured.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            _fileNamePrefix = EditorGUILayout.TextField("File Prefix", _fileNamePrefix);
            _startIndex = EditorGUILayout.IntField("Start Index", _startIndex);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Source PNGs", EditorStyles.boldLabel);

            _genScroll = EditorGUILayout.BeginScrollView(_genScroll, GUILayout.Height(140));
            for (int i = 0; i < _pngs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                Object current = _pngs[i];
                var dropped = EditorGUILayout.ObjectField(current, typeof(Object), false);
                if (dropped != null && !(dropped is Texture2D) && !(dropped is Sprite))
                    dropped = current;
                _pngs[i] = ResolveTexture(dropped);
                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    _pngs.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Slot")) _pngs.Add(null);
            if (GUILayout.Button("Use Selected"))
            {
                _pngs.Clear();
                foreach (var o in Selection.objects)
                {
                    var t = ResolveTexture(o);
                    if (t != null) _pngs.Add(t);
                }
            }
            if (GUILayout.Button("Clear")) _pngs.Clear();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(_pngs.Count == 0))
            {
                if (GUILayout.Button("Import All", GUILayout.Height(28)))
                    ImportAll();
            }

            // Single-PNG quick action: import + open in Edit.
            using (new EditorGUI.DisabledScope(_pngs.Count != 1 || _pngs[0] == null))
            {
                if (GUILayout.Button("Import & Edit (single)", GUILayout.Height(22)))
                    ImportAndEditSingle();
            }

            if (!string.IsNullOrEmpty(_lastReport))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Last Run", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(_lastReport, MessageType.None);
            }
        }

        private void ImportAll()
        {
            var sb = new System.Text.StringBuilder();
            int idx = _startIndex;
            for (int i = 0; i < _pngs.Count; i++)
            {
                var png = _pngs[i];
                if (png == null) continue;
                string outName = $"{_fileNamePrefix}_{idx}";
                var result = LevelImporter.Import(png, _laneCount, _difficultyIndex, _config, outName);
                if (result.Success)
                    sb.AppendLine($"OK  {png.name} -> {outName}.json (attempts: {result.Attempts})");
                else
                    sb.AppendLine($"FAIL  {png.name}: {result.Error}");
                idx++;
            }
            _lastReport = sb.ToString();
            Repaint();
        }

        private void ImportAndEditSingle()
        {
            var png = _pngs[0];
            string outName = $"{_fileNamePrefix}_{_startIndex}";
            var result = LevelImporter.Import(png, _laneCount, _difficultyIndex, _config, outName);
            if (!result.Success)
            {
                _lastReport = $"FAIL  {png.name}: {result.Error}";
                return;
            }
            _editLevel = result.Level;
            _editSourcePath = result.OutputPath;
            _editSaveMode = SaveMode.NewFile; // generate output is a fresh file; subsequent saves overwrite this path
            _selectedLane = -1;
            _selectedPig = -1;
            _editStatus = $"Generated from {png.name}.";
            _tab = Tab.Edit;
            Repaint();
        }

        // ---------- Load Tab ----------

        private void DrawLoadTab()
        {
            EditorGUILayout.LabelField("Load Existing JSON", EditorStyles.boldLabel);
            _loadJson = (TextAsset)EditorGUILayout.ObjectField(
                "Level JSON", _loadJson, typeof(TextAsset), false);

            using (new EditorGUI.DisabledScope(_loadJson == null))
            {
                if (GUILayout.Button("Open in Editor", GUILayout.Height(28)))
                    LoadFromAsset(_loadJson);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Or pick by path", EditorStyles.boldLabel);
            if (GUILayout.Button("Browse..."))
            {
                string path = EditorUtility.OpenFilePanel("Open Level JSON", _config.OutputFolder, "json");
                if (!string.IsNullOrEmpty(path)) LoadFromPath(path);
            }
        }

        private void LoadFromAsset(TextAsset asset)
        {
            try
            {
                _editLevel = JsonUtility.FromJson<LevelDefinition>(asset.text);
                _editSourcePath = AssetDatabase.GetAssetPath(asset);
                _editSaveMode = SaveMode.Overwrite;
                _selectedLane = -1;
                _selectedPig = -1;
                _editStatus = $"Loaded {asset.name}.";
                _tab = Tab.Edit;
                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Load Error", e.Message, "OK");
            }
        }

        private void LoadFromPath(string absolutePath)
        {
            try
            {
                var text = File.ReadAllText(absolutePath);
                _editLevel = JsonUtility.FromJson<LevelDefinition>(text);
                _editSourcePath = absolutePath;
                _editSaveMode = SaveMode.Overwrite;
                _selectedLane = -1;
                _selectedPig = -1;
                _editStatus = $"Loaded {Path.GetFileName(absolutePath)}.";
                _tab = Tab.Edit;
                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Load Error", e.Message, "OK");
            }
        }

        // ---------- Edit Tab ----------

        private void DrawEditTab()
        {
            if (_editLevel == null)
            {
                EditorGUILayout.HelpBox("No level loaded. Use the Generate or Load tab first.", MessageType.Info);
                return;
            }

            // Toolbar
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Size: {_editLevel.Width}x{_editLevel.Height}    Lanes: {_editLevel.Lanes?.Length ?? 0}",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Validate", GUILayout.Width(80))) RunValidate();
            if (GUILayout.Button("Regenerate Lanes", GUILayout.Width(140))) RegenerateLanes();
            if (GUILayout.Button("Save", GUILayout.Width(80))) SaveEdit();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_editStatus))
                EditorGUILayout.HelpBox(_editStatus, MessageType.None);

            _editScroll = EditorGUILayout.BeginScrollView(_editScroll);

            DrawBoardEditor();
            EditorGUILayout.Space();
            DrawLaneEditor();
            EditorGUILayout.Space();
            DrawPigDetailPanel();

            EditorGUILayout.EndScrollView();
        }

        private void DrawBoardEditor()
        {
            EditorGUILayout.LabelField("Board (click to set selected color, right-click to erase)", EditorStyles.boldLabel);

            int w = _editLevel.Width;
            int h = _editLevel.Height;
            if (w == 0 || h == 0) return;

            // Available width inside the scroll view; clamp height to a reasonable max.
            float availW = EditorGUIUtility.currentViewWidth - 40f;
            float maxH = 500f;

            float cellByW = availW / w;
            float cellByH = maxH / h;
            float cell = Mathf.Floor(Mathf.Min(cellByW, cellByH));
            cell = Mathf.Max(4f, cell);

            Rect rect = GUILayoutUtility.GetRect(w * cell, h * cell);
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));

            var palette = _config.Palette;
            var e = Event.current;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int flat = y * w + x;
                    var id = _editLevel.Cubes[flat];
                    var cellRect = new Rect(rect.x + x * cell, rect.y + y * cell, cell - 1, cell - 1);
                    var color = id == ColorId.None
                        ? new Color(0.18f, 0.10f, 0.25f)   // dark purple, distinct from Black palette color
                        : (palette != null ? palette.GetColor(id) : Color.magenta);
                    EditorGUI.DrawRect(cellRect, color);

                    if (e.type == EventType.MouseDown && cellRect.Contains(e.mousePosition))
                    {
                        if (e.button == 1)
                        {
                            _editLevel.Cubes[flat] = ColorId.None;
                            _editStatus = "Board changed. Run Validate or Regenerate Lanes.";
                            GUI.changed = true;
                            e.Use();
                            Repaint();
                        }
                        else if (e.button == 0 && _selectedColor != ColorId.None)
                        {
                            _editLevel.Cubes[flat] = _selectedColor;
                            _editStatus = "Board changed. Run Validate or Regenerate Lanes.";
                            GUI.changed = true;
                            e.Use();
                            Repaint();
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selected Color (left click to paint)", EditorStyles.miniBoldLabel);
            DrawColorPicker();
        }

        private ColorId _selectedColor = ColorId.Red;

        private void DrawColorPicker()
        {
            var palette = _config.Palette;
            if (palette == null || palette.Entries == null) return;

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < palette.Entries.Length; i++)
            {
                var entry = palette.Entries[i];
                if (entry.Id == ColorId.None) continue;
                var bg = GUI.backgroundColor;
                GUI.backgroundColor = entry.Color;
                bool selected = _selectedColor == entry.Id;
                var style = selected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;
                if (GUILayout.Button(selected ? "*" : " ", style, GUILayout.Width(28), GUILayout.Height(22)))
                    _selectedColor = entry.Id;
                GUI.backgroundColor = bg;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"Painting: {_selectedColor}", EditorStyles.miniLabel);
        }

        private void DrawLaneEditor()
        {
            EditorGUILayout.LabelField("Lanes (click pig to select)", EditorStyles.boldLabel);
            var palette = _config.Palette;
            var lanes = _editLevel.Lanes;
            if (lanes == null) return;

            EditorGUILayout.BeginHorizontal();
            for (int li = 0; li < lanes.Length; li++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(76));
                EditorGUILayout.LabelField($"Lane {li}", EditorStyles.miniBoldLabel);
                var pigs = lanes[li].Pigs;
                if (pigs != null)
                {
                    for (int pi = 0; pi < pigs.Length; pi++)
                    {
                        var pig = pigs[pi];
                        var bg = GUI.backgroundColor;
                        GUI.backgroundColor = palette != null ? palette.GetColor(pig.Color) : Color.gray;
                        bool isSel = (li == _selectedLane && pi == _selectedPig);
                        string label = (isSel ? ">" : "") + pig.Ammo.ToString();
                        if (GUILayout.Button(label, GUILayout.Width(60), GUILayout.Height(22)))
                        {
                            _selectedLane = li;
                            _selectedPig = pi;
                        }
                        GUI.backgroundColor = bg;
                    }
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+", GUILayout.Width(28))) AddPig(li);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPigDetailPanel()
        {
            EditorGUILayout.LabelField("Selected Pig", EditorStyles.boldLabel);
            if (_selectedLane < 0 || _selectedPig < 0
                || _selectedLane >= _editLevel.Lanes.Length
                || _selectedPig >= _editLevel.Lanes[_selectedLane].Pigs.Length)
            {
                EditorGUILayout.HelpBox("No pig selected.", MessageType.None);
                return;
            }

            var pigs = _editLevel.Lanes[_selectedLane].Pigs;
            var pig = pigs[_selectedPig];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Lane {_selectedLane}, Slot {_selectedPig}", GUILayout.Width(160));
            pig.Color = (ColorId)EditorGUILayout.EnumPopup("Color", pig.Color);
            EditorGUILayout.EndHorizontal();

            pig.Ammo = EditorGUILayout.IntSlider("Ammo", pig.Ammo,
                _config.MinAmmoPerPig, _config.MaxAmmoPerPig);

            pigs[_selectedPig] = pig;

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_selectedPig <= 0))
                if (GUILayout.Button("Move Up")) MovePigInLane(_selectedLane, _selectedPig, -1);
            using (new EditorGUI.DisabledScope(_selectedPig >= pigs.Length - 1))
                if (GUILayout.Button("Move Down")) MovePigInLane(_selectedLane, _selectedPig, +1);
            if (GUILayout.Button("Remove")) RemoveSelectedPig();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Move to Lane:", GUILayout.Width(100));
            for (int li = 0; li < _editLevel.Lanes.Length; li++)
            {
                using (new EditorGUI.DisabledScope(li == _selectedLane))
                    if (GUILayout.Button(li.ToString(), GUILayout.Width(28)))
                        MovePigToLane(_selectedLane, _selectedPig, li);
            }
            EditorGUILayout.EndHorizontal();
        }

        // ---------- Edit operations ----------

        private void AddPig(int laneIndex)
        {
            var lanes = _editLevel.Lanes;
            var old = lanes[laneIndex].Pigs ?? new PigData[0];
            var n = new PigData[old.Length + 1];
            for (int i = 0; i < old.Length; i++) n[i] = old[i];
            n[old.Length] = new PigData { Color = _selectedColor, Ammo = _config.MinAmmoPerPig };
            lanes[laneIndex].Pigs = n;
            _selectedLane = laneIndex;
            _selectedPig = old.Length;
        }

        private void RemoveSelectedPig()
        {
            var pigs = _editLevel.Lanes[_selectedLane].Pigs;
            var n = new PigData[pigs.Length - 1];
            for (int i = 0, k = 0; i < pigs.Length; i++)
                if (i != _selectedPig) n[k++] = pigs[i];
            _editLevel.Lanes[_selectedLane].Pigs = n;
            _selectedPig = Mathf.Min(_selectedPig, n.Length - 1);
        }

        private void MovePigInLane(int laneIndex, int pigIndex, int delta)
        {
            var pigs = _editLevel.Lanes[laneIndex].Pigs;
            int target = pigIndex + delta;
            if (target < 0 || target >= pigs.Length) return;
            (pigs[pigIndex], pigs[target]) = (pigs[target], pigs[pigIndex]);
            _selectedPig = target;
        }

        private void MovePigToLane(int fromLane, int pigIndex, int toLane)
        {
            var fromPigs = _editLevel.Lanes[fromLane].Pigs;
            var toPigs = _editLevel.Lanes[toLane].Pigs ?? new PigData[0];
            var moving = fromPigs[pigIndex];

            var newFrom = new PigData[fromPigs.Length - 1];
            for (int i = 0, k = 0; i < fromPigs.Length; i++)
                if (i != pigIndex) newFrom[k++] = fromPigs[i];

            var newTo = new PigData[toPigs.Length + 1];
            for (int i = 0; i < toPigs.Length; i++) newTo[i] = toPigs[i];
            newTo[toPigs.Length] = moving;

            _editLevel.Lanes[fromLane].Pigs = newFrom;
            _editLevel.Lanes[toLane].Pigs = newTo;
            _selectedLane = toLane;
            _selectedPig = newTo.Length - 1;
        }

        private void RunValidate()
        {
            bool ok = GreedyLevelOracle.IsSolvable(_editLevel.Cubes, _editLevel.Lanes, _config.LevelDataServiceConfig.TraySize);
            _editStatus = ok ? "Validate: SOLVABLE" : "Validate: NOT SOLVABLE";
        }

        private void RegenerateLanes()
        {
            var preset = _config.DifficultyPresets[Mathf.Clamp(_difficultyIndex, 0, _config.DifficultyPresets.Length - 1)];
            int laneCount = _editLevel.Lanes != null && _editLevel.Lanes.Length > 0
                ? _editLevel.Lanes.Length
                : Mathf.Clamp(_config.DefaultLaneCount, LaneMin, LaneMax);

            int seedBase = (_editSourcePath ?? "regen").GetHashCode();
            LaneData[] lanes = null;
            int attempts = 0;
            for (int i = 0; i < _config.MaxValidationRetries; i++)
            {
                attempts++;
                var rng = new System.Random(seedBase + i);
                lanes = PigGenerator.Generate(_editLevel.Cubes, laneCount, preset, _config, rng);
                if (GreedyLevelOracle.IsSolvable(_editLevel.Cubes, lanes, _config.LevelDataServiceConfig.TraySize)) break;
                lanes = null;
            }

            if (lanes == null)
            {
                _editStatus = $"Regenerate FAILED after {attempts} attempts.";
                return;
            }

            _editLevel.Lanes = lanes;
            _selectedLane = -1;
            _selectedPig = -1;
            _editStatus = $"Lanes regenerated (attempts: {attempts}).";
        }

        private void SaveEdit()
        {
            if (_editLevel == null) return;

            string path = _editSourcePath;
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanel("Save Level JSON", _config.OutputFolder, "level", "json");
                if (string.IsNullOrEmpty(path)) return;
                _editSourcePath = path;
            }

            try
            {
                File.WriteAllText(path, JsonUtility.ToJson(_editLevel, prettyPrint: true));
                AssetDatabase.Refresh();
                _editStatus = $"Saved to {path}";
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Save Error", e.Message, "OK");
            }
        }

        // ---------- Helpers ----------

        private static Texture2D ResolveTexture(Object obj)
        {
            if (obj == null) return null;
            if (obj is Texture2D t) return t;
            if (obj is Sprite s) return s.texture;
            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path))
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            return null;
        }
    }
}