﻿namespace Model
{
    public class ReportDisplayInfo
    {
        public int? ProjectID { get; set; }
        public string ProjectName
        {
            get; set;
        }

        public int CurrentTask { get; set; }
        public int LastTask { get; set; }
        public int CurrentTime { get; set; }
        public int LastTime { get; set; }
        public int CurrentModify { get; set; }
        public int LastModify { get; set; }
        public int CurrentAvg { get; set; }
        public int LastAvg { get; set; }
    }
}