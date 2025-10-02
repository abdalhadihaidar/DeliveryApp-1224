using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace DeliveryApp.Application.Services
{
    /// <summary>
    /// Transaction management service for ensuring data consistency
    /// Handles complex multi-step operations with proper rollback capabilities
    /// </summary>
    public class TransactionManagementService : DomainService
    {
        private readonly ILogger<TransactionManagementService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public TransactionManagementService(ILogger<TransactionManagementService> logger, IUnitOfWorkManager unitOfWorkManager)
        {
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
        }

        /// <summary>
        /// Executes multiple operations within a single transaction
        /// If any operation fails, all changes are rolled back
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> operation,
            string operationName = "Unknown",
            CancellationToken cancellationToken = default)
        {
            using var uow = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            
            try
            {
                _logger.LogInformation("Starting transaction for operation: {OperationName}", operationName);
                
                var result = await operation();
                
                await uow.CompleteAsync(cancellationToken);
                
                _logger.LogInformation("Transaction completed successfully for operation: {OperationName}", operationName);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed for operation: {OperationName}. Rolling back changes.", operationName);
                
                // Rollback is automatic when using statement exits
                throw;
            }
        }

        /// <summary>
        /// Executes multiple operations within a single transaction (void return)
        /// </summary>
        public async Task ExecuteInTransactionAsync(
            Func<Task> operation,
            string operationName = "Unknown",
            CancellationToken cancellationToken = default)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await operation();
                return true; // Dummy return value
            }, operationName, cancellationToken);
        }

        /// <summary>
        /// Executes multiple operations with retry logic
        /// Useful for handling transient database errors
        /// </summary>
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            string operationName = "Unknown",
            int maxRetries = 3,
            TimeSpan delayBetweenRetries = default,
            CancellationToken cancellationToken = default)
        {
            if (delayBetweenRetries == default)
                delayBetweenRetries = TimeSpan.FromSeconds(1);

            var lastException = (Exception?)null;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await ExecuteInTransactionAsync(operation, $"{operationName} (Attempt {attempt})", cancellationToken);
                }
                catch (Exception ex) when (IsTransientError(ex) && attempt < maxRetries)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, 
                        "Transient error in operation: {OperationName}, attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms.", 
                        operationName, attempt, maxRetries, delayBetweenRetries.TotalMilliseconds);
                    
                    await Task.Delay(delayBetweenRetries, cancellationToken);
                }
            }

            _logger.LogError(lastException, 
                "Operation failed after {MaxRetries} attempts: {OperationName}", 
                maxRetries, operationName);
            
            throw lastException ?? new InvalidOperationException($"Operation failed: {operationName}");
        }

        /// <summary>
        /// Executes multiple operations in sequence within a single transaction
        /// If any operation fails, all previous operations are rolled back
        /// </summary>
        public async Task<T> ExecuteSequenceAsync<T>(
            IEnumerable<Func<Task>> operations,
            Func<Task<T>> finalOperation,
            string operationName = "Sequence",
            CancellationToken cancellationToken = default)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                // Execute all operations in sequence
                foreach (var operation in operations)
                {
                    await operation();
                }

                // Execute final operation and return result
                return await finalOperation();
            }, operationName, cancellationToken);
        }

        /// <summary>
        /// Executes operations in parallel within a single transaction
        /// All operations must succeed or all will be rolled back
        /// </summary>
        public async Task<T[]> ExecuteParallelAsync<T>(
            IEnumerable<Func<Task<T>>> operations,
            string operationName = "Parallel",
            CancellationToken cancellationToken = default)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var tasks = operations.Select(op => op()).ToArray();
                return await Task.WhenAll(tasks);
            }, operationName, cancellationToken);
        }

        /// <summary>
        /// Executes a compensation pattern - if the main operation fails,
        /// the compensation operation is executed to clean up
        /// </summary>
        public async Task<T> ExecuteWithCompensationAsync<T>(
            Func<Task<T>> mainOperation,
            Func<Task> compensationOperation,
            string operationName = "Compensation",
            CancellationToken cancellationToken = default)
        {
            T result;
            bool operationSucceeded = false;

            try
            {
                result = await ExecuteInTransactionAsync(mainOperation, operationName, cancellationToken);
                operationSucceeded = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Main operation failed: {OperationName}. Executing compensation.", operationName);
                
                try
                {
                    await compensationOperation();
                }
                catch (Exception compensationEx)
                {
                    _logger.LogError(compensationEx, "Compensation operation also failed: {OperationName}", operationName);
                    throw new InvalidOperationException(
                        $"Both main operation and compensation failed. Main: {ex.Message}, Compensation: {compensationEx.Message}", ex);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Validates that all operations can be executed before starting the transaction
        /// </summary>
        public async Task<T> ExecuteWithValidationAsync<T>(
            Func<Task<bool>> validationOperation,
            Func<Task<T>> mainOperation,
            string operationName = "Validation",
            CancellationToken cancellationToken = default)
        {
            // First validate without transaction
            var isValid = await validationOperation();
            if (!isValid)
            {
                throw new InvalidOperationException($"Validation failed for operation: {operationName}");
            }

            // Then execute main operation in transaction
            return await ExecuteInTransactionAsync(mainOperation, operationName, cancellationToken);
        }

        /// <summary>
        /// Determines if an exception is transient and worth retrying
        /// </summary>
        private static bool IsTransientError(Exception ex)
        {
            return ex switch
            {
                // Database connection issues
                DbUpdateConcurrencyException => true,
                DbUpdateException when ex.InnerException?.Message.Contains("timeout") == true => true,
                
                // Network issues
                HttpRequestException => true,
                TaskCanceledException when ex.InnerException is TimeoutException => true,
                
                // SQL Server specific transient errors
                _ when ex.Message.Contains("timeout") => true,
                _ when ex.Message.Contains("connection") => true,
                _ when ex.Message.Contains("deadlock") => true,
                
                _ => false
            };
        }
    }
}
