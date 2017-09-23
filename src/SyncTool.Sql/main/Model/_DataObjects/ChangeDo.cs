namespace SyncTool.Sql.Model
{
    class ChangeDo
    {
        public int FileId { get; set; }

        public int? CurrentId { get; set; }

        public int? PreviousId { get; set; }

        public long? CurrentLastWriteTimeTicks { get; set; }

        public long? PreviousLastWriteTimeTicks { get; set; }

        public long? CurrentLength { get; set; }

        public long? PreviousLength { get; set; }

        public void Deconstruct(out FileInstanceDo previous, out FileInstanceDo current, out int fileId)
        {
            previous = PreviousId.HasValue
                ? new FileInstanceDo() { Id = PreviousId.Value, LastWriteTimeTicks = PreviousLastWriteTimeTicks.Value, Length = PreviousLength.Value }
                : null;

            current = CurrentId.HasValue
                ? new FileInstanceDo() { Id = CurrentId.Value, LastWriteTimeTicks = CurrentLastWriteTimeTicks.Value, Length = CurrentLength.Value }
                : null;

            fileId = FileId;
        }
    }
}