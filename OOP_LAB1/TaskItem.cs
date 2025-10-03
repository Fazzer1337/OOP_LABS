using System;

namespace OOP_LAB1
{
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public DateTime DueTime { get; set; }
    }
}
