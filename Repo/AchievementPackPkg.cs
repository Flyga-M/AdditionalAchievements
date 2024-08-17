using AchievementLib.Pack.PersistantData;
using System;
using System.Text.Json.Serialization;

namespace Flyga.AdditionalAchievements.Repo
{
    // mostly copied from https://github.com/blish-hud/Pathing/blob/main/MarkerPackRepo/MarkerPackPkg.cs
    [Store]
    public class AchievementPackPkg : IRetrievable
    {
        private bool _keepUpdated;

        public event EventHandler<bool> KeepUpdatedChanged;

        public string Name { get; set; } // TODO: make localizable?

        [StorageProperty(IsPrimaryKey = true, ColumnName = "Id", DoNotRetrieve = true)]
        public string Namespace { get; }
        public string Description { get; set; } // TODO: make localizable?
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public string[] Tags { get; set; }
        public Version Version { get; set; }
        public string Author { get; set; }
        public DateTime LastUpdate { get; set; }

        [JsonIgnore]
        public bool IsRetrieving { get; set; }

        [JsonIgnore]
        public string FileName
        {
            get
            {
                Uri uri = new Uri(DownloadUrl);
                
                try
                {
                    return System.IO.Path.GetFileName(uri.LocalPath);
                }
                catch
                {
                    return $"{Namespace}_v{Version}.zip";
                }
            }
        }

        [JsonIgnore]
        public PkgState State { get; set; }

        [JsonIgnore]
        [StorageProperty]
        public bool KeepUpdated
        {
            get => _keepUpdated;
            set
            {
                bool oldValue = _keepUpdated;
                _keepUpdated = value;

                if (oldValue != value)
                {
                    Storage.TryStoreProperty(this, nameof(KeepUpdated));
                    KeepUpdatedChanged?.Invoke(this, value);
                }
            }
        }

        [JsonConstructor]
        public AchievementPackPkg(string name, string @namespace, string description, string downloadUrl, string infoUrl, string[] tags, Version version, string author, DateTime lastUpdate)
        {
            Name = name;
            Namespace = @namespace;
            Description = description;
            DownloadUrl = downloadUrl;
            InfoUrl = infoUrl;
            Tags = tags;
            Version = version;
            Author = author;
            LastUpdate = lastUpdate;

            State = new PkgState();

            Storage.TryRetrieve(this, out _);
        }
    }
}
