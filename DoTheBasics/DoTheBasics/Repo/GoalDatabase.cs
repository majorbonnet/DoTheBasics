using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoTheBasics.Repo
{
    public class GoalDatabase : BaseDatabase
    {
        public override async Task EnsureTablesAreCreatedAsync()
        {
            await DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
            await DatabaseConnection.CreateTablesAsync(
                CreateFlags.None, 
                typeof(Goal),
                typeof(GoalCompletion),
                typeof(GoalStats)).ConfigureAwait(false);
        }

        public async Task<List<Goal>> GetGoalsAsync()
        {
            return await AttemptAndRetry(() => DatabaseConnection.QueryAsync<Goal>("SELECT * FROM Goal WHERE IsActive = 1")).ConfigureAwait(false);
        }

        public async Task<Goal> GetGoalAsync(int goalId)
        {
            return await AttemptAndRetry(() => DatabaseConnection.FindAsync<Goal>(goalId)).ConfigureAwait(false);
        }

        public async Task<List<GoalCompletion>> GetGoalCompletionsAsync(int goalId)
        {
            return await AttemptAndRetry(() => DatabaseConnection.Table<GoalCompletion>().ToListAsync()).ConfigureAwait(false);
        }

        public async Task<GoalStats> GetGoalStats(int goalId)
        {
            return await AttemptAndRetry(() => DatabaseConnection.FindWithQueryAsync<GoalStats>("SELECT * FROM GoalStats WHERE GoalId = ?", goalId)).ConfigureAwait(false);
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

            await AttemptAndRetry(() => DatabaseConnection.InsertAsync(goal)).ConfigureAwait(false);

            return goal;
        }

        public async Task<Goal> UpdateGoal(Goal goal)
        {
            await AttemptAndRetry(() => DatabaseConnection.ExecuteAsync(
                "UPDATE Goal SET Title = ?, Description = ?, GoalHour = ?, GoalMinute = ? WHERE Id = ?",
                goal.Title,
                goal.Description,
                goal.GoalHour,
                goal.GoalMinute,
                goal.Id)).ConfigureAwait(false);

            return goal;
        }

        public async Task DeactivateGoal(int goalId)
        {
            await AttemptAndRetry(() => DatabaseConnection.ExecuteAsync(
                "UPDATE Goal SET IsActive = 0 WHERE Id = ?", 
                goalId)).ConfigureAwait(false);
        }

        public async Task DeleteGoal(int goalId)
        {
            await AttemptAndRetry(() => DatabaseConnection.RunInTransactionAsync(tran =>
            {
                tran.Execute("DELETE FROM GoalStats WHERE GoalId = ?", goalId);
                tran.Execute("DELETE FROM GoalCompletion WHERE GoalId = ?", goalId);
                tran.Execute("DELETE FROM Goal WHERE Id = ?", goalId);
            })).ConfigureAwait(false);
        }

        public async Task<Goal> AddGoalCompletion(Goal goal, DateTime completionTime)
        {
            var goalCompletion = new GoalCompletion { GoalId = goal.Id, CompletionTime = completionTime };

            await AttemptAndRetry(() => DatabaseConnection.RunInTransactionAsync((tran) =>
            {
                tran.Insert(goalCompletion);

                tran.Execute("UPDATE Goal SET LastCompletion = ? WHERE Id = ?", completionTime, goal.Id);

            })).ConfigureAwait(false);

            var updatedGoal = await AttemptAndRetry(() => DatabaseConnection.FindAsync<Goal>(goal.Id)).ConfigureAwait(false);

            return updatedGoal;
        }

        public async Task<Goal> UndoGoalCompletion(int goalId)
        {
            var last2Completions = await AttemptAndRetry(() =>
                    DatabaseConnection.QueryAsync<GoalCompletion>(@"SELECT GoalId, CompletionTime 
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

                await AttemptAndRetry(() => DatabaseConnection.RunInTransactionAsync(tran =>
                {
                    tran.Execute("UPDATE Goal SET LastCompletion = ? WHERE Id = ?", penultimateCompletion.CompletionTime, goalId);
                    tran.Execute("DELETE FROM GoalCompletion WHERE GoalId = ? AND CompletionTime = ?");
                })).ConfigureAwait(false);
            }

            if (last2Completions.Count == 1)
            {
                await AttemptAndRetry(() => DatabaseConnection.RunInTransactionAsync(tran =>
                {
                    tran.Execute("UPDATE Goal SET LastCompletion = ? WHERE Id = ?", DateTime.MinValue, goalId);
                    tran.Execute("DELETE FROM GoalCompletion WHERE GoalId = ?");
                })).ConfigureAwait(false);
            }

            var updatedGoal = await AttemptAndRetry(() => DatabaseConnection.FindAsync<Goal>(goalId)).ConfigureAwait(false);

            return updatedGoal;
        }
    }
}
