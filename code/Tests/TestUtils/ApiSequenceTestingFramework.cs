// <copyright file="ApiSequenceTestingFramework.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using System.Web.Http;

    /// <summary>
    /// This class takes a sequence (list) of controller actions and checks, and executes them in sequence.
    /// After the execution of each action, its accompanying check is also performed and its return value is returned.
    /// The caller is responsible for writing the check and deciding on its behavior upon encountering an error.
    /// For example, a check could use asserts; in this case, the Execute will terminate as soon as an assert fires.
    /// As another example, a check could use a logger to log its errors; in this case, the Execute will run its
    /// sequence end-to-end.
    /// </summary>
    public static class ApiSequenceTestingFramework
    {
        /// <summary>
        /// Executes a sequence of controller actions and checks their results. The actions are parameterless.
        /// </summary>
        /// <param name="apiSeq">sequence of controller actions and its accompanying action checks</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<List<IHttpActionResult>> Execute(List<Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>>> apiSeq)
        {
            if (apiSeq == null)
            {
                return null;
            }

            var resultList = new List<IHttpActionResult>();
            for (int i = 0; i < apiSeq.Count; i += 1)
            {
                var result = await ApiSequenceTestingFramework.Execute(apiSeq[i]);
                resultList.Add(result);
            }

            return resultList;
        }

        /// <summary>
        /// Executes a controller action and then checks its result. The action is parameterless.
        /// </summary>
        /// <param name="apiTuple">A controller action and its accompanying action check</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<IHttpActionResult> Execute(Tuple<Func<Task<IHttpActionResult>>, Action<IHttpActionResult>> apiTuple)
        {
            // If no tuple or controller action (item1) are present, return early
            if (apiTuple == null || apiTuple.Item1 == null)
            {
                return null;
            }

            var controllerAction = apiTuple.Item1;
            var actionCheck = apiTuple.Item2;

            // Perform the action
            var t = controllerAction.Invoke();
            var actionResult = await t;

            // Perform the check if present
            actionCheck?.Invoke(actionResult);

            return actionResult;
        }
    }
}
