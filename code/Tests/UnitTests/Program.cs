// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Logging;
    using SocialPlus.Server.Email;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Run unit tests from the command line
    /// </summary>
    public class Program
    {
        // List of classes to test
        private static readonly List<string> TestClasses = new List<string>()
        {
            "KVTests",
        };

        // how long should we wait until we abandon a test and move on? (seconds)
        private static int secsPerTest = 600;

        // should we send an email when a test fails?
        private static bool sendEmailOnTestFailure = true;

        /// <summary>
        /// Runs tests one-by-one in a loop
        /// </summary>
        /// <param name="args">Argument list</param>
        private static void Main(string[] args)
        {
            ParseArgs(args);

            bool hasTestFailed = false;
            Email msg = new Email();

            foreach (var v in TestClasses)
            {
                var kvFactory = new KVFactory();
                foreach (IKV kv in kvFactory.CreateKV())
                {
                    // Make a test class object
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var currentNamespace = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
                    object[] constructorParams = new object[1] { kv };
                    object classInstance = ReflectionUtils.CreateClassInstance(currentAssembly, currentNamespace, v, constructorParams);

                    if (classInstance == null)
                    {
                        Console.WriteLine("Error: couldn't find the class with the parameter types specified!!!");
                        throw new Exception("Could not find test class with the parameter types specified in code!");
                    }

                    // For each method of the class labelled with TestMethod attribute
                    foreach (var testMethod in ReflectionUtils.GetMethodsWithTypeTAttribute(classInstance.GetType(), typeof(TestMethodAttribute)))
                    {
                        DateTime testStartTime = DateTime.Now;

                        Console.WriteLine(testMethod.Name + " started against " + TestUtilities.PrettyServerName(TestConstants.ServerApiBaseUrl));
                        try
                        {
                            Task test = (Task)testMethod.Invoke(classInstance, null);

                            // Check whether the test terminates on time
                            if (test.Wait(secsPerTest * 1000))
                            {
                                Console.WriteLine(testMethod.Name + " finished within {0} secs.", DateTime.Now.Subtract(testStartTime).Seconds);
                            }
                            else
                            {
                                Console.WriteLine(testMethod.Name + " has not finished within {0} secs. Moved on.", secsPerTest);
                                throw new Exception(string.Format(testMethod.Name + " has not finished within {0} secs. Moved on.", secsPerTest));
                            }
                        }
                        catch (Exception ex)
                        {
                            // Write the error to the console
                            Console.WriteLine(ex.ToString());

                            // If this test is the first failed test in this run, create a new email, otherwise just appent to the message
                            if (!hasTestFailed)
                            {
                                msg.Subject = "Failed unit tests";
                                msg.To = new List<string>() { TestConstants.FailedTestsEmail };
                                msg.Category = "Unit Testing";
                                msg.HtmlBody = TestUtilities.Exception2HtmlEmail(testMethod.Name, ex);
                            }
                            else
                            {
                                msg.HtmlBody += "<br><br>" + TestUtilities.Exception2HtmlEmail(testMethod.Name, ex);
                            }

                            hasTestFailed = true;
                        }
                    }
                }
            }

            // If any tests failed during this run, check whether we should send e-mail about them
            if (hasTestFailed && sendEmailOnTestFailure)
            {
                string configFilePath = ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName;
                string sendGridKey = TestUtilities.GetSendGridKey(configFilePath);

                try
                {
                    var log = new Log(LogDestination.Console, Log.DefaultCategoryName);
                    SendGridEmail sendingClient = new SendGridEmail(log, sendGridKey);
                    sendingClient.SendEmail(msg).Wait();
                }
                catch (Exception e)
                {
                    // SendEmail failed. Report the error on the console and move on.
                    System.ConsoleColor oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Encountered an error during SendEmail:");
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null)
                    {
                        Console.WriteLine(e.InnerException.Message);
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine();
                    Console.WriteLine("Tried to send the following message:");
                    Console.WriteLine(msg.Subject);
                    Console.WriteLine(msg.HtmlBody);
                    Console.ForegroundColor = oldColor;
                }
            }
        }

        /// <summary>
        /// Prints the usage
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("Options: ");
            Console.WriteLine("-t=XXX\t How long to wait for completion of invidual test. Default = {0} (secs)", secsPerTest);
            Console.WriteLine("-s\t Silent mode - doesn't send any emails when a test fails.");
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Quick-and-dirty argument parsing to our program
        /// </summary>
        /// <param name="args">passed-in arguments</param>
        private static void ParseArgs(string[] args)
        {
            // if no arguments, print usage and exit
            if (args.Length == 0)
            {
                PrintUsage();
                Environment.Exit(0);
            }

            try
            {
                // quick-and-dirty parsing of the arguments
                foreach (string arg in args)
                {
                    string[] words = arg.Split('=');
                    if (words[0] == "-t")
                    {
                        secsPerTest = int.Parse(words[1]);
                    }
                    else if (words[0] == "-s")
                    {
                        sendEmailOnTestFailure = false;
                    }
                    else
                    {
                        PrintUsage();
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception)
            {
                PrintUsage();
                Environment.Exit(0);
            }
        }
    }
}
