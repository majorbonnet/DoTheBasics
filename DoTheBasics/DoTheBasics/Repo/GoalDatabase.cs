using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DoTheBasics.Models;

namespace DoTheBasics.Repo
{
    public class GoalDatabase : BaseDatabase
    {
        public async Task<List<Goal>> GetGoalsAsync()
        {
            var conn = await GetDatabaseConnection<Goal>().ConfigureAwait(false);

            return await AttemptAndRetry(() => conn.Table<Goal>().ToListAsync()).ConfigureAwait(false);
        }

        public async Task<Goal> GetGoalAsync(int goalId)
        {
            var conn = await GetDatabaseConnection<Goal>().ConfigureAwait(false);

            return await AttemptAndRetry(() => conn.FindAsync<Goal>(goalId)).ConfigureAwait(false);
        }

        public async Task<List<GoalCompletion>> GetGoalCompletionsAsync(int goalId)
        {
            var conn = await GetDatabaseConnection<GoalCompletion>().ConfigureAwait(false);

            return await AttemptAndRetry(() => conn.Table<GoalCompletion>().ToListAsync()).ConfigureAwait(false);
        }

        public async Task<GoalStats> GetGoalStats(int goalId)
        {
            var conn = await GetDatabaseConnection<GoalStats>().ConfigureAwait(false);

            return await AttemptAndRetry(() => conn.FindWithQueryAsync<GoalStats>("SELECT * FROM GoalStats WHERE GoalId = ?", goalId)).ConfigureAwait(false);
        }

        public async Task<Goal> AddGoal(string title, string description, int hour, int minute)
        {
            var goal = new Goal
            {
                Title = title,
                Description = description,
                GoalHour = hour,
                GoalMinute = minute
            };

            var conn = await GetDatabaseConnection<Goal>().ConfigureAwait(false);

            await conn.InsertAsync(goal);

            return goal;
        }

        public async Task<Goal> AddGoalCompletion(Goal goal, DateTime completionTime)
        {
            var goalCompletion = new GoalCompletion { GoalId = goal.Id, CompletionTime = completionTime };
            var conn = await GetDatabaseConnection<Goal, GoalCompletion>().ConfigureAwait(false);

            await conn.RunInTransactionAsync((tran) =>
            {
                tran.Insert(goalCompletion);

                tran.Execute("UPDATE Goal SET LastCompletion = ? WHERE Id = ?", completionTime, goal.Id);

            });

            var updatedGoal = await conn.FindAsync<Goal>(goal.Id);

            return updatedGoal;
        }
    }
}
