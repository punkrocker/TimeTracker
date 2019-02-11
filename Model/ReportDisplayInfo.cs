namespace Model
{
    public class ReportDisplayInfo
    {
        public int? ProjectId { get; set; }
        public string ProjectName
        {
            get; set;
        }
        public string TeamName { get; set;  }
        public int TeamId { get; set; }
        public int CurrentTask { get; set; }
        public int LastTask { get; set; }
        public int CurrentTime { get; set; }
        public int LastTime { get; set; }
        public int CurrentModify { get; set; }
        public int LastModify { get; set; }
        public double CurrentAvg { get; set; }
        public double LastAvg { get; set; }
    }
}