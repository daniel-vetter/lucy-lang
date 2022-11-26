using Lucy.Common.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

/// <summary>
/// This class executes delegates in the background.
/// It keeps track of all running "jobs" so the system can be shutdown without any running jobs left.
/// </summary>
[Service(Lifetime.Singleton)]
public class JobRunner
{
    private readonly List<Task> _runningJobs = new List<Task>();
    private volatile bool _isRunning = true;

    /// <summary>
    /// Execute a function in the background
    /// </summary>
    public void Run(Func<Task> handler)
    {
        if (!_isRunning)
            throw new Exception("JobRunner can not longer accept jobs because it was closed");

        lock (_runningJobs)
        {
            if (!_isRunning)
                throw new Exception("JobRunner can not longer accept jobs because it was closed");

            var task = Task.Run(async () => await handler());
            _runningJobs.Add(task);

            task.ContinueWith(_ =>
            {
                lock (_runningJobs)
                    _runningJobs.Remove(task);
            });
        }
    }

    /// <summary>
    /// Stops processing new jobs. This method will return as soon as all jobs are done.
    /// </summary>
    public async Task CloseAndWaitTillAllJobsAreDone()
    {
        Task[] remainingTasks;
        lock (_runningJobs)
        {
            _isRunning = false;
            remainingTasks = _runningJobs.ToArray();
        }
        await Task.WhenAll(remainingTasks);
    }
}