using Hangfire;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Data;
using VBSPOSS.Services.Interfaces;

namespace VBSPOSS.Services.Implements
{
    public class ScriptExecutionJobService
        : IScriptExecutionJobService
    {
        private readonly ApplicationDbContext _context;

        private readonly IScriptExecutionService
            _scriptExecutionService;

        private readonly ILogger<ScriptExecutionJobService>
            _logger;

        public ScriptExecutionJobService(
            ApplicationDbContext context,
            IScriptExecutionService scriptExecutionService,
            ILogger<ScriptExecutionJobService> logger)
        {
            _context = context;

            _scriptExecutionService =
                scriptExecutionService;

            _logger = logger;
        }


        [DisableConcurrentExecution(300)]

        public async Task ProcessWaitingScripts()
        {
            try
            {
                var now = DateTime.Now;

                var scripts =
                    await _context
                        .ScriptExecutionQueues
                        .Where(x =>
                            x.Status == 0
                            && x.ExecuteType == 1
                            && x.ScheduleTime != null
                            && x.ScheduleTime <= now)
                        .OrderByDescending(x =>
                            x.PriorityLevel)
                        .ThenBy(x =>
                            x.ScheduleTime)
                        .ToListAsync();

                foreach (var item in scripts)
                {
                    try
                    {
                        _logger.LogInformation(
                            "Auto execute script QueueId={QueueId}",
                            item.Id);

                        await _scriptExecutionService
                            .ExecuteScripts(
                                new List<long>
                                {
                                    item.Id
                                },
                                "AUTO_JOB");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Auto execute error QueueId={QueueId}",
                            item.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ProcessWaitingScripts Error");
            }
        }
    }
}
