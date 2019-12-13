// <copyright file="LogTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Logging;

    /// <summary>
    /// Test Log class
    /// </summary>
    [TestClass]
    public class LogTests
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// String builder
        /// </summary>
        private StringBuilder stringBuilder;

        /// <summary>
        /// Text writer trace listener
        /// </summary>
        private TextWriterTraceListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogTests"/> class.
        /// </summary>
        public LogTests()
        {
            // setup trace listener
            this.stringBuilder = new StringBuilder();
            TextWriter writer = new StringWriter(this.stringBuilder);
            this.listener = new TextWriterTraceListener(writer);
            this.log = new Log(LogDestination.TraceSource, Log.DefaultCategoryName, this.listener);
        }

        /// <summary>
        /// Test error reporting of an exception
        /// </summary>
        [TestMethod]
        public void ExceptionToError()
        {
            // create a new exception
            Exception e1 = new Exception("exception1");

            // issue the alerts call
            this.log.LogError(e1);

            // flush the listener
            this.listener.Flush();

            // verify the string has it
            string trace = this.stringBuilder.ToString();
            Assert.IsTrue(trace.Contains(e1.Message));
        }

        /// <summary>
        /// Test error reporting of a null exception
        /// </summary>
        [TestMethod]
        public void NullExceptionToError()
        {
            // create a null exception
            Exception e1 = null;

            // issue the alerts call
            this.log.LogError(e1);

            // flush the listener
            this.listener.Flush();

            // verify the string has it
            string trace = this.stringBuilder.ToString();
            Assert.IsTrue(trace.Contains("NullExceptionToError"));
        }

        /// <summary>
        /// Test error reporting of an inner exception
        /// </summary>
        [TestMethod]
        public void InnerExceptionToError()
        {
            try
            {
                try
                {
                    try
                    {
                        // create an initial exception1
                        throw new Exception("exception1");
                    }
                    catch (Exception e)
                    {
                        // make exception1 the inner for exception2
                        throw new Exception("exception2", e);
                    }
                }
                catch (Exception e)
                {
                    // make exception2 the inner for exception3
                    throw new Exception("exception3", e);
                }
            }
            catch (Exception e)
            {
                this.log.LogError(e);

                // flush the listener
                this.listener.Flush();

                // verify the string has it
                string trace = this.stringBuilder.ToString();
                Assert.IsTrue(trace.Contains("exception3"));
                Assert.IsTrue(trace.Contains("exception2"));
                Assert.IsTrue(trace.Contains("exception1"));
            }
        }

        /// <summary>
        /// Test error reporting of an aggregate exception as an inner exception
        /// </summary>
        [TestMethod]
        public void AggregateInnerExceptionToError()
        {
            try
            {
                // create a new exception
                try
                {
                    // create a new aggregate exception
                    List<Exception> exceptions = new List<Exception>();
                    Exception e1 = new Exception("exception1");
                    Exception e2 = new Exception("exception2");
                    exceptions.Add(e1);
                    exceptions.Add(e2);
                    AggregateException aggregateException = new AggregateException("aggregate1", exceptions);
                    throw aggregateException;
                }
                catch (Exception e)
                {
                    // make the aggregate exception the inner for exception2
                    throw new Exception("exception4", e);
                }
            }
            catch (Exception e)
            {
                this.log.LogError(e);

                // flush the listener
                this.listener.Flush();

                // verify the string has it
                string trace = this.stringBuilder.ToString();
                Assert.IsTrue(trace.Contains("exception1"));
                Assert.IsTrue(trace.Contains("exception2"));
                Assert.IsTrue(trace.Contains("aggregate1"));
                Assert.IsTrue(trace.Contains("exception4"));
            }
        }

        /// <summary>
        /// Test error reporting of an aggregate exception
        /// </summary>
        [TestMethod]
        public void AggregateExceptionToError()
        {
            // create a new aggregate exception
            List<Exception> exceptions = new List<Exception>();
            Exception e1 = new Exception("exception1");
            Exception e2 = new Exception("exception2");
            Exception e3 = new Exception("exception3");
            exceptions.Add(e1);
            exceptions.Add(e2);
            exceptions.Add(e3);
            AggregateException aggregateException = new AggregateException(exceptions);

            // issue the alerts call
            this.log.LogError(aggregateException);

            // flush the listener
            this.listener.Flush();

            // verify the string has it
            string trace = this.stringBuilder.ToString();
            Assert.IsTrue(trace.Contains(e1.Message));
            Assert.IsTrue(trace.Contains(e2.Message));
            Assert.IsTrue(trace.Contains(e3.Message));
        }
    }
}