//-----------------------------------------------------------------------
// <copyright file="Entity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class Entity.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Entity class
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Gets partition key for the entity
        /// </summary>
        public string PartitionKey { get; internal set; }

        /// <summary>
        /// Gets or sets ETag condition for the entity
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets ETag for the entity after the action
        /// </summary>
        internal string CustomETag { get; set; }

        /// <summary>
        /// Gets or sets cache flags such as invalid and ETag flags
        /// </summary>
        internal CacheFlags CacheFlags { get; set; }

        /// <summary>
        /// Gets or sets expiry timestamp.
        /// If the current timestamp is greater than the expiry timestamp, the cache entity can be overwritten.
        /// </summary>
        internal DateTime CacheExpiry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the cache entity is invalid.
        /// The value for this field is calculated from cache status field.
        /// </summary>
        internal bool CacheInvalid
        {
            get
            {
                return this.HasCacheFlag(CacheFlags.Invalid);
            }

            set
            {
                this.SetCacheFlag(CacheFlags.Invalid, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cache has no ETag.
        /// The value for this field is calculated from cache status field
        /// </summary>
        internal bool NoETag
        {
            get
            {
                return this.HasCacheFlag(CacheFlags.NoETag);
            }

            set
            {
                this.SetCacheFlag(CacheFlags.NoETag, value);
            }
        }

        /// <summary>
        /// Clone entity
        /// </summary>
        /// <returns>Cloned entity</returns>
        public Entity Clone()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            object clonedObject = type.InvokeMember(string.Empty, System.Reflection.BindingFlags.CreateInstance, null, this, null);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    property.SetValue(clonedObject, property.GetValue(this, null), null);
                }
            }

            Entity clonedEntity = clonedObject as Entity;
            clonedEntity.CustomETag = this.CustomETag;
            clonedEntity.CacheFlags = this.CacheFlags;
            clonedEntity.CacheExpiry = this.CacheExpiry;
            return clonedEntity;
        }

        /// <summary>
        /// Set cache flag in entity
        /// </summary>
        /// <param name="cacheFlag">Cache flag to set</param>
        /// <param name="value">Cache flag value to set</param>
        private void SetCacheFlag(CacheFlags cacheFlag, bool value)
        {
            if (value)
            {
                this.CacheFlags |= cacheFlag;
            }
            else
            {
                this.CacheFlags &= ~cacheFlag;
            }
        }

        /// <summary>
        /// Returns a value indicating whether cache flag is set
        /// </summary>
        /// <param name="cacheFlag">Cache flag to query</param>
        /// <returns>A value indicating whether cache flag is set</returns>
        private bool HasCacheFlag(CacheFlags cacheFlag)
        {
            return this.CacheFlags.HasFlag(cacheFlag);
        }
    }
}
