namespace CheatingReminder
{
    using System;
    using System.Diagnostics;

    public class Task
    {
        private string name;

        public Task(string taskName)
        {
            this.Name = taskName;
            this.Process = null;
            this.IsChecked = false;
        }
        public Task(string taskName, Process process)
        {
            this.Name = taskName;
            this.Process = process;
            this.IsChecked = false;
        }

        public string Name 
        {
            get
            {
                return this.name;
            }
            private set 
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("Name of the task cannot be null.");
                }

                this.name = value;
            }
        }
        public Process Process { get; set; }
        public bool IsChecked { get; set; }
    }
}