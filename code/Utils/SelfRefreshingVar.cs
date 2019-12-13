// <copyright file="SelfRefreshingVar.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A self refreshing variable (SRV) is a variable that refreshes itself periodically.
    /// Upon creation, the caller provides a refresh callback and an interval, and we invoke this
    /// callback periodically at the specified interval.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By design, the refresh logic does not depend on the current value of the SRV.
    /// This code is thread safe, in that reading and updating the value is protected by a lock.
    /// However, the code does not enforce ordering:  it makes no guarantee that a variable update
    /// from Initialize() happens before a variable update from the timer, and it makes no guarantee
    /// that multiple updates from timers are applied in the order that they are invoked.
    /// </para>
    /// <para>
    /// We provide two constructors: one that initializes the value and one that leaves the value
    /// un-initialized. When creating an un-initialized variable, the caller is responsible for
    /// calling Initialize(). If the caller forgets, it's possible for the variable to remain
    /// un-initialized until the first call to refresh.  We make no guarantees on the value returned
    /// by GetValue() during the un-initialized period.
    /// </para>
    /// <para>
    /// Examples of uses are configuration variables that refresh periodically by reading
    /// a configuration file, or state variables that refresh by reading a value from a table.
    /// We support the un-initialized constructor for the following usage pattern: when the
    /// caller creates a self-refreshing variable inside a constructor, and the variable's initial
    /// value is computed from the result of an async operation. Because async methods cannot be
    /// called from a constructor, the caller creates the SRV uninitialized, and calls Initialize()
    /// before the first time it needs to read the variable. Initialize() is an async call, but it
    /// can then be located outside of the constructor, thereby avoiding the problem.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the variable</typeparam>
    public class SelfRefreshingVar<T> : IDisposable
    {
        /// <summary>
        /// Callback invoked when variable must be refreshed
        /// </summary>
        private readonly Func<Task<T>> refreshCallback;

        /// <summary>
        /// Callback invoked when executing the refreshCallback generates an exception
        /// </summary>
        private readonly Action<string, Exception> alertCallback;

        /// <summary>
        /// Object used for a lock
        /// </summary>
        private readonly object lockInstance = new object();

        /// <summary>
        /// The value of the self-refreshing variable
        /// </summary>
        private T value;

        /// <summary>
        /// Flag indicating whether disposed has been called
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Internal timer used to implement wakeup when is time to refresh the variable
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfRefreshingVar{T}"/> class and the variable.
        /// </summary>
        /// <remarks>This constructor creates an initialized self-refreshing variable</remarks>
        /// <param name="initValue">Initial value for the variable</param>
        /// <param name="interval">Refreshing interval</param>
        /// <param name="refreshCallback">Callback to invoke to refresh the value</param>
        /// <param name="alertCallback">Alerting function if something goes wrong on refreshing</param>
        public SelfRefreshingVar(T initValue, TimeSpan interval, Func<Task<T>> refreshCallback, Action<string, Exception> alertCallback)
        {
            // We do not need to lock this write of the value because it's
            // done in the constructor *before* the call to StartTimer
            this.value = initValue;

            this.refreshCallback = refreshCallback;
            this.alertCallback = alertCallback;
            this.StartTimer(interval);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfRefreshingVar{T}"/> class and the variable.
        /// </summary>
        /// <remarks>This constructor creates an un-initialized self-refreshing variable</remarks>
        /// <param name="interval">Refreshing interval</param>
        /// <param name="refreshCallback">Callback to invoke to refresh the value</param>
        /// <param name="alertCallback">Alerting function if something goes wrong on refreshing</param>
        public SelfRefreshingVar(TimeSpan interval, Func<Task<T>> refreshCallback, Action<string, Exception> alertCallback)
        {
            this.refreshCallback = refreshCallback;
            this.alertCallback = alertCallback;
            this.StartTimer(interval);
        }

        /// <summary>
        /// Gets value of self-refreshing variable.
        /// </summary>
        /// <returns>variable's value</returns>
        public T GetValue()
        {
            lock (this.lockInstance)
            {
                return this.value;
            }
        }

        /// <summary>
        /// Implements the dispose method
        /// </summary>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            // If Dispose is called by two different threads, it is possible that this.timer is already null.
            // If not null, call its dispose
            this.timer?.Dispose();
            this.isDisposed = true;

            // Inform the GC that the self-refreshing variable state can be garbage collected
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize an un-initialized variable.
        /// </summary>
        /// <returns>task</returns>
        public async Task Initialize()
        {
            try
            {
                T temp = await this.refreshCallback();
                lock (this.lockInstance)
                {
                    this.value = temp;
                }
            }
            catch (Exception ex)
            {
                this.alertCallback?.Invoke($"Failed to initialize self-refreshing variable with value {this.value}", ex);
            }
        }

        /// <summary>
        /// Refreshes the value of the self-refreshing variable by calling the refresh callback.
        /// This call blocks until refresh is complete. The elapsed timer runs on a thread pool thread.
        /// </summary>
        /// <param name="state">state (unused)</param>
        private void RefreshData(object state)
        {
            try
            {
                T temp = this.refreshCallback().Result;
                lock (this)
                {
                    this.value = temp;
                }
            }
            catch (Exception ex)
            {
                // Don't let timer thread crash service if exception is thrown
                this.alertCallback?.Invoke($"Failed to refresh self-refreshing variable with value {this.value}", ex);
            }
        }

        /// <summary>
        /// Starts the timer. Upon each tick, the self-refreshing variable is refreshed
        /// </summary>
        /// <param name="interval">timer's interval</param>
        private void StartTimer(TimeSpan interval)
        {
            TimerCallback timerCallback = new TimerCallback(this.RefreshData);
            this.timer = new Timer(timerCallback, state: null, dueTime: interval, period: interval);
        }
    }
}
