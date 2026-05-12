using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using VBSPOSS.Data;
using VBSPOSS.Data.OSS.Models;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Models;
using VBSPOSS.Services.Interfaces;
using VBSPOSS.ViewModels;

namespace VBSPOSS.Services.Implements
{
    public class ScriptExecutionService
        : IScriptExecutionService
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;

        private readonly ILogger<ScriptExecutionService> _logger;

        private readonly IConnectionStringProvider _connectionStringProvider;

        public ScriptExecutionService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ScriptExecutionService> logger,
            IConnectionStringProvider connectionStringProvider)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _connectionStringProvider = connectionStringProvider;

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

            if (queue.Status == 1)
                throw new Exception(
                    "Script đang được thực thi.");

            queue.Status = 1;

            queue.ExecuteTime = DateTime.Now;

            queue.ErrorMessage = null;

            await _context.SaveChangesAsync();

            var oracleConnectionString =
                _connectionStringProvider
                    .GetOracleConnectionString();

            await using var conn =
                new OracleConnection(
                    oracleConnectionString);

            await conn.OpenAsync();

            await using var tran =
                conn.BeginTransaction();

            try
            {
                var rawScript =
                    queue.ScriptContent ?? "";

                // Normalize script
                var script =
                    NormalizeOracleScript(
                        rawScript,
                        queue.ExecuteMode);

                await using var command =
                    conn.CreateCommand();

                command.Transaction = tran;

                command.BindByName = true;

                command.CommandTimeout = 300;

                command.CommandType =
                    System.Data.CommandType.Text;

                command.CommandText = script;

                var affectedRows =
                    await command.ExecuteNonQueryAsync();

                await tran.CommitAsync();

                queue.Status = 2;

                queue.ExecutedBy = executedBy;

                queue.ExecutedDate = DateTime.Now;

                queue.ErrorMessage = null;

                // Insert log
                _context.ScriptExecutionLogs.Add(
                    new ScriptExecutionLog
                    {
                        QueueId = queue.Id,
                        StartTime = queue.ExecuteTime ?? DateTime.Now,
                        EndTime = DateTime.Now,
                        ExecutionStatus = 0,
                        AffectedRows = affectedRows,
                        OracleMessage = "SUCCESS",
                        ExecutionLogContent = script,
                        CreatedDate = DateTime.Now
                    });
            }
            catch (Exception ex)
            {
                try
                {
                    await tran.RollbackAsync();
                }
                catch
                {

                }

                queue.Status = 3;

                queue.RetryCount += 1;

                queue.ErrorMessage = ex.ToString();

                _context.ScriptExecutionLogs.Add(
                    new ScriptExecutionLog
                    {
                        QueueId = queue.Id,
                        StartTime = queue.ExecuteTime ?? DateTime.Now,
                        EndTime = DateTime.Now,
                        ExecutionStatus = 1,
                        ErrorCode = ex.HResult.ToString(),
                        ErrorMessage = ex.Message,
                        ExecutionLogContent = ex.ToString(),
                        CreatedDate = DateTime.Now
                    });

                _logger.LogError(
                    ex,
                    "Execute Oracle Script Error. QueueId={QueueId}",
                    queue.Id);

                throw;
            }
            finally
            {
                await _context.SaveChangesAsync();
            }
        }

        private string NormalizeOracleScript(
    string script,
    int executeMode)
        {
            if (string.IsNullOrWhiteSpace(script))
                return "";

            // Remove BOM + xuống dòng dư
            script = script
                .Replace("\r", "")
                .Trim();

            // Remove dấu ; cuối
            while (script.EndsWith(";"))
            {
                script =
                    script.Substring(
                        0,
                        script.Length - 1).Trim();
            }

            // SQL MODE
            // 0 = SQL
            if (executeMode == 0)
            {
                return script;
            }

            // PROCEDURE MODE
            // 1 = PROCEDURE
            if (executeMode == 1)
            {
                // Ví dụ:
                // PKG_TEST.RUN_PROC

                if (!script.StartsWith("BEGIN",
                    StringComparison.OrdinalIgnoreCase))
                {
                    script =
                        $"BEGIN {script}; END;";
                }

                return script;
            }

            // PACKAGE / PLSQL MODE
            // 2 = PACKAGE
            if (executeMode == 2)
            {
                var upper =
                    script.ToUpper();

                // Nếu chưa có BEGIN END
                if (!upper.StartsWith("BEGIN"))
                {
                    script =
                        $"BEGIN {script}; END;";
                }

                return script;
            }

            return script;
        }

        //private async Task ExecuteScript(long id,string executedBy)
        //{
        //    var queue =
        //        await _context.ScriptExecutionQueues
        //            .FirstOrDefaultAsync(x => x.Id == id);

        //    if (queue == null)
        //        return;

        //    queue.Status = 1;
        //    await _context.SaveChangesAsync();

        //    var oracleConnectionString =_connectionStringProvider.GetOracleConnectionString();
        //    using var conn =new OracleConnection(oracleConnectionString);

        //    await conn.OpenAsync();

        //    using var tran =conn.BeginTransaction();

        //    try
        //    {
        //        var command =
        //            conn.CreateCommand();

        //        command.Transaction = tran;

        //        command.CommandText =
        //            queue.ScriptContent;

        //        await command.ExecuteNonQueryAsync();

        //        tran.Commit();

        //        queue.Status = 2;

        //        queue.ExecutedBy = executedBy;

        //        queue.ExecutedDate = DateTime.Now;
        //    }
        //    catch (Exception ex)
        //    {
        //        tran.Rollback();

        //        queue.Status = 3;

        //        queue.ErrorMessage = ex.ToString();

        //        throw;
        //    }

        //    await _context.SaveChangesAsync();
        //}

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
                throw ;
            }
        }
    }
}
