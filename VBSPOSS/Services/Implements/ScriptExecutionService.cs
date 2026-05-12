using Oracle.ManagedDataAccess.Client;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace VBSPOSS.Services.Implements
{
    public class ScriptExecutionService
        : IScriptExecutionService
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;

        private readonly ILogger<ScriptExecutionService> _logger;

        public ScriptExecutionService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ScriptExecutionService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<ScriptExecutionViewModel>>
            LoadScriptQueue(
                string moduleCode,
                int? status,
                DateTime? fromDate,
                DateTime? toDate)
        {
            try
            {
                var query =
                _context.ScriptExecutionQueues
                    .AsQueryable();

            if (!string.IsNullOrEmpty(moduleCode))
            {
                query =
                    query.Where(x =>
                        x.ModuleCode == moduleCode);
            }

            if (status != null)
            {
                query =
                    query.Where(x =>
                        x.Status == status);
            }

            if (fromDate != null)
            {
                query =
                    query.Where(x =>
                        x.EffectiveDate >= fromDate);
            }

            if (toDate != null)
            {
                query =
                    query.Where(x =>
                        x.EffectiveDate <= toDate);
            }

            return await query
                .OrderByDescending(x => x.CreatedDate)
                .Select(x =>
                    new ScriptExecutionViewModel
                    {
                        Id = x.Id,
                        ModuleCode = x.ModuleCode,
                        ScriptName = x.ScriptName,
                        ExecuteType = x.ExecuteType,
                        Status = x.Status,
                        EffectiveDate = x.EffectiveDate,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                throw ex;
            }
        }

        public async Task<ServiceResult>
            ExecuteScripts(
                List<long> ids,
                string executedBy)
        {
            try
            {
                foreach (var id in ids)
                {
                    await ExecuteScript(
                        id,
                        executedBy);
                }

                return ServiceResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return ServiceResult.ErrorResult(ex.Message);
            }
        }

        private async Task ExecuteScript(
            long id,
            string executedBy)
        {
            var queue =
                await _context.ScriptExecutionQueues
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (queue == null)
                return;

            queue.Status = 1;

            await _context.SaveChangesAsync();

            using var conn =
                new OracleConnection(
                    _configuration
                        .GetConnectionString(
                            "CoreOracle"));

            await conn.OpenAsync();

            using var tran =
                conn.BeginTransaction();

            try
            {
                var command =
                    conn.CreateCommand();

                command.Transaction = tran;

                command.CommandText =
                    queue.ScriptContent;

                await command.ExecuteNonQueryAsync();

                tran.Commit();

                queue.Status = 2;

                queue.ExecutedBy = executedBy;

                queue.ExecutedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                tran.Rollback();

                queue.Status = 3;

                queue.ErrorMessage = ex.ToString();

                throw;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ServiceResult>
            RetryScript(
                long id,
                string executedBy)
        {
            await ExecuteScript(id, executedBy);

            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult>
            CancelScript(
                long id,
                string executedBy)
        {
            var item =
                await _context.ScriptExecutionQueues
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (item != null)
            {
                item.Status = 4;

                item.ModifiedBy = executedBy;

                item.ModifiedDate = DateTime.Now;

                await _context.SaveChangesAsync();
            }

            return ServiceResult.SuccessResult();
        }

        public async Task<List<ScriptExecutionLog>>
            GetExecutionLogs(long queueId)
        {
            return await _context.ScriptExecutionLogs
                .Where(x => x.QueueId == queueId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<ScriptExecutionQueue>
            GetScriptDetail(long id)
        {
            try
            {                
                return await _context.ScriptExecutionQueues
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw ex;
            }
        }
    }
}
