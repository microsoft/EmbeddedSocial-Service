// <copyright file="WorkerRole.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.ServiceRuntime;
    using SocialPlus.Logging;
    using SocialPlus.Server.DependencyResolution;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Utils;
    using SocialPlus.Server.Workers;
    using StructureMap;

    /// <summary>
    /// Worker role class
    /// </summary>
    public class WorkerRole : RoleEntryPoint
    {
        /// <summary>
        /// Worker status check period
        /// </summary>
        private const int WorkerStatusCheckPeriod = 30000;

        /// <summary>
        /// Default limit of outgoing connections per endpoint.
        /// See the .NET documentation of the ServicePointManager class for more detail.
        /// </summary>
        private readonly int defaultConnectionLimit = 10000;

        /// <summary>
        /// Cancellation token source
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Run completed event
        /// </summary>
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        /// <summary>
        /// Dependency resolution container
        /// </summary>
        private IContainer container;

        /// <summary>
        /// Log
        /// </summary>
        private ILog log;

        /// <summary>
        /// Gets or sets queue workers
        /// </summary>
        protected IEnumerable<IWorker> Workers { get; set; }

        /// <summary>
        /// Gets or sets processor tasks
        /// </summary>
        protected List<Task> Tasks { get; set; }

        /// <summary>
        /// Worker role entry point
        /// </summary>
        public override void Run()
        {
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        /// <summary>
        /// Worker role on start event
        /// </summary>
        /// <returns>Result of on start</returns>
        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = this.defaultConnectionLimit;
            RoleEnvironment.Changing += this.RoleEnvironmentChanging;

            this.container = IoC.Initialize();
            this.log = this.container.GetInstance<ILog>();
            var registry = new WindowsRegistry(this.log);

            registry.ConfigureLocalTcpSettings();

            bool result = base.OnStart();
            return result;
        }

        /// <summary>
        /// Worker role on stop event
        /// </summary>
        public override void OnStop()
        {
            this.cancellationTokenSource.Cancel();

            foreach (var job in this.Workers)
            {
                job.Stop();
                var disposable = job as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            try
            {
                Task.WaitAll(this.Tasks.ToArray());
            }
            catch (AggregateException)
            {
                // Write log
            }

            this.runCompleteEvent.WaitOne();
            base.OnStop();
        }

        /// <summary>
        /// Run async
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Run async task</returns>
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            this.Workers = this.CreateWorkers();
            this.Tasks = new List<Task>();

            bool tableInit = false;
            Exception exception = null;
            try
            {
                // initialize ctstore before starting workers
                var ctStore = this.container.GetInstance<ICTStoreManager>();
                tableInit = await ctStore.Initialize();
            }
            catch (Exception e)
            {
                exception = e;
            }

            if (tableInit == false)
            {
                string errorMessage = "CTstore version number does not match the expected version number." + Environment.NewLine +
                    "If your tables are empty, then you probably forgot to provision storage." + Environment.NewLine +
                    "If not, then you need to convert the data format and update the storage version number.";
                this.log.LogException(errorMessage, exception);
            }

            foreach (var processor in this.Workers)
            {
                var task = Task.Factory.StartNew(processor.Run);
                this.Tasks.Add(task);
            }

            // Control and restart a faulted job
            while (!cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < this.Tasks.Count; i++)
                {
                    var task = this.Tasks[i];
                    if (task.IsFaulted)
                    {
                        // log information about the task exception
                        this.log.LogError(task.Exception);

                        var jobToRestart = this.Workers.ElementAt(i);
                        this.Tasks[i] = Task.Factory.StartNew(jobToRestart.Run);
                    }
                }

                await Task.Delay(WorkerStatusCheckPeriod);
            }
        }

        /// <summary>
        /// Role environment changing
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">Role environment changing event args</param>
        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Create workers
        /// </summary>
        /// <returns>List of workers</returns>
        private IEnumerable<IWorker> CreateWorkers()
        {
            return new IWorker[]
            {
                this.container.GetInstance<FanoutTopicsWorker>(),
                this.container.GetInstance<FanoutActivitiesWorker>(),
                this.container.GetInstance<FollowingImportsWorker>(),
                this.container.GetInstance<ResizeImagesWorker>(),
                this.container.GetInstance<LikesWorker>(),
                this.container.GetInstance<RelationshipsWorker>(),
                this.container.GetInstance<ExpirationsWorker>(),
                this.container.GetInstance<ReportsWorker>(),
                this.container.GetInstance<SearchWorker>()
            };
        }
    }
}
