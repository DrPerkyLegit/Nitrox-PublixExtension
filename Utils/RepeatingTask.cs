using System;
using System.Threading.Tasks;

public class RepeatingTask
{
    public static async Task StartAsync(Func<Task<bool>> action, int delayMs, CancellationToken token = default)
    {
        while (!token.IsCancellationRequested)
        {
            bool shouldContinue = await action();
            if (!shouldContinue)
                break;

            await Task.Delay(delayMs, token);
        }
    }

    public static void RunInBackground(Func<Task<bool>> action, int delayMs, CancellationToken token = default)
    {
        _ = Task.Run(() => StartAsync(action, delayMs, token));
    }
}
