using System;
using System.Collections.Generic;
using UnityEngine;

public static class ReadingArchiveProgress
{
    public const string PrefKey = "DoNotLeaveMe.ReadingArchive.v1";

    [Serializable]
    private class SaveData
    {
        public int version = 1;
        public List<string> discovered = new List<string>();
    }

    private static SaveData data;

    private static SaveData Data
    {
        get
        {
            if (data != null)
                return data;
            string json = PlayerPrefs.GetString(PrefKey, string.Empty);
            data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);
            if (data == null)
                data = new SaveData();
            if (data.discovered == null)
                data.discovered = new List<string>();
            var unique = new HashSet<string>(data.discovered, StringComparer.Ordinal);
            data.discovered = new List<string>(unique);
            return data;
        }
    }

    private static string Key(string id, ReadingArchiveRole role)
    {
        return id + "|" + role;
    }

    public static bool IsDiscovered(string id, ReadingArchiveRole role)
    {
        return !string.IsNullOrEmpty(id) && Data.discovered.Contains(Key(id, role));
    }

    public static bool Discover(string id, ReadingArchiveRole role)
    {
        ReadingArchiveCatalog catalog = ReadingArchiveCatalog.Instance;
        ReadingArchiveCatalog.Entry entry = catalog != null ? catalog.Find(id) : null;
        if (entry == null || !ReadingArchiveCatalog.HasValidPages(entry.PagesFor(role)))
            return false;

        string key = Key(id, role);
        if (Data.discovered.Contains(key))
            return false;

        Data.discovered.Add(key);
        Save();
        return true;
    }

    public static void MigrateLegacyTutorials()
    {
        Migrate(TutorialPopup.PrefKey, TutorialPopup.OpeningArchiveId);
        Migrate(TutorialPopup.CheckpointPrefKeyDefault, TutorialPopup.CheckpointArchiveId);
        Migrate(TutorialPopup.Level045PrefKeyDefault, TutorialPopup.LevelIntroArchiveId);
    }

    private static void Migrate(string legacyKey, string archiveId)
    {
        if (PlayerPrefs.GetInt(legacyKey, 0) == 1)
            Discover(archiveId, ReadingArchiveRole.Human);
    }

    private static void Save()
    {
        PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    public static void ResetCacheForTests()
    {
        data = null;
    }
#endif
}
