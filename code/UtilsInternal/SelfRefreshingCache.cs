// <copyright file="SelfRefreshingCache.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Timers;

    /// <summary>
    /// Implements a cache whose elements are refreshed periodically.
    /// This cache takes in a refresh frequency, a per-key refresh method, and an action.
    /// It sets up a timer that fires at a rate equal to the refresh frequency.
    /// When the timer fires, the refresh method is called on each key present in the dictionary.
    /// The action is used for error tracking (alerting); the action is called each time
    /// an exception fires in the refresh method.
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the dictionary</typeparam>
    /// <typeparam name="TValue">Tye type of the value in the dictionary</typeparam>
    public class SelfRefreshingCache<TKey, TValue> : IDisposable
    {
        /// <summary>
        /// Data dictionary that starts out empty
        /// </summary>
        private readonly ConcurrentDictionary<TKey, TValue> dataDictionary = new ConcurrentDictionary<TKey, TValue>();

        /// <summary>
        /// Passed-in code on how to refresh a key
        /// </summary>
        private readonly Func<TKey, Task<TValue>> perKeyRefreshMethod;

        /// <summary>
        /// Frequency at which the keys must be refreshed
        /// </summary>
        private readonly TimeSpan frequency;

        /// <summary>
        /// Internal timer
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// Used to generate alerts
        /// </summary>
        private readonly Action<string> alertFunction;

        /// <summary>
        /// Flag on whether disposed has been called
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfRefreshingCache{TKey, TValue}"/> class.
        /// This cache is initally empty. On first GetValueAsync call, it will invoke the getDataFunction.
        /// </summary>
        /// <param name="frequency">How frequently should the cache be updated</param>
        /// <param name="perKeyRefreshMethod">What is the function to use to update a cached entry</param>
        /// <param name="alertFunction">What is the function used to generate alerts</param>
        public SelfRefreshingCache(TimeSpan frequency, Func<TKey, Task<TValue>> perKeyRefreshMethod, Action<string> alertFunction = null)
        {
            this.frequency = frequency;
            this.perKeyRefreshMethod = perKeyRefreshMethod;
            this.alertFunction = alertFunction;

            // Start timer
            this.timer = new Timer(frequency.TotalMilliseconds);
            this.timer.Elapsed += this.RefreshDataDictionaryAsync;
            this.timer.Enabled = true;
        }

        /// <summary>
        /// Gets the value from the dictionary given the key, and if not found, calls
        /// the per-key refresh method, inserts the value into the cache, and returns it
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The value corresponding to the key</returns>
        public async Task<TValue> GetValueAsync(TKey key)
        {
            TValue result;

            if (!this.dataDictionary.TryGetValue(key, out result))
            {
                result = await this.perKeyRefreshMethod(key);
                this.dataDictionary[key] = result;
            }

            return result;
        }

        /// <summary>
        /// Implements the dispose method
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Diposes the timer
        /// </summary>
        /// <param name="disposing">Whether disposing or not</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (this.timer != null)
                {
                    this.timer.Dispose();
                }
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// Refreshes the internal dictionary
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">The arguments</param>
        private async void RefreshDataDictionaryAsync(object sender, EventArgs args)
        {
            foreach (var key in this.dataDictionary.Keys)
            {
                try
                {
                    var result = await this.perKeyRefreshMethod(key);
                    this.dataDictionary[key] = result;
                }
                catch (Exception e)
                {
                    // Don't let timer thread crash service if exception is thrown.
                    if (this.alertFunction != null)
                    {
                        this.alertFunction(string.Format("Failed to refresh cache key {0} with error: {1}", key.ToString(), e.Message));
                    }
                }
            }
        }
    }
}