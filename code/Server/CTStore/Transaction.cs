//-----------------------------------------------------------------------
// <copyright file="Transaction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class Transaction.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Transaction class
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Container name for the transaction
        /// </summary>
        private string containerName;

        /// <summary>
        /// Partition key for transaction
        /// </summary>
        private string partitionKey;

        /// <summary>
        /// Transaction operations
        /// </summary>
        private List<Operation> operations;

        /// <summary>
        /// Cache only operations in the transaction
        /// </summary>
        private List<Operation> cacheOnlyOperations;

        /// <summary>
        /// Persistent only operations in the transaction
        /// </summary>
        private List<Operation> persistentOnlyOperations;

        /// <summary>
        /// Default operations in the transaction
        /// </summary>
        private List<Operation> defaultOperations;

        /// <summary>
        /// Indices of default operations in the list of operations
        /// </summary>
        private List<int> defaultOperationIndices;

        /// <summary>
        /// Entity keys (container name + table name + partition key + key + [Item key]) for operations in the transaction.
        /// This hash set is used to check if there is more than one operation with the same entity.
        /// Only one operation is allowed per entity in a transaction.
        /// </summary>
        private HashSet<string> entityKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class
        /// </summary>
        public Transaction()
        {
            this.operations = new List<Operation>();
            this.cacheOnlyOperations = new List<Operation>();
            this.persistentOnlyOperations = new List<Operation>();
            this.defaultOperations = new List<Operation>();
            this.defaultOperationIndices = new List<int>();
            this.entityKeys = new HashSet<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class
        /// </summary>
        /// <param name="operations">List of operations</param>
        public Transaction(List<Operation> operations)
            : this()
        {
            foreach (Operation operation in operations)
            {
                this.Add(operation);
            }
        }

        /// <summary>
        /// Gets transaction operations
        /// </summary>
        public List<Operation> Operations
        {
            get
            {
                return this.operations;
            }
        }

        /// <summary>
        /// Gets cache-only operations in transaction
        /// </summary>
        public List<Operation> CacheOnlyOperations
        {
            get
            {
                return this.cacheOnlyOperations;
            }
        }

        /// <summary>
        /// Gets persistent-only operations in transaction
        /// </summary>
        public List<Operation> PersistentOnlyOperations
        {
            get
            {
                return this.persistentOnlyOperations;
            }
        }

        /// <summary>
        /// Gets default operations in transaction
        /// </summary>
        public List<Operation> DefaultOperations
        {
            get
            {
                return this.defaultOperations;
            }
        }

        /// <summary>
        /// Gets default operation indices in transaction
        /// </summary>
        public List<int> DefaultOperationIndices
        {
            get
            {
                return this.defaultOperationIndices;
            }
        }

        /// <summary>
        /// Gets container name for the transaction
        /// </summary>
        public string ContainerName
        {
            get
            {
                return this.containerName;
            }
        }

        /// <summary>
        /// Gets partition key for the transaction
        /// </summary>
        public string PartitionKey
        {
            get
            {
                return this.partitionKey;
            }
        }

        /// <summary>
        /// Add operation to transaction
        /// </summary>
        /// <param name="operation">Table operation</param>
        public void Add(Operation operation)
        {
            if (this.containerName == null)
            {
                this.containerName = operation.Table.ContainerName;
                this.partitionKey = operation.PartitionKey;
            }
            else
            {
                if (!this.containerName.Equals(operation.Table.ContainerName))
                {
                    throw new ArgumentException("All operations in a transaction should be in the same container.");
                }

                if (this.partitionKey != operation.PartitionKey)
                {
                    throw new ArgumentException("All operations in a transaction should have the same partition key.");
                }
            }

            string entityKey = this.GetEntityKey(operation);
            if (!this.entityKeys.Contains(entityKey))
            {
                this.entityKeys.Add(entityKey);
            }
            else
            {
                throw new ArgumentException("An entity can appear only once in the transaction.");
            }

            // A transaction can either have all CacheOnly operations or no CacheOnly operation
            // CacheOnly operation cannot be combined with non-CacheOnly operations
            if ((operation.Table.StorageMode == StorageMode.CacheOnly
                && (this.operations.Count - this.cacheOnlyOperations.Count > 0))
                || (operation.Table.StorageMode != StorageMode.CacheOnly
                && this.cacheOnlyOperations.Count > 0))
            {
                throw new ArgumentException("CacheOnly operations cannot be combined with non-CacheOnly operations in a transaction.");
            }

            if (operation.Table.StorageMode == StorageMode.CacheOnly)
            {
                this.cacheOnlyOperations.Add(operation);
            }
            else if (operation.Table.StorageMode == StorageMode.PersistentOnly)
            {
                this.persistentOnlyOperations.Add(operation);
            }
            else if (operation.Table.StorageMode == StorageMode.Default)
            {
                this.defaultOperations.Add(operation);
                this.defaultOperationIndices.Add(this.operations.Count);
            }

            this.operations.Add(operation);
        }

        /// <summary>
        /// Get unique key for the entity the operation is operating on
        /// </summary>
        /// <param name="operation">Store operation</param>
        /// <returns>Unique entity key</returns>
        private string GetEntityKey(Operation operation)
        {
            return string.Join(
                ":",
                operation.Table.ContainerName,
                operation.Table.TableName,
                operation.PartitionKey,
                operation.Key,
                operation.ItemKey);
        }
    }
}
