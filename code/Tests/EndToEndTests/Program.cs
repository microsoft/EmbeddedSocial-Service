// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.EndToEndTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Email;
    using SocialPlus.TestUtils;

    /// <summary>
    /// Run tests from command line. The code here users reflection to discover all TestMethods present
    /// in the EndToEndTests. Each test method discovered is executed if present in the list of passing tests.
    /// Each test is waited on to finish in no longer than <c>secsPerTest</c>.
    /// At the end of each run, an e-mail is sent if (1) any of the tests failed and (2) the list of failed tests
    /// include a test for which no e-mail was sent recently (configured using variable <c>silencePerDuplicateAlert</c>).
    /// The code than sleeps for <c>secsPerDelay</c> and then starts again.
    /// </summary>
    public class Program
    {
        // List of passing tests as of March 2017
        private static readonly List<string> PassingTests = new List<string>()
        {
            "CreateDeleteUserTest",
            "CreateMultipleDeleteMultipleUsersTest",
            "CheckUserHandlesCaseSensitivity",
            "UpdateUserPhotoTest",
            "GetLinkAccountsTest",
            "BlobTest_PostAndGet",
            "GetTopicsForUserTest",
            "GetTopicsForUserUsingHandleTest",
            "HashTagsAutoCompleteTest",
            "GetPutCountNotificationTest",   // uses sbq
            "CreateDeleteSessionTest",
            "AnonGetUserTest",

            // PinTests
            "PinUnPinDeletePinTest",
            "GetPinFeedTest",

            // SocialFollowTests
            "SocialFollowingUserTest",
            "SocialFollowUnfollowTest",      // uses sbq
            "SocialUnfollowTopicTest",       // uses sbq
            "SocialFollowPrivateUserTest",
            "SocialFollowPrivateUserRejectTest",
            "SocialDeleteFollowTest",
            "SocialBlockUnblockUserTest",

            // LikeTests
            "LikeTopicTest",
            "RapidLikeTopicTest",
            "RapidLikeUnlikeTopicTest",
            "GetLikesTopicTest",
            "LikeCommentReplyDeleteTest",

            // BatchTests
            "BatchTestMultiGetTopics",
            "BatchTestMultiPostUsers",

            // ImageTests
            "TopicImage",
            "UserImage",
            "AppIcon",
            "TinyImage",
            "LocalSmallImage",
            "LocalMediumImage",
            "LargePostOnFirstRequest",

            // TopicTests
            "CreateVerifyDeleteTopicTest",
            "GetTopicsTest",
            "PutTopicTest",
            "GetTopicsFromManyUsersTest",
            "CheckTopicHandlesCaseSensitivity",

            // CommentsTests
            "PostGetLikeDeleteCommentTest",
            "AnonymousReadCommentTest",

            // RepliesTests
            "PostGetRepliesTest",
            "GetRepliesForACommentTest",
            "DeleteRepliesTest",

            // BatchHandlerTests
            "IssueNonBatch",
            "IssueBatch",
            "IssueBadBatch",
            "IssueTwoBatches",
            "IssueManyBatches",

            // AAD tests (you must have the appropriate certs installed for these tests to pass)
            "CreateDeleteUserWithValidAADTokenValidSPClient",
            "CreateDeleteUserWithInvalidAADToken",
            "CreateDeleteUserWithValidAADTokenInvalidAudience",

            // app published topics tests
            "AppPublishedCreateVerifyDeleteTopicTest",
            "AppPublishedPinUnpinTest",
            "AppPublishedGetPinFeedTest",
            "AppPublishedLikeTopicTest",
            "AppPublishedGetLikesTopicTest",
            "AppPublishedLikeCommentReplyDeleteTest",
            "AppPublishedGetPutCountNotificationTest",
            "AppPublishedTopicSearchTest",

            // named topics tests
            "CreateVerifyDeleteNamedTopicTest",
            "CreateUpdateDeleteNamedTopicTest",

            // SearchTests
            "SearchUsersTest",
            "SearchUpdatedTopicTest",
            "SearchTopicsTestUsingEmptyString",

            // ActivityTests (all these use SBQ)
            "FollowingActivityTest",
            "CommentActivityTest",
            "ReplyActivityTest",
            "LikeActivityTest",

            // FollowingTopicTests (all these use SBQ)
            "FollowingTopicActivityTest",
            "FollowingTopicCommentActivityTest",
            "FollowingTopicReplyActivityTest",
            "FollowingTopicLikeActivityTest",
            "UnfollowNotFollowedTopicTest",

            // CommentNotificationTests
            "CommentNotification",

            // ClientNameAndConfigTests
            "CreateVerifyDeleteClientNameAndConfigTest"
        };

        // Partial list of failing tests:
        // "PopularTopicsTest"
        // "GetTopicTestWithEmptyString"
        // "SearchTopicsTest"
        // "ConcurrentLikeCommentTest"

        // cache the time when test failed and notification was generated
        private static readonly ConcurrentDictionary<string, DateTime> CacheLastFailedTimeWithNotification = new ConcurrentDictionary<string, DateTime>();

        // how many runs of tests? 0 means run forever
        private static int numberOfRuns = 0;

        // how long should we wait until we abandon a test and move on? (seconds)
        private static int secsPerTest = 600;

        // how long should we wait until new test run starts? (seconds)
        private static int secsPerDelay = 60 * 60;

        // how long should we wait until sending a duplicate alert? (hours)
        private static int silencePerDuplicateAlert = 12;

        // should we send an email when a test fails?
        private static bool sendEmailOnTestFailure = true;

        // should we run the tests concurrently?
        private static bool runConcurrently = false;

        // allows user to run single test by specifying the test name on the command line
        private static string singleTestName = null;

        /// <summary>
        /// Run tests one-by-one or concurrently in a loop
        /// </summary>
        /// <param name="args">Argument list</param>
        private static void Main(string[] args)
        {
            // Make tests use TLS 1.2 for their connections if the other endpoint supports it.
            // This line can be removed once we upgrade to using .NET 4.6 or higher (although it should cause no harm)
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // Disable the use of SSL 3.0 because it's not longer considered secure.
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;

            ParseArgs(args);

            // When number of runs is set to 0, run "forever"
            if (numberOfRuns == 0)
            {
                numberOfRuns = int.MaxValue;
            }

            ExecuteTests(runConcurrently);
        }

        /// <summary>
        /// Runs tests one-by-one in a loop or concurrently based on the flag
        /// </summary>
        /// <param name="runConcurrently">true to run it concurrently</param>
        private static void ExecuteTests(bool runConcurrently = false)
        {
            // Loop numberOfRuns times. This code could probably be optimized to use
            // reflection once, rather than each time we loop.
            for (int i = 0; i < numberOfRuns; i += 1)
            {
                bool hasTestFailed = false;
                List<Task> testResultTasks = new List<Task>();
                ConcurrentBag<string> testResultMessages = new ConcurrentBag<string>();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                List<string> testsToRun;
                if (singleTestName != null)
                {
                    testsToRun = new List<string>();
                    testsToRun.Add(singleTestName);
                }
                else
                {
                    testsToRun = PassingTests;
                }

                foreach (Tuple<object, MethodInfo> classesAndMethods in ReflectionUtils.GetClassMethodTests(testsToRun))
                {
                    object classInstance = classesAndMethods.Item1;
                    MethodInfo methodInfo = classesAndMethods.Item2;
                    DateTime testStartTime = DateTime.Now;
                    Console.WriteLine("Run #" + i + " " + methodInfo.Name + " started against " + TestUtilities.PrettyServerName(TestConstants.ServerApiBaseUrl));

                    // A test with a timeout wrapped in a task
                    Task testTask = ((Task)methodInfo.Invoke(classInstance, null)).TimeoutAfter(secsPerTest * 1000);

                    // A test result task that continues on completion of test task
                    Task testResultTask = testTask.ContinueWith(
                        testResult =>
                        {
                            // Test cancelled with timeout
                            if (testResult.IsCanceled)
                            {
                                Console.WriteLine(methodInfo.Name + " has not finished within {0} secs. Moved on. Exception {1}.", secsPerTest, new Exception(string.Format(methodInfo.Name + " has not finished within {0} secs. Moved on.", secsPerTest)));
                            }
                            else if (testResult.IsFaulted)
                            {
                                // Test threw an exception
                                Console.WriteLine(methodInfo.Name + " finished within {0} secs. Exception {1}.", DateTime.Now.Subtract(testStartTime).Seconds, testResult.Exception.Flatten());
                            }

                            if (testResult.IsCanceled || testResult.IsFaulted)
                            {
                                // For each failed test, do:
                                //    check if test is in the cache of notified tests.
                                //    if it is, check when it was the last time the test failed and the notification was generated.
                                //       if less than silencePerDuplicateAlert, do nothing and continue
                                //    otherwise, cache this method as a failed test, and mark that we should send an e-mail.
                                DateTime lastFailedTimeWithNotification;
                                if (CacheLastFailedTimeWithNotification.TryGetValue(methodInfo.Name, out lastFailedTimeWithNotification))
                                {
                                    if (testStartTime.Subtract(lastFailedTimeWithNotification).Hours >= silencePerDuplicateAlert)
                                    {
                                        hasTestFailed = true;
                                        CacheLastFailedTimeWithNotification[methodInfo.Name] = testStartTime;
                                    }
                                }
                                else
                                {
                                    hasTestFailed = true;
                                    CacheLastFailedTimeWithNotification[methodInfo.Name] = testStartTime;
                                }

                                var testException = testResult.IsCanceled ? new Exception(string.Format(methodInfo.Name + " has not finished within {0} secs. Moved on.", secsPerTest)) : testResult.Exception.Flatten();
                                testResultMessages.Add(TestUtilities.Exception2HtmlEmail(methodInfo.Name, testException));
                            }
                            else
                            {
                                // Test passed
                                Console.WriteLine(methodInfo.Name + " finished within {0} secs.", DateTime.Now.Subtract(testStartTime).Seconds);
                            }
                        });

                    if (runConcurrently)
                    {
                        testResultTasks.Add(testResultTask);
                    }
                    else
                    {
                        testResultTask.Wait();
                    }
                }

                if (runConcurrently)
                {
                    // Wait for all test result tasks to finish
                    Task.Run(async () => await Task.WhenAll(testResultTasks.ToArray())).Wait();
                }

                stopWatch.Stop();
                Console.WriteLine("Run #{0} completed in {1} seconds", i, stopWatch.Elapsed.TotalSeconds);

                // If any tests failed during this run, check whether we should send e-mail about them
                FormatResultAndSendEmail(i, hasTestFailed, testResultMessages);

                // Wait between tests. No need to wait if this is the last run
                if (i + 1 < numberOfRuns)
                {
                    Console.WriteLine("Started to wait for {0} seconds at {1}", secsPerDelay, DateTime.Now.ToString("HH:mm:ss"));
                    Thread.Sleep(secsPerDelay * 1000);
                }
            }
        }

        /// <summary>
        /// Method to format test result messages and send them in an email
        /// Note that this is done only for failed tests
        /// </summary>
        /// <param name="runId">run id</param>
        /// <param name="hasTestFailed">flag to check of any test has failed</param>
        /// <param name="testResultMessages">test result messages for failed tests</param>
        private static void FormatResultAndSendEmail(int runId, bool hasTestFailed, IEnumerable<string> testResultMessages)
        {
            // If any tests failed during this run, check whether we should send e-mail about them
            if (hasTestFailed && sendEmailOnTestFailure)
            {
                Email msg = new Email();
                try
                {
                    msg.Subject = string.Format("Failed tests during run#:{0} out of {1}", runId, numberOfRuns);
                    msg.To = new List<string>() { TestConstants.FailedTestsEmail };
                    msg.Category = "EndToEnd Testing";
                    msg.HtmlBody = string.Join("<br><br>", testResultMessages);
                    string configFilePath = ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName;
                    string sendGridKey = TestUtilities.GetSendGridKey(configFilePath);
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
            Console.WriteLine("-n=XXX\t Number of loops. Default = {0} (run continously)", numberOfRuns);
            Console.WriteLine("-t=XXX\t How long to wait for completion of invidual test. Default = {0} (secs)", secsPerTest);
            Console.WriteLine("-d=XXX\t How long to wait after one full test run. Default = {0} (secs)", secsPerDelay);
            Console.WriteLine("-a=XXX\t How long to silence duplicate alerts for. Default = {0} hours", silencePerDuplicateAlert);
            Console.WriteLine("-1=<testname>\t Run a single test rather than the full list of PassingTests.");
            Console.WriteLine("-s\t Silent mode - doesn't send any emails when a test fails.");
            Console.WriteLine("-concurrent\t Run all the tests concurrently.");
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
                    if (words[0] == "-n")
                    {
                        numberOfRuns = int.Parse(words[1]);
                    }
                    else if (words[0] == "-t")
                    {
                        secsPerTest = int.Parse(words[1]);
                    }
                    else if (words[0] == "-d")
                    {
                        secsPerDelay = int.Parse(words[1]);
                    }
                    else if (words[0] == "-a")
                    {
                        silencePerDuplicateAlert = int.Parse(words[1]);
                    }
                    else if (words[0] == "-s")
                    {
                        sendEmailOnTestFailure = false;
                    }
                    else if (words[0] == "-concurrent")
                    {
                        runConcurrently = true;
                    }
                    else if (words[0] == "-1")
                    {
                        singleTestName = words[1];
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
