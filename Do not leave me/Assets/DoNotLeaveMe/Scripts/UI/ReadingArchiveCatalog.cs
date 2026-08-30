using System;
using System.Collections.Generic;
using UnityEngine;

public enum ReadingArchiveCategory { Story, Controls }
public enum ReadingArchiveRole { Human, Dog }

[CreateAssetMenu(menuName = "DoNotLeaveMe/Reading Archive Catalog", fileName = "ReadingArchiveCatalog")]
public class ReadingArchiveCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string id;
        public string title;
        public ReadingArchiveCategory category;
        public Sprite[] humanPages;
        public Sprite[] dogPages;

        public Sprite[] PagesFor(ReadingArchiveRole role)
        {
            return role == ReadingArchiveRole.Dog ? dogPages : humanPages;
        }
    }

    public const string ResourcePath = "DoNotLeaveMe/ReadingArchiveCatalog";

    [SerializeField] private List<Entry> entries = new List<Entry>();
    private static ReadingArchiveCatalog instance;

    public static ReadingArchiveCatalog Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<ReadingArchiveCatalog>(ResourcePath);
            return instance;
        }
    }

    public IReadOnlyList<Entry> Entries => entries;

#if UNITY_EDITOR
    public List<Entry> EditorEntries => entries;
#endif

    public Entry Find(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        for (int i = 0; i < entries.Count; i++)
            if (entries[i] != null && string.Equals(entries[i].id, id, StringComparison.Ordinal))
                return entries[i];
        return null;
    }

    public static bool HasValidPages(Sprite[] pages)
    {
        if (pages == null || pages.Length == 0)
            return false;
        for (int i = 0; i < pages.Length; i++)
            if (pages[i] != null)
                return true;
        return false;
    }
}
