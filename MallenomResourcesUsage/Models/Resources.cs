namespace MallenomResourcesUsage.Models
{
    public class Resources
    {
        public string? CPULoading { get; set; }
        public string? CPULoadingOnBar
        {
            get { return "width: " + CPULoading + "%";}
        }

        public string? DisksInformation { get; set; }
        public List<Drive> Drives { get; set; } = new List<Drive>();

        public double FreeRAM { get; set; }
        public double TotalRAM { get; set; }
        public double UsedRAM { get; set; }

        public string? UsedRAMOnBar
        {
            get { return "width: " + (Convert.ToInt32((UsedRAM / TotalRAM) * 100)).ToString() + "%"; }
        }

    }
    public class Drive
    {
        public string? Name { get; set; }
        public string? FullSize { get; set; }
        public string? FreeSize { get; set; }

        public string? UsedSize { get; set; }

        public string? UsedSizeOnBar { get; set; }
      
        public Drive(string _name, string _freesize, string _fullsize, string _usedsize, string _usedsizeonbar)
        {
            Name = _name;
            FullSize = _fullsize;
            FreeSize = _freesize;
            UsedSize = _usedsize;
            UsedSizeOnBar = _usedsizeonbar;
        }

    }

}
