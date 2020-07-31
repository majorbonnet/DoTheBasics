using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoTheBasics.Repo
{
    public class GoalDatabase : BaseDatabase
    {
        public async Task<List<Goal>> GetGoalsAsync()
        {
            var conn = await GetDatabaseConnection<Goal>().ConfigureAwait(false);

            return await AttemptAndRetry(() => conn.QueryAsync<Goal>("SELECT * FROM Goal WHERE IsActive = 1")).ConfigureAwait(false);
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
                GoalMinute = minute,
                LastCompletion = DateTime.MinValue,
                IsActive = true
            };

            var conn = await GetDatabaseConnection<Goal>().ConfigureAwait(false);

            await conn.InsertAsync(goal);

            return goal;
        }

        public async Task<Goal> UpdateGoal(Goal goal)
        {
            var conn = await GetDatabaseConnection<Goal>().ConfigureAwait(false);

            await conn.ExecuteAsync("UPDATE Goal SET Title = ?, Description = ?, GoalHour = ?, GoalMinute = ? WHERE Id = ?",
                goal.Title,
                goal.Description,
                goal.GoalHour,
                goal.GoalMinute,
                goal.Id);

            return goal;
        }

        public async Task DeactivateGoal(int goalId)
        {
            var conn = await GetDatabaseConnection<Goal>().ConfigureAwait(false);

            await conn.ExecuteAsync("UPDATE Goal SET IsActive = 0 WHERE Id = ?", goalId);
        }

        public async Task DeleteGoal(int goalId)
        {
            var conn = await GetDatabaseConnection<Goal, GoalCompletion, GoalStats>().ConfigureAwait(false);

            await conn.RunInTransactionAsync(tran =>
            {
                tran.Execute("DELETE FROM GoalStats WHERE GoalId = ?", goalId);
                tran.Execute("DELETE FROM GoalCompletion WHERE GoalId = ?", goalId);
                tran.Execute("DELETE FROM Goal WHERE Id = ?", goalId);
            });
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

        public async Task<Goal> UndoGoalCompletion(int goalId)
        {
            var conn = await GetDatabaseConnection<Goal, GoalCompletion>().ConfigureAwait(false);

            var last2Completions = await AttemptAndRetry(() => 
                    conn.QueryAsync<GoalCompletion>(@"SELECT GoalId, CompletionTime 
FROM GoalCompletion 
WHERE GoalId = ? 
ORDER BY CompletionTime DESC
LIMIT 2", goalId)
                 )
                .ConfigureAwait(false);

            if (last2Completions.Count > 1)
            {
                var penultimateCompletion = last2Completions.OrderBy(c => c.CompletionTime).First();
                var lastCompletion = last2Completions.OrderByDescending(c => c.CompletionTime).First();

                await conn.RunInTransactionAsync(tran =>
                {
                    tran.Execute("UPDATE Goal SET LastCompletion = ? WHERE Id = ?", penultimateCompletion.CompletionTime, goalId);
                    tran.Execute("DELETE FROM GoalCompletion WHERE GoalId = ? AND CompletionTime = ?");
                });
            }

            if (last2Completions.Count == 1)
            {
                await conn.RunInTransactionAsync(tran =>
                {
                    tran.Execute("UPDATE Goal SET LastCompletion = ? WHERE Id = ?", DateTime.MinValue, goalId);
                    tran.Execute("DELETE FROM GoalCompletion WHERE GoalId = ?");
                });
            }

            var updatedGoal = await AttemptAndRetry(() => conn.FindAsync<Goal>(goalId)).ConfigureAwait(false);

            return updatedGoal;
        }
    }
}
