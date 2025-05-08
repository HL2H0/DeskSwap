namespace DeskSwap
{
    class DeskSwapProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        // public Dictionary<string, bool> Folders { get; set; } = new();
        // public Dictionary<string, bool> Files { get; set; } = new();
        
        public string[] Folders { get; set; } = [];
        public string[] Files { get; set; } = [];
        public Dictionary<string, string> OriginalPaths { get; set; } = [];


        public DeskSwapProfile(string name, string description, string[] folders, string[] files, Dictionary<string,string> originalPaths)
        {
            Name = name;
            Description = description;
            Folders = folders;
            Files = files;
            OriginalPaths = originalPaths;
        }
    }
}
