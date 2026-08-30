using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ReadingArchiveEditor
{
    private const string CatalogPath = "Assets/Resources/DoNotLeaveMe/ReadingArchiveCatalog.asset";
    private const string LevelsRoot = "Assets/DoNotLeaveMe/Levels/";
    private static readonly string[] LevelNames = { "Level_01", "Level_02", "Level_03", "Level_04", "Level_05" };

    [InitializeOnLoadMethod]
    private static void ValidateAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Validate();
                RunProgressSelfTests();
            }
        };
    }

    [MenuItem("Tools/DoNotLeaveMe/阅读档案/构建与迁移")]
    public static void BuildAndMigrate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        ReadingArchiveCatalog catalog = AssetDatabase.LoadAssetAtPath<ReadingArchiveCatalog>(CatalogPath);
        if (catalog == null)
        {
            Debug.LogError("[ReadingArchive] 找不到目录资产：" + CatalogPath);
            return;
        }

        SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach (string levelName in LevelNames)
                SyncLevel(catalog, levelName);
            SyncTutorials(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        Validate();
    }

    private static void SyncLevel(ReadingArchiveCatalog catalog, string levelName)
    {
        string path = LevelsRoot + levelName + ".unity";
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        NoticeBoard[] boards = UnityEngine.Object.FindObjectsOfType<NoticeBoard>(true)
            .OrderBy(board => HierarchyPath(board.transform), StringComparer.Ordinal)
            .ToArray();
        var usedIds = new HashSet<string>(StringComparer.Ordinal);
        bool sceneChanged = false;

        for (int i = 0; i < boards.Length; i++)
        {
            SerializedObject source = new SerializedObject(boards[i]);
            SerializedProperty idProperty = source.FindProperty("archiveEntryId");
            string id = idProperty.stringValue;
            if (string.IsNullOrEmpty(id))
            {
                int suffix = 1;
                do
                    id = "story." + levelName.ToLowerInvariant() + "." + suffix++.ToString("00");
                while (usedIds.Contains(id) || catalog.Find(id) != null);
                idProperty.stringValue = id;
                source.ApplyModifiedPropertiesWithoutUndo();
                sceneChanged = true;
            }
            usedIds.Add(id);

            ReadingArchiveCatalog.Entry entry = catalog.Find(id);
            if (entry == null)
            {
                entry = new ReadingArchiveCatalog.Entry
                {
                    id = id,
                    title = levelName.Replace("Level_", "第") + "关 · 阅读资料",
                    category = ReadingArchiveCategory.Story
                };
                catalog.EditorEntries.Add(entry);
            }
            entry.humanPages = ReadPages(source.FindProperty("humanPages"));
            entry.dogPages = ReadPages(source.FindProperty("dogPages"));
        }

        if (sceneChanged)
            EditorSceneManager.SaveScene(scene);
    }

    private static void SyncTutorials(ReadingArchiveCatalog catalog)
    {
        Scene scene = EditorSceneManager.OpenScene(LevelsRoot + "Persistent.unity", OpenSceneMode.Single);
        TutorialPopup popup = UnityEngine.Object.FindObjectOfType<TutorialPopup>(true);
        if (popup == null)
            throw new InvalidOperationException("Persistent 场景找不到 TutorialPopup。");

        SerializedObject source = new SerializedObject(popup);
        SyncTutorialEntry(catalog, TutorialPopup.OpeningArchiveId, "基础操作说明",
            source.FindProperty("humanPages"), source.FindProperty("dogPages"));
        SyncTutorialEntry(catalog, TutorialPopup.CheckpointArchiveId, "存档地毯说明",
            source.FindProperty("humanCheckpointPages"), source.FindProperty("dogCheckpointPages"));
        SyncTutorialEntry(catalog, TutorialPopup.LevelIntroArchiveId, "第四点五关操作提示",
            source.FindProperty("humanLevelIntroPages"), source.FindProperty("dogLevelIntroPages"));
    }

    private static void SyncTutorialEntry(ReadingArchiveCatalog catalog, string id, string title,
        SerializedProperty human, SerializedProperty dog)
    {
        ReadingArchiveCatalog.Entry entry = catalog.Find(id);
        if (entry == null)
        {
            entry = new ReadingArchiveCatalog.Entry { id = id, title = title };
            catalog.EditorEntries.Add(entry);
        }
        entry.category = ReadingArchiveCategory.Controls;
        entry.humanPages = ReadPages(human);
        entry.dogPages = ReadPages(dog);
    }

    private static Sprite[] ReadPages(SerializedProperty array)
    {
        var pages = new List<Sprite>();
        for (int i = 0; i < array.arraySize; i++)
        {
            Sprite page = array.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
            if (page != null)
                pages.Add(page);
        }
        return pages.ToArray();
    }

    [MenuItem("Tools/DoNotLeaveMe/阅读档案/校验")]
    public static void Validate()
    {
        var errors = new List<string>();
        ReadingArchiveCatalog catalog = AssetDatabase.LoadAssetAtPath<ReadingArchiveCatalog>(CatalogPath);
        if (catalog == null)
        {
            Debug.LogError("[ReadingArchive] 找不到目录资产：" + CatalogPath);
            return;
        }

        var catalogIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (ReadingArchiveCatalog.Entry entry in catalog.Entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.id))
            {
                errors.Add("目录存在空 ID 条目。");
                continue;
            }
            if (!catalogIds.Add(entry.id))
                errors.Add("目录 ID 重复：" + entry.id);
            if (string.IsNullOrWhiteSpace(entry.title))
                errors.Add("目录标题为空：" + entry.id);
            if (!ReadingArchiveCatalog.HasValidPages(entry.humanPages) &&
                !ReadingArchiveCatalog.HasValidPages(entry.dogPages))
                errors.Add("人和狗页面都为空：" + entry.id);
            CheckNullPages(entry.id + "/Human", entry.humanPages, errors);
            CheckNullPages(entry.id + "/Dog", entry.dogPages, errors);
        }

        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        int boardCount = 0;
        var idPattern = new Regex(@"(?m)^  archiveEntryId: (\S+)\s*$");
        foreach (string levelName in LevelNames)
        {
            string text = File.ReadAllText(Path.GetFullPath(LevelsRoot + levelName + ".unity"));
            foreach (Match match in idPattern.Matches(text))
            {
                boardCount++;
                string id = match.Groups[1].Value;
                if (!sourceIds.Add(id))
                    errors.Add("场景公告 ID 重复：" + id);
                if (!catalogIds.Contains(id))
                    errors.Add("场景公告未进入目录：" + id);
            }
        }
        if (boardCount != 15)
            errors.Add("正式公告数量应为 15，实际为 " + boardCount + "。");

        foreach (string tutorialId in new[] { TutorialPopup.OpeningArchiveId, TutorialPopup.CheckpointArchiveId, TutorialPopup.LevelIntroArchiveId })
            if (!catalogIds.Contains(tutorialId))
                errors.Add("缺少教程目录条目：" + tutorialId);

        if (errors.Count == 0)
            Debug.Log("[ReadingArchive] 校验通过：15 个剧情来源、3 组操作提示、" + catalog.Entries.Count + " 个目录条目。");
        else
            Debug.LogError("[ReadingArchive] 校验失败：\n- " + string.Join("\n- ", errors));
    }

    private static void CheckNullPages(string label, Sprite[] pages, List<string> errors)
    {
        if (pages == null)
            return;
        for (int i = 0; i < pages.Length; i++)
            if (pages[i] == null)
                errors.Add(label + " 第 " + (i + 1) + " 页为空。");
    }

    private static string HierarchyPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    [MenuItem("Tools/DoNotLeaveMe/阅读档案/运行进度自测")]
    public static void RunProgressSelfTests()
    {
        string[] intKeys =
        {
            TutorialPopup.PrefKey,
            TutorialPopup.CheckpointPrefKeyDefault,
            TutorialPopup.Level045PrefKeyDefault
        };
        bool progressExisted = PlayerPrefs.HasKey(ReadingArchiveProgress.PrefKey);
        string oldProgress = PlayerPrefs.GetString(ReadingArchiveProgress.PrefKey, string.Empty);
        var oldInts = new Dictionary<string, int>();
        var oldIntExists = new Dictionary<string, bool>();
        foreach (string key in intKeys)
        {
            oldIntExists[key] = PlayerPrefs.HasKey(key);
            oldInts[key] = PlayerPrefs.GetInt(key, 0);
        }

        try
        {
            PlayerPrefs.DeleteKey(ReadingArchiveProgress.PrefKey);
            foreach (string key in intKeys)
                PlayerPrefs.DeleteKey(key);
            ReadingArchiveProgress.ResetCacheForTests();

            const string storyId = "story.level_01.01";
            Require(ReadingArchiveProgress.Discover(storyId, ReadingArchiveRole.Human), "首次发现应成功");
            Require(!ReadingArchiveProgress.Discover(storyId, ReadingArchiveRole.Human), "重复发现应去重");
            Require(ReadingArchiveProgress.IsDiscovered(storyId, ReadingArchiveRole.Human), "Human 发现应可读取");
            Require(!ReadingArchiveProgress.IsDiscovered(storyId, ReadingArchiveRole.Dog), "Human 发现不应解锁 Dog");

            PlayerPrefs.SetString(ReadingArchiveProgress.PrefKey,
                "{\"version\":1,\"discovered\":[\"missing.entry|Human\"]}");
            ReadingArchiveProgress.ResetCacheForTests();
            Require(!ReadingArchiveProgress.IsDiscovered(storyId, ReadingArchiveRole.Human), "陈旧 ID 不应误解锁有效条目");

            PlayerPrefs.DeleteKey(ReadingArchiveProgress.PrefKey);
            PlayerPrefs.SetInt(TutorialPopup.PrefKey, 1);
            ReadingArchiveProgress.ResetCacheForTests();
            ReadingArchiveProgress.MigrateLegacyTutorials();
            Require(ReadingArchiveProgress.IsDiscovered(TutorialPopup.OpeningArchiveId, ReadingArchiveRole.Human),
                "旧开局教程键应迁移为 Human 档案条目");
            Debug.Log("[ReadingArchive] 进度自测通过：首次发现、去重、角色隔离、陈旧 ID、旧存档迁移。");
        }
        catch (Exception exception)
        {
            Debug.LogError("[ReadingArchive] 进度自测失败：" + exception);
        }
        finally
        {
            if (progressExisted)
                PlayerPrefs.SetString(ReadingArchiveProgress.PrefKey, oldProgress);
            else
                PlayerPrefs.DeleteKey(ReadingArchiveProgress.PrefKey);
            foreach (string key in intKeys)
            {
                if (oldIntExists[key])
                    PlayerPrefs.SetInt(key, oldInts[key]);
                else
                    PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
            ReadingArchiveProgress.ResetCacheForTests();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
