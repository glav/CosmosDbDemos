using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosCommon.Helpers
{
    public class AsyncRunner
    {
        private List<Task> _taskList = new List<Task>();
        public void AddTaskToRunConcurrently(Task taskToRun)
        {
            _taskList.Add(taskToRun);
        }

        public async Task RunAllTasks()
        {
            await Task.WhenAll(_taskList.ToArray());
            _taskList.Clear();
        }

        public void ClearTasks()
        {
            _taskList.Clear();
        }
    }
}
