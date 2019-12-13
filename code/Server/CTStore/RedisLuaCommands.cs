//-----------------------------------------------------------------------
// <copyright file="RedisLuaCommands.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements class RedisLuaCommands.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.CTStore
{
    /// <summary>
    /// <c>Redis Lua</c> commands
    /// </summary>
    public class RedisLuaCommands
    {
        /// <summary>
        /// The script returns a result array. The first element of the array is the success code.
        /// Success code 1 means successful execution and the following elements in the array
        /// are results of individual transactions.
        /// Success code less than 0 means pre condition failed failure. The negative number
        /// indicates the operation that violated the pre condition in a transaction.
        /// For example, -1 refers to the first operation, and -2 refers to the second operation.
        /// The script is constructed with all conditions executing before the operations
        /// since <c>Redis</c> does not have rollbacks. If all conditions are satisfied,
        /// we are sure that the operations will succeed.
        /// </summary>
        public const string ResultArray = "local result={1}";

        /// <summary>
        /// Object Insert condition. Return error if object already exists.
        /// </summary>
        public const string ObjectInsertCondition = "if redis.call('exists', KEYS[{0}]) == 1 then return {{{1}, 0}} end";

        /// <summary>
        /// Object ETag condition. Return error if the ETag doesn't match. ETag is used for replace, merge,
        /// and delete operations.
        /// </summary>
        public const string ObjectETagCondition = "if redis.call('hget', KEYS[{0}], 'ETag') ~= ARGV[{1}] then if redis.call('exists', KEYS[{0}]) == 0 then return {{{2}, 0}} else return {{{2}, 1}} end end";

        /// <summary>
        /// Object Delete condition. Return error if object does not exist.
        /// </summary>
        public const string ObjectDeleteCondition = "if redis.call('exists', KEYS[{0}]) == 0 then return {{{1}, 0}} end";

        /// <summary>
        /// Object wildcard replace condition. Same as object delete condition.
        /// </summary>
        public const string ObjectWilcardReplaceCondition = ObjectDeleteCondition;

        /// <summary>
        /// Object wildcard merge condition. Same as object delete condition.
        /// </summary>
        public const string ObjectWilcardMergeCondition = ObjectDeleteCondition;

        /// <summary>
        /// Object Insert action.
        /// </summary>
        public const string ObjectInsertAction = "local values = {{}} local j = 1 for i={2}, {3} do values[j] = ARGV[i] j = j + 1 end result[{0}] = redis.call('hmset', KEYS[{1}], unpack(values))";

        /// <summary>
        /// Object Delete action.
        /// </summary>
        public const string ObjectDeleteAction = "result[{0}] = redis.call('del', KEYS[{1}])";

        /// <summary>
        /// Object Insert or Replace action.
        /// </summary>
        public const string ObjectInsertOrReplaceAction = "redis.call('del', KEYS[{1}]) " + ObjectInsertAction;

        /// <summary>
        /// Object Replace action. It is same as insert or replace action, but replace
        /// operation would have a precondition added.
        /// </summary>
        public const string ObjectReplaceAction = ObjectInsertOrReplaceAction;

        /// <summary>
        /// Object Merge action. It is same as insert action.
        /// </summary>
        public const string ObjectMergeAction = ObjectInsertAction;

        /// <summary>
        /// Object Insert or Merge action. It is same as merge action, but this action will not have a precondition.
        /// </summary>
        public const string ObjectInsertOrMergeAction = ObjectMergeAction;

        /// <summary>
        /// Fixed object Insert condition. Return error if object already exists.
        /// </summary>
        public const string FixedObjectInsertCondition = "if redis.call('exists', KEYS[{0}]) == 1 then return {{{1}, 0}} end";

        /// <summary>
        /// Fixed object ETag condition. Return error if the ETag doesn't match. ETag is used for replace
        /// and delete operations. For cache-only fixed objects, ETag is the entire entity.
        /// </summary>
        public const string FixedObjectETagCondition = "if redis.call('get', KEYS[{0}]) ~= ARGV[{1}] then if redis.call('exists', KEYS[{0}]) == 0 then return {{{2}, 0}} else return {{{2}, 1}} end end";

        /// <summary>
        /// Fixed object Delete condition. Return error if object does not exist.
        /// </summary>
        public const string FixedObjectDeleteCondition = "if redis.call('exists', KEYS[{0}]) == 0 then return {{{1}, 0}} end";

        /// <summary>
        /// Fixed object wildcard replace condition. Same as fixed object delete condition.
        /// </summary>
        public const string FixedObjectWilcardReplaceCondition = FixedObjectDeleteCondition;

        /// <summary>
        /// Fixed object Insert action.
        /// </summary>
        public const string FixedObjectInsertAction = "result[{0}] = redis.call('set', KEYS[{1}], ARGV[{2}])";

        /// <summary>
        /// Fixed object Delete action.
        /// </summary>
        public const string FixedObjectDeleteAction = "result[{0}] = redis.call('del', KEYS[{1}])";

        /// <summary>
        /// Fixed object Insert or Replace action. It is same as insert, but insert
        /// operation would have a precondition added.
        /// </summary>
        public const string FixedObjectInsertOrReplaceAction = FixedObjectInsertAction;

        /// <summary>
        /// Fixed object Replace action. It is same as insert, but replace
        /// operation would have a precondition added.
        /// </summary>
        public const string FixedObjectReplaceAction = FixedObjectInsertAction;

        /// <summary>
        /// Feed insert condition
        /// </summary>
        public const string FeedInsertCondition = "local v = redis.call('zrangebylex', KEYS[{0}], ARGV[{1}], ARGV[{2}])[1] if v ~= nil then return {{{3}, 0}} end";

        /// <summary>
        /// Feed ETag condition
        /// </summary>
        public const string FeedETagCondition = "local v = redis.call('zrangebylex', KEYS[{0}], ARGV[{1}], ARGV[{2}])[1] if v == nil then return {{{4}, 0}} else if v ~= ARGV[{3}] then return {{{4}, 1}} end end";

        /// <summary>
        /// Feed delete condition
        /// </summary>
        public const string FeedDeleteCondition = "local v = redis.call('zrangebylex', KEYS[{0}], ARGV[{1}], ARGV[{2}])[1] if v == nil then return {{{3}, 0}} end";

        /// <summary>
        /// Feed wildcard replace condition. Same as feed delete condition.
        /// </summary>
        public const string FeedWildcardReplaceCondition = FeedDeleteCondition;

        /// <summary>
        /// Feed insert action
        /// </summary>
        public const string FeedInsertAction = "result[{0}] = redis.call('zadd', KEYS[{1}], 1, ARGV[{2}])";

        /// <summary>
        /// Feed delete action
        /// </summary>
        public const string FeedDeleteAction = "result[{0}] = redis.call('zremrangebylex', KEYS[{1}], ARGV[{2}], ARGV[{3}])";

        /// <summary>
        /// Feed insert or replace action
        /// </summary>
        public const string FeedInsertOrReplaceAction = "redis.call('zremrangebylex', KEYS[{1}], ARGV[{2}], ARGV[{3}]) result[{0}] = redis.call('zadd', KEYS[{1}], 1, ARGV[{4}])";

        /// <summary>
        /// Feed replace action
        /// </summary>
        public const string FeedReplaceAction = FeedInsertOrReplaceAction;

        /// <summary>
        /// Feed insert or replace if not last action
        /// </summary>
        public const string FeedInsertOrReplaceIfNotLastAction = FeedInsertOrReplaceAction + " if (redis.call('zrank', KEYS[{1}], ARGV[{4}]) == (redis.call('zcard', KEYS[{1}]) - 1)) then redis.call('zremrangebylex', KEYS[{1}], ARGV[{2}], ARGV[{3}]) result[{0}] = 'NOK' end";

        /// <summary>
        /// Feed insert if not empty action
        /// </summary>
        public const string FeedInsertIfNotEmptyAction = "if redis.call('exists', KEYS[{1}]) == 0 then result[{0}] = 'NOK' else " + FeedInsertAction + " end";

        /// <summary>
        /// Feed ascending trim action
        /// </summary>
        public const string FeedTrimAction = "redis.call('zremrangebyrank', KEYS[{0}], ARGV[{1}], -1)";

        /// <summary>
        /// Feed item query
        /// </summary>
        public const string FeedItemQuery = "return redis.call('zrangebylex', KEYS[1], ARGV[1], ARGV[2])";

        /// <summary>
        /// Feed query
        /// </summary>
        public const string FeedQuery = "return redis.call('zrangebylex', KEYS[1], ARGV[1], '+', 'LIMIT', 0, ARGV[2])";

        /// <summary>
        /// Count Insert condition. Same as object insert condition.
        /// </summary>
        public const string CountInsertCondition = ObjectInsertCondition;

        /// <summary>
        /// Count ETag condition. Same as object ETag condition.
        /// </summary>
        public const string CountETagCondition = ObjectETagCondition;

        /// <summary>
        /// Count Delete condition. Same as object delete condition.
        /// </summary>
        public const string CountDeleteCondition = ObjectDeleteCondition;

        /// <summary>
        /// Count Increment condition. Same as count delete condition.
        /// </summary>
        public const string CountIncrementCondition = CountDeleteCondition;

        /// <summary>
        /// Count wildcard replace condition. Same as count delete condition.
        /// </summary>
        public const string CountWildcardReplaceCondition = CountDeleteCondition;

        /// <summary>
        /// Count Insert action. Same as object insert action.
        /// </summary>
        public const string CountInsertAction = ObjectInsertAction;

        /// <summary>
        /// Count Delete action. Same as object delete action.
        /// </summary>
        public const string CountDeleteAction = ObjectDeleteAction;

        /// <summary>
        /// Count Insert Or Replace action. Same as object insert or replace action.
        /// </summary>
        public const string CountInsertOrReplaceAction = ObjectInsertOrReplaceAction;

        /// <summary>
        /// Count Replace action. Same object replace action.
        /// </summary>
        public const string CountReplaceAction = ObjectReplaceAction;

        /// <summary>
        /// Count Increment action.
        /// </summary>
        public const string CountIncrementAction = "result[{0}] = redis.call('hincrbyfloat', KEYS[{1}], ARGV[{2}], ARGV[{3}]) result[{0}] = result[{0}] .. ',' .. redis.call('hget', KEYS[{1}], 'ETag')";

        /// <summary>
        /// Count Insert or Increment action.
        /// </summary>
        public const string CountInsertOrIncrementAction = "if redis.call('exists', KEYS[{1}]) == 0 then " + CountInsertAction + " result[{0}] = ARGV[{5}] else result[{0}] = redis.call('hincrbyfloat', KEYS[{1}], ARGV[{4}], ARGV[{5}]) end result[{0}] = result[{0}] .. ',' .. redis.call('hget', KEYS[{1}], 'ETag')";

        /// <summary>
        /// Rank feed Insert condition.
        /// </summary>
        public const string RankFeedInsertCondition = "if redis.call('zrank', KEYS[{0}], ARGV[{1}]) ~= false then return {{{2}, 0}} end";

        /// <summary>
        /// Rank feed Delete condition.
        /// </summary>
        public const string RankFeedDeleteCondition = "if redis.call('zrank', KEYS[{0}], ARGV[{1}]) == false then return {{{2}, 0}} end";

        /// <summary>
        /// Rank feed Increment condition. Same as rank feed delete condition.
        /// </summary>
        public const string RankFeedIncrementCondition = RankFeedDeleteCondition;

        /// <summary>
        /// Rank feed Insert action.
        /// </summary>
        public const string RankFeedInsertAction = "result[{0}] = redis.call('zadd', KEYS[{1}], ARGV[{3}], ARGV[{2}])";

        /// <summary>
        /// Rank feed Delete action.
        /// </summary>
        public const string RankFeedDeleteAction = "result[{0}] = redis.call('zrem', KEYS[{1}], ARGV[{2}])";

        /// <summary>
        /// Rank feed Insert or Replace action. Same as rank feed insert action.
        /// </summary>
        public const string RankFeedInsertOrReplaceAction = RankFeedInsertAction;

        /// <summary>
        /// Rank feed Increment action.
        /// </summary>
        public const string RankFeedIncrementAction = "result[{0}] = redis.call('zincrby', KEYS[{1}], ARGV[{3}], ARGV[{2}])";

        /// <summary>
        /// Rank feed Insert or Increment action. Same as rank feed Increment action.
        /// </summary>
        public const string RankFeedInsertOrIncrementAction = RankFeedIncrementAction;

        /// <summary>
        /// Rank feed ascending trim action
        /// </summary>
        public const string RankFeedAscendingTrimAction = "redis.call('zremrangebyrank', KEYS[{0}], ARGV[{1}], -1)";

        /// <summary>
        /// Rank feed descending trim action
        /// </summary>
        public const string RankFeedDescendingTrimAction = "redis.call('zremrangebyrank', KEYS[{0}], 0, -ARGV[{1}]-1)";

        /// <summary>
        /// Return result array when success.
        /// </summary>
        public const string LuaReturnSuccess = "return result";
    }
}
