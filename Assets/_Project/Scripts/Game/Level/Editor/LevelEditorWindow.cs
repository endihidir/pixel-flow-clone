using System.Collections.Generic;
using System.IO;
using System.Text;
using Game.Level.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Level.EditorTools
{
    public sealed class LevelEditorWindow : EditorWindow
    {
        private const int LaneMin = 2;
        private const int LaneMax = 5;
        private const string EditorConfigPathKey = "PixelFlow.LevelEditor.ConfigPath";
        private const string SourceTextureFolderKey = "PixelFlow.LevelEditor.SourceTextureFolder";
        private const string LevelJsonFolderKey = "PixelFlow.LevelEditor.LevelJsonFolder";
        private const string TextureSettingsKey = "PixelFlow.LevelEditor.TextureSettings";

        private enum Tab { Generate, Load, Edit }
        private enum SaveMode { NewFile, Overwrite }

        private sealed class TextureEntry
        {
            public Texture2D Texture;
            public LevelTextureImportSettings Settings;
        }

        private LevelEditorConfigSO _config;
        private Tab _tab = Tab.Generate;

        private readonly List<TextureEntry> _textureEntries = new();
        private readonly List<TextAsset> _jsonAssets = new();
        private readonly Dictionary<string, LevelTextureImportSettings> _textureSettingsByPath = new();

        private string _sourceTextureFolder = "Assets";
        private string _levelJsonFolder = "Assets";
        private string _fileNamePrefix = "level";
        private int _startLevelNumber = 1;
        private Vector2 _genScroll;
        private Vector2 _loadScroll;
        private string _lastReport;
        private bool _defaultsInitialized;

        private TextAsset _loadJson;

        private LevelJson _editLevelJson;
        private string _editSourcePath;
        private SaveMode _editSaveMode;
        private Vector2 _editScroll;
        private int _selectedLane = -1;
        private int _selectedlaneUnit = -1;
        private string _editStatus;
        private ColorId _selectedColor = ColorId.Red;

        [MenuItem("Game/Level/Level Editor")]
        public static void Open()
        {
            var w = GetWindow<LevelEditorWindow>("Level Editor");
            w.minSize = new Vector2(680, 520);
            w.Show();
        }

        private void OnEnable()
        {
            _sourceTextureFolder = EditorPrefs.GetString(SourceTextureFolderKey, "Assets");
            _levelJsonFolder = EditorPrefs.GetString(LevelJsonFolderKey, "Assets");
            LoadTextureSettingsPrefs();

            string configPath = EditorPrefs.GetString(EditorConfigPathKey, string.Empty);
            if (!string.IsNullOrEmpty(configPath))
                _config = AssetDatabase.LoadAssetAtPath<LevelEditorConfigSO>(configPath);
        }

        private void OnGUI()
        {
            DrawConfigField();
            if (_config == null) return;

            EnsureDefaultsInit();
            DrawTabs();

            EditorGUILayout.Space();
            switch (_tab)
            {
                case Tab.Generate: DrawGenerateTab(); break;
                case Tab.Load:     DrawLoadTab();     break;
                case Tab.Edit:     DrawEditTab();     break;
            }
        }

        private void DrawConfigField()
        {
            EditorGUI.BeginChangeCheck();
            _config = (LevelEditorConfigSO)EditorGUILayout.ObjectField("Editor Config", _config, typeof(LevelEditorConfigSO), false);
            if (EditorGUI.EndChangeCheck())
            {
                string path = _config != null ? AssetDatabase.GetAssetPath(_config) : string.Empty;
                EditorPrefs.SetString(EditorConfigPathKey, path);
                _defaultsInitialized = false;
            }

            if (_config == null)
                EditorGUILayout.HelpBox("Assign a LevelEditorConfigSO to begin.", MessageType.Info);
        }

        private void EnsureDefaultsInit()
        {
            if (_defaultsInitialized) return;
            if (_levelJsonFolder == "Assets" && !string.IsNullOrEmpty(_config.OutputFolder))
                _levelJsonFolder = _config.OutputFolder;
            _defaultsInitialized = true;
        }

        private void DrawTabs()
        {
            EditorGUILayout.Space();
            int idx = (int)_tab;
            int newIdx = GUILayout.Toolbar(idx, new[] { "Generate", "Load", "Edit" });
            if (newIdx != idx) _tab = (Tab)newIdx;
        }

        private void DrawGenerateTab()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            _fileNamePrefix = EditorGUILayout.TextField("File Prefix", _fileNamePrefix);
            _startLevelNumber = EditorGUILayout.IntField("Default Start Level Number", _startLevelNumber);

            var presets = _config.DifficultyPresets;
            if (presets == null || presets.Length == 0)
                EditorGUILayout.HelpBox("No DifficultyPresets configured.", MessageType.Warning);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Source Textures", EditorStyles.boldLabel);
            DrawFolderField("Texture Folder", ref _sourceTextureFolder, SourceTextureFolderKey, RefreshTexturesFromFolder);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Textures")) RefreshTexturesFromFolder();
            if (GUILayout.Button("Use Selected Textures")) UseSelectedTextures();
            if (GUILayout.Button("Add Slot")) AddTextureEntry(null);
            if (GUILayout.Button("Clear")) _textureEntries.Clear();
            EditorGUILayout.EndHorizontal();

            _genScroll = EditorGUILayout.BeginScrollView(_genScroll, GUILayout.Height(420));
            for (int i = 0; i < _textureEntries.Count; i++)
                DrawTextureEntry(i);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(GetValidTextureCount() == 0))
            {
                if (GUILayout.Button("Import All", GUILayout.Height(28)))
                    ImportAll();
            }

            using (new EditorGUI.DisabledScope(GetValidTextureCount() != 1))
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

        private void DrawTextureEntry(int index)
        {
            var entry = _textureEntries[index];

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            var texture = (Texture2D)EditorGUILayout.ObjectField(entry.Texture, typeof(Texture2D), false, GUILayout.Width(96), GUILayout.Height(96));
            if (EditorGUI.EndChangeCheck())
            {
                entry.Texture = texture;
                entry.Settings = GetStoredOrDefaultSettings(texture, _startLevelNumber + index);
            }

            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                _textureEntries.RemoveAt(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var settings = entry.Settings;
            settings.LevelNumber = EditorGUILayout.IntField("Level Number", settings.LevelNumber);

            var presets = _config.DifficultyPresets;
            if (presets != null && presets.Length > 0)
            {
                var names = new string[presets.Length];
                for (int i = 0; i < presets.Length; i++)
                    names[i] = string.IsNullOrEmpty(presets[i].Name) ? $"Preset {i}" : presets[i].Name;

                settings.DifficultyIndex = EditorGUILayout.Popup("Difficulty", Mathf.Clamp(settings.DifficultyIndex, 0, presets.Length - 1), names);
            }

            int laneCount = settings.LaneCount <= 0 ? _config.DefaultLaneCount : settings.LaneCount;
            settings.LaneCount = EditorGUILayout.IntSlider("Lane Count", Mathf.Clamp(laneCount, LaneMin, LaneMax), LaneMin, LaneMax);
            settings.MaxGridSize = EditorGUILayout.IntField("Max Grid Size", settings.MaxGridSize);
            settings.AlphaThreshold = EditorGUILayout.Slider("Alpha Threshold", settings.AlphaThreshold, 0f, 1f);
            settings.CellOpaqueRatio = EditorGUILayout.Slider("Cell Opaque Ratio", settings.CellOpaqueRatio, 0f, 1f);
            settings.MaxGridSize = Mathf.Max(1, settings.MaxGridSize);
            settings.LevelNumber = Mathf.Max(1, settings.LevelNumber);

            if (EditorGUI.EndChangeCheck())
            {
                entry.Settings = settings;
                StoreTextureSettings(entry.Texture, settings);
                SaveTextureSettingsPrefs();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLoadTab()
        {
            EditorGUILayout.LabelField("Load Existing JSON", EditorStyles.boldLabel);
            DrawFolderField("JSON Folder", ref _levelJsonFolder, LevelJsonFolderKey, RefreshJsonAssetsFromFolder);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh JSON List")) RefreshJsonAssetsFromFolder();
            EditorGUILayout.EndHorizontal();

            _loadScroll = EditorGUILayout.BeginScrollView(_loadScroll, GUILayout.Height(220));
            for (int i = 0; i < _jsonAssets.Count; i++)
            {
                var json = _jsonAssets[i];
                if (json == null) continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(json, typeof(TextAsset), false);
                if (GUILayout.Button("Open", GUILayout.Width(70)))
                    LoadFromAsset(json);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manual", EditorStyles.boldLabel);
            _loadJson = (TextAsset)EditorGUILayout.ObjectField("Level JSON", _loadJson, typeof(TextAsset), false);

            using (new EditorGUI.DisabledScope(_loadJson == null))
            {
                if (GUILayout.Button("Open in Editor", GUILayout.Height(28)))
                    LoadFromAsset(_loadJson);
            }

            if (GUILayout.Button("Browse..."))
            {
                string path = EditorUtility.OpenFilePanel("Open Level JSON", AssetPathToAbsolute(_levelJsonFolder), "json");
                if (!string.IsNullOrEmpty(path)) LoadFromPath(path);
            }
        }

        private void DrawFolderField(string label, ref string folder, string prefsKey, System.Action refreshAction)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            folder = EditorGUILayout.TextField(label, folder);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetString(prefsKey, folder);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
            {
                string selected = EditorUtility.OpenFolderPanel(label, AssetPathToAbsolute(folder), string.Empty);
                if (!string.IsNullOrEmpty(selected))
                {
                    folder = AbsolutePathToAssetPath(selected);
                    EditorPrefs.SetString(prefsKey, folder);
                    refreshAction?.Invoke();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void RefreshTexturesFromFolder()
        {
            _textureEntries.Clear();

            foreach (string assetPath in FindAssetPathsRecursive(_sourceTextureFolder, "t:Texture2D", ".png", ".jpg", ".jpeg", ".tga", ".psd"))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tex != null) AddTextureEntry(tex);
            }

            _lastReport = $"Found {_textureEntries.Count} texture(s).";
        }

        private void RefreshJsonAssetsFromFolder()
        {
            _jsonAssets.Clear();

            foreach (string assetPath in FindAssetPathsRecursive(_levelJsonFolder, "t:TextAsset", ".json"))
            {
                var json = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                if (json != null) _jsonAssets.Add(json);
            }
        }

        private void UseSelectedTextures()
        {
            _textureEntries.Clear();
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D tex)
                    AddTextureEntry(tex);
            }
        }

        private void AddTextureEntry(Texture2D texture)
        {
            _textureEntries.Add(new TextureEntry
            {
                Texture = texture,
                Settings = GetStoredOrDefaultSettings(texture, _startLevelNumber + GetValidTextureCount())
            });
        }

        private void ImportAll()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < _textureEntries.Count; i++)
            {
                var entry = _textureEntries[i];
                var png = entry.Texture;
                if (png == null) continue;

                int levelNumber = Mathf.Max(1, entry.Settings.LevelNumber);
                int difficultyIndex = GetClampedDifficultyIndex(entry.Settings.DifficultyIndex);
                int laneCount = Mathf.Clamp(entry.Settings.LaneCount, LaneMin, LaneMax);
                string outName = $"{_fileNamePrefix}_{levelNumber}";
                var result = LevelImportPipeline.Import(png, laneCount, difficultyIndex, _config, entry.Settings, outName, levelNumber);

                if (result.Success)
                    sb.AppendLine($"OK  {png.name} -> {outName}.json (level {levelNumber}, lanes: {laneCount}, difficulty: {difficultyIndex}, attempts: {result.Attempts})");
                else
                    sb.AppendLine($"FAIL  {png.name}: {result.Error}");
            }

            _lastReport = sb.ToString();
            RefreshJsonAssetsFromFolder();
            Repaint();
        }

        private void ImportAndEditSingle()
        {
            TextureEntry entry = null;
            for (int i = 0; i < _textureEntries.Count; i++)
            {
                if (_textureEntries[i].Texture != null)
                {
                    entry = _textureEntries[i];
                    break;
                }
            }

            if (entry == null) return;

            var png = entry.Texture;
            int levelNumber = Mathf.Max(1, entry.Settings.LevelNumber);
            int difficultyIndex = GetClampedDifficultyIndex(entry.Settings.DifficultyIndex);
            int laneCount = Mathf.Clamp(entry.Settings.LaneCount, LaneMin, LaneMax);
            string outName = $"{_fileNamePrefix}_{levelNumber}";
            var result = LevelImportPipeline.Import(png, laneCount, difficultyIndex, _config, entry.Settings, outName, levelNumber);
            if (!result.Success)
            {
                _lastReport = $"FAIL  {png.name}: {result.Error}";
                return;
            }

            _editLevelJson = result.LevelJson;
            _editSourcePath = result.OutputPath;
            _editSaveMode = SaveMode.NewFile;
            _selectedLane = -1;
            _selectedlaneUnit = -1;
            _editStatus = $"Generated from {png.name}.";
            _tab = Tab.Edit;
            RefreshJsonAssetsFromFolder();
            Repaint();
        }

        private void LoadFromAsset(TextAsset asset)
        {
            try
            {
                _editLevelJson = JsonUtility.FromJson<LevelJson>(asset.text);
                _editSourcePath = AssetDatabase.GetAssetPath(asset);
                _editSaveMode = SaveMode.Overwrite;
                _selectedLane = -1;
                _selectedlaneUnit = -1;
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
                var levelJson = JsonUtility.FromJson<LevelJson>(text);
                _editLevelJson = levelJson;
                _editSourcePath = absolutePath;
                _editSaveMode = SaveMode.Overwrite;
                _selectedLane = -1;
                _selectedlaneUnit = -1;
                _editStatus = $"Loaded {Path.GetFileName(absolutePath)}.";
                _tab = Tab.Edit;
                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Load Error", e.Message, "OK");
            }
        }

        private void DrawEditTab()
        {
            if (_editLevelJson == null)
            {
                EditorGUILayout.HelpBox("No level loaded. Use the Generate or Load tab first.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Size: {_editLevelJson.grid_width}x{_editLevelJson.grid_height}    Lanes: {_editLevelJson.lanes?.Length ?? 0}", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            int newLevelNumber = EditorGUILayout.IntField("Level #", _editLevelJson.level_number, GUILayout.Width(120));
            if (EditorGUI.EndChangeCheck())
                _editLevelJson.level_number = newLevelNumber;

            if (GUILayout.Button("Validate", GUILayout.Width(80))) RunValidate();
            if (GUILayout.Button("Regenerate Lanes", GUILayout.Width(140))) RegenerateLanes();
            if (GUILayout.Button("Save", GUILayout.Width(80))) SaveEdit();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_editStatus))
            {
                Color previousColor = GUI.color;

                if (_editStatus.StartsWith("Solvable"))
                    GUI.color = Color.green;
                else if (_editStatus.StartsWith("Not solvable"))
                    GUI.color = Color.red;

                EditorGUILayout.HelpBox(_editStatus, MessageType.None);

                GUI.color = previousColor;
            }

            _editScroll = EditorGUILayout.BeginScrollView(_editScroll);

            DrawBoardEditor();
            EditorGUILayout.Space();
            DrawLaneEditor();
            EditorGUILayout.Space();
            DrawlaneUnitDetailPanel();

            EditorGUILayout.EndScrollView();
        }

        private void DrawBoardEditor()
        {
            EditorGUILayout.LabelField("Board (click to set selected color, right-click to erase)", EditorStyles.boldLabel);

            int w = _editLevelJson.grid_width;
            int h = _editLevelJson.grid_height;
            if (w == 0 || h == 0) return;

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
                    var id = ParseColor(_editLevelJson.pixels[flat]);
                    var cellRect = new Rect(rect.x + x * cell, rect.y + y * cell, cell - 1, cell - 1);
                    var color = id == ColorId.None
                        ? new Color(0.18f, 0.10f, 0.25f)
                        : (palette != null ? palette.GetColor(id) : Color.magenta);
                    EditorGUI.DrawRect(cellRect, color);

                    if (e.type == EventType.MouseDown && cellRect.Contains(e.mousePosition))
                    {
                        if (e.button == 1)
                        {
                            _editLevelJson.pixels[flat] = ColorId.None.ToString();
                            _editStatus = "Board changed. Run Validate or Regenerate Lanes.";
                            GUI.changed = true;
                            e.Use();
                            Repaint();
                        }
                        else if (e.button == 0 && _selectedColor != ColorId.None)
                        {
                            _editLevelJson.pixels[flat] = _selectedColor.ToString();
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
            EditorGUILayout.LabelField("Lanes (click laneUnit to select)", EditorStyles.boldLabel);
            var palette = _config.Palette;

            var lanes = _editLevelJson.lanes;
            if (lanes == null) return;

            EditorGUILayout.BeginHorizontal();
            for (int li = 0; li < lanes.Length; li++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(76));
                EditorGUILayout.LabelField($"Lane {li}", EditorStyles.miniBoldLabel);
                var laneUnits = lanes[li].laneUnits;
                if (laneUnits != null)
                {
                    for (int pi = 0; pi < laneUnits.Length; pi++)
                    {
                        var laneUnit = laneUnits[pi];
                        var bg = GUI.backgroundColor;
                        GUI.backgroundColor = palette != null ? palette.GetColor(ParseColor(laneUnit.color)) : Color.gray;
                        bool isSel = (li == _selectedLane && pi == _selectedlaneUnit);
                        string label = (isSel ? ">" : "") + laneUnit.ammo.ToString();
                        if (GUILayout.Button(label, GUILayout.Width(60), GUILayout.Height(22)))
                        {
                            _selectedLane = li;
                            _selectedlaneUnit = pi;
                        }
                        GUI.backgroundColor = bg;
                    }
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+", GUILayout.Width(28))) AddlaneUnit(li);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawlaneUnitDetailPanel()
        {
            EditorGUILayout.LabelField("Selected laneUnit", EditorStyles.boldLabel);
            if (_selectedLane < 0 || _selectedlaneUnit < 0
                || _selectedLane >= _editLevelJson.lanes.Length
                || _selectedlaneUnit >= _editLevelJson.lanes[_selectedLane].laneUnits.Length)
            {
                EditorGUILayout.HelpBox("No laneUnit selected.", MessageType.None);
                return;
            }

            var laneUnits = _editLevelJson.lanes[_selectedLane].laneUnits;
            var laneUnit = laneUnits[_selectedlaneUnit];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Lane {_selectedLane}, Slot {_selectedlaneUnit}", GUILayout.Width(160));
            laneUnit.color = ((ColorId)EditorGUILayout.EnumPopup("Color", ParseColor(laneUnit.color))).ToString();
            EditorGUILayout.EndHorizontal();

            laneUnit.ammo = EditorGUILayout.IntSlider("Ammo", laneUnit.ammo,
                _config.MinAmmoPerLaneUnit, _config.MaxAmmoPerLaneUnit);

            laneUnits[_selectedlaneUnit] = laneUnit;

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_selectedlaneUnit <= 0))
                if (GUILayout.Button("Move Up")) MovelaneUnitInLane(_selectedLane, _selectedlaneUnit, -1);
            using (new EditorGUI.DisabledScope(_selectedlaneUnit >= laneUnits.Length - 1))
                if (GUILayout.Button("Move Down")) MovelaneUnitInLane(_selectedLane, _selectedlaneUnit, +1);
            if (GUILayout.Button("Remove")) RemoveSelectedlaneUnit();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Move to Lane:", GUILayout.Width(100));
            for (int li = 0; li < _editLevelJson.lanes.Length; li++)
            {
                using (new EditorGUI.DisabledScope(li == _selectedLane))
                    if (GUILayout.Button(li.ToString(), GUILayout.Width(28)))
                        MovelaneUnitToLane(_selectedLane, _selectedlaneUnit, li);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddlaneUnit(int laneIndex)
        {
            var lanes = _editLevelJson.lanes;
            var old = lanes[laneIndex].laneUnits ?? new LaneUnitJson[0];
            var n = new LaneUnitJson[old.Length + 1];
            for (int i = 0; i < old.Length; i++) n[i] = old[i];
            n[old.Length] = new LaneUnitJson { color = _selectedColor.ToString(), ammo = _config.MinAmmoPerLaneUnit };
            lanes[laneIndex].laneUnits = n;
            _selectedLane = laneIndex;
            _selectedlaneUnit = old.Length;
        }

        private void RemoveSelectedlaneUnit()
        {
            var laneUnits = _editLevelJson.lanes[_selectedLane].laneUnits;
            var n = new LaneUnitJson[laneUnits.Length - 1];
            for (int i = 0, k = 0; i < laneUnits.Length; i++)
                if (i != _selectedlaneUnit) n[k++] = laneUnits[i];
            _editLevelJson.lanes[_selectedLane].laneUnits = n;
            _selectedlaneUnit = Mathf.Min(_selectedlaneUnit, n.Length - 1);
        }

        private void MovelaneUnitInLane(int laneIndex, int laneUnitIndex, int delta)
        {
            var laneUnits = _editLevelJson.lanes[laneIndex].laneUnits;
            int target = laneUnitIndex + delta;
            if (target < 0 || target >= laneUnits.Length) return;
            (laneUnits[laneUnitIndex], laneUnits[target]) = (laneUnits[target], laneUnits[laneUnitIndex]);
            _selectedlaneUnit = target;
        }

        private void MovelaneUnitToLane(int fromLane, int laneUnitIndex, int toLane)
        {
            var fromlaneUnits = _editLevelJson.lanes[fromLane].laneUnits;
            var tolaneUnits = _editLevelJson.lanes[toLane].laneUnits ?? new LaneUnitJson[0];
            var moving = fromlaneUnits[laneUnitIndex];

            var newFrom = new LaneUnitJson[fromlaneUnits.Length - 1];
            for (int i = 0, k = 0; i < fromlaneUnits.Length; i++)
                if (i != laneUnitIndex) newFrom[k++] = fromlaneUnits[i];

            var newTo = new LaneUnitJson[tolaneUnits.Length + 1];
            for (int i = 0; i < tolaneUnits.Length; i++) newTo[i] = tolaneUnits[i];
            newTo[tolaneUnits.Length] = moving;

            _editLevelJson.lanes[fromLane].laneUnits = newFrom;
            _editLevelJson.lanes[toLane].laneUnits = newTo;
            _selectedLane = toLane;
            _selectedlaneUnit = newTo.Length - 1;
        }

        private void RunValidate()
        {
            if (GreedyLevelValidator.IsSolvable(_editLevelJson.pixels, _editLevelJson.lanes, _config.LevelDataServiceConfig.UnitSlotSize, out string reason))
            {
                _editStatus = "Solvable";
            }
            else
            {
                _editStatus = string.IsNullOrEmpty(reason)
                    ? "Not solvable"
                    : "Not solvable\n" + reason;
            }
        }

        private void RegenerateLanes()
        {
            int laneCount = _editLevelJson.lanes != null && _editLevelJson.lanes.Length > 0
                ? _editLevelJson.lanes.Length
                : Mathf.Clamp(_config.DefaultLaneCount, LaneMin, LaneMax);

            int difficultyIndex = GetClampedDifficultyIndex(0);
            var preset = _config.DifficultyPresets[difficultyIndex];

            int seedBase = (_editSourcePath ?? "regen").GetHashCode();
            LaneJson[] lanes = null;
            int attempts = 0;
            for (int i = 0; i < _config.MaxValidationRetries; i++)
            {
                attempts++;
                var rng = new System.Random(seedBase + i);
                lanes = LaneGenerator.Generate(ToColorArray(_editLevelJson.pixels), laneCount, preset, _config, rng);
                if (GreedyLevelValidator.IsSolvable(_editLevelJson.pixels, lanes, _config.LevelDataServiceConfig.UnitSlotSize)) break;
                lanes = null;
            }

            if (lanes == null)
            {
                _editStatus = $"Regenerate FAILED after {attempts} attempts.";
                return;
            }
            _editLevelJson.lanes = lanes;

            _selectedLane = -1;
            _selectedlaneUnit = -1;
            _editStatus = $"Lanes regenerated (attempts: {attempts}).";
        }

        private void SaveEdit()
        {
            if (_editLevelJson == null) return;

            string path = _editSourcePath;
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanel("Save Level JSON", AssetPathToAbsolute(_levelJsonFolder), "level", "json");
                if (string.IsNullOrEmpty(path)) return;
                _editSourcePath = path;
            }

            try
            {
                File.WriteAllText(path, LevelJsonConverter.ToJson(_editLevelJson, prettyPrint: true));
                AssetDatabase.Refresh();
                RefreshJsonAssetsFromFolder();
                _editStatus = $"Saved to {path}";
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Save Error", e.Message, "OK");
            }
        }

        private int GetValidTextureCount()
        {
            int count = 0;
            for (int i = 0; i < _textureEntries.Count; i++)
                if (_textureEntries[i].Texture != null) count++;
            return count;
        }

        private LevelTextureImportSettings GetStoredOrDefaultSettings(Texture2D texture, int suggestedLevelNumber)
        {
            if (texture != null)
            {
                string path = AssetDatabase.GetAssetPath(texture);
                if (!string.IsNullOrEmpty(path) && _textureSettingsByPath.TryGetValue(path, out var settings))
                {
                    settings.DifficultyIndex = GetClampedDifficultyIndex(settings.DifficultyIndex < 0 ? 0 : settings.DifficultyIndex);
                    settings.LevelNumber = settings.LevelNumber <= 0 ? Mathf.Max(1, suggestedLevelNumber) : Mathf.Max(1, settings.LevelNumber);
                    settings.LaneCount = Mathf.Clamp(settings.LaneCount <= 0 ? _config.DefaultLaneCount : settings.LaneCount, LaneMin, LaneMax);
                    return settings;
                }
            }

            var defaultSettings = LevelTextureImportSettings.FromConfig(_config);
            defaultSettings.DifficultyIndex = GetClampedDifficultyIndex(0);
            defaultSettings.LevelNumber = Mathf.Max(1, suggestedLevelNumber);
            defaultSettings.LaneCount = Mathf.Clamp(_config.DefaultLaneCount, LaneMin, LaneMax);
            return defaultSettings;
        }

        private int GetClampedDifficultyIndex(int difficultyIndex)
        {
            var presets = _config != null ? _config.DifficultyPresets : null;
            if (presets == null || presets.Length == 0)
                return 0;

            return Mathf.Clamp(difficultyIndex, 0, presets.Length - 1);
        }

        private void StoreTextureSettings(Texture2D texture, LevelTextureImportSettings settings)
        {
            if (texture == null) return;
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;
            _textureSettingsByPath[path] = settings;
        }

        private void LoadTextureSettingsPrefs()
        {
            _textureSettingsByPath.Clear();
            string raw = EditorPrefs.GetString(TextureSettingsKey, string.Empty);
            if (string.IsNullOrEmpty(raw)) return;

            var lines = raw.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                var parts = line.Split('|');
                if (parts.Length != 4 && parts.Length != 6 && parts.Length != 7) continue;
                if (!int.TryParse(parts[1], out int maxGridSize)) continue;
                if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float alpha)) continue;
                if (!float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ratio)) continue;

                int difficultyIndex = -1;
                int levelNumber = 0;
                int laneCount = 0;

                if (parts.Length >= 6)
                {
                    int.TryParse(parts[4], out difficultyIndex);
                    int.TryParse(parts[5], out levelNumber);
                }

                if (parts.Length == 7)
                    int.TryParse(parts[6], out laneCount);

                _textureSettingsByPath[parts[0]] = new LevelTextureImportSettings(maxGridSize, alpha, ratio, difficultyIndex, levelNumber, laneCount);
            }
        }

        private void SaveTextureSettingsPrefs()
        {
            var sb = new StringBuilder();
            foreach (var kv in _textureSettingsByPath)
            {
                sb.Append(kv.Key).Append('|');
                sb.Append(kv.Value.MaxGridSize).Append('|');
                sb.Append(kv.Value.AlphaThreshold.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append('|');
                sb.Append(kv.Value.CellOpaqueRatio.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append('|');
                sb.Append(kv.Value.DifficultyIndex).Append('|');
                sb.Append(Mathf.Max(1, kv.Value.LevelNumber)).Append('|');
                sb.Append(Mathf.Clamp(kv.Value.LaneCount <= 0 ? LaneMin : kv.Value.LaneCount, LaneMin, LaneMax)).Append('\n');
            }
            EditorPrefs.SetString(TextureSettingsKey, sb.ToString());
        }

        private static IEnumerable<string> FindAssetPathsRecursive(string folder, string filter, params string[] extensions)
        {
            if (string.IsNullOrEmpty(folder)) yield break;

            string assetFolder = folder.StartsWith("Assets") ? folder : AbsolutePathToAssetPath(folder);
            if (!assetFolder.StartsWith("Assets")) yield break;

            string[] guids = AssetDatabase.FindAssets(filter, new[] { assetFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!HasExtension(path, extensions)) continue;
                yield return path;
            }
        }

        private static bool HasExtension(string path, string[] extensions)
        {
            if (extensions == null || extensions.Length == 0) return true;
            string ext = Path.GetExtension(path).ToLowerInvariant();
            for (int i = 0; i < extensions.Length; i++)
            {
                if (ext == extensions[i]) return true;
            }
            return false;
        }

        private static string AssetPathToAbsolute(string path)
        {
            if (string.IsNullOrEmpty(path)) return Application.dataPath;
            if (Path.IsPathRooted(path)) return path;
            if (path == "Assets") return Application.dataPath;
            if (path.StartsWith("Assets/")) return Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
            return path;
        }

        private static string AbsolutePathToAssetPath(string absolutePath)
        {
            absolutePath = absolutePath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (absolutePath == dataPath) return "Assets";
            if (absolutePath.StartsWith(dataPath + "/")) return "Assets/" + absolutePath.Substring(dataPath.Length + 1);
            return absolutePath;
        }

        private static ColorId ParseColor(string value)
        {
            return System.Enum.TryParse(value, out ColorId id) ? id : ColorId.None;
        }

        private static ColorId[] ToColorArray(string[] pixels)
        {
            var result = new ColorId[pixels?.Length ?? 0];
            for (int i = 0; i < result.Length; i++) result[i] = ParseColor(pixels[i]);
            return result;
        }
    }
}