using System;
using System.Collections.Generic;
using SyncTool.Common.Groups;
using SyncTool.Configuration;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using NodaTime;

namespace SyncTool.Synchronization.TestHelpers
{
    public class HistoryBuilder
    {
        readonly IGroup m_Group;
        readonly string m_Name;
        readonly IDictionary<string, File> m_Files = new Dictionary<string, File>(StringComparer.InvariantCultureIgnoreCase);


        public SyncFolder SyncFolder { get; }

        public IDirectory CurrentState { get; private set; }

        public HistoryBuilder(IGroup group, string name)
        {
            m_Group = @group;
            m_Name = name;
            m_Group.GetHistoryService().CreateHistory(m_Name);

            SyncFolder = new SyncFolder(m_Name) { Path = "Irrelevant" };

            m_Group.GetConfigurationService().AddItem(SyncFolder);
        }

        public void AddFile(string fileName, Instant lastWriteTime)
        {
            if (!m_Files.ContainsKey(fileName))
            {
                m_Files.Add(fileName, new File(null, fileName) { LastWriteTime = lastWriteTime});
            }
            UpdateCurrentState();
        }

        public void AddFile(string file) => AddFiles(file);

        public void AddFiles(params string[] files)
        {
            foreach (var fileName in files)
            {
                if (!m_Files.ContainsKey(fileName))
                {
                    m_Files.Add(fileName, new File(null, fileName) { LastWriteTime = SystemClock.Instance.GetCurrentInstant() });
                }
            }
            UpdateCurrentState();
        }

        public void RemoveFile(string file) => RemoveFiles(file);

        public void RemoveFiles(params string[] files)
        {
            foreach (var fileName in files)
            {
                m_Files.Remove(fileName);
            }
            UpdateCurrentState();
        }


        public IFileSystemSnapshot CreateSnapshot()
        {
            UpdateCurrentState();
            return m_Group.GetHistoryService()[m_Name].CreateSnapshot(CurrentState);
        }

        public IFileSystemHistory GetHistory()
        {
            return m_Group.GetHistoryService()[m_Name];
        }


        void UpdateCurrentState()
        {
            var dir = new Directory(null, "root");
            foreach (var file in m_Files.Values)
            {
                dir.Add(d => file.WithParent(d));
            }
            CurrentState = dir;
        }
    }
}