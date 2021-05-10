namespace PDMDatabase.MemoryDatabase
{
    public interface IInMemoryTable
    {
        long Version { get; set; }

        void LoadData();
        int GetNum();
        void SetTracerEnabled(bool enabled);
    }
}