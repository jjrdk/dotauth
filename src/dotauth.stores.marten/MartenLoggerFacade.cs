namespace DotAuth.Stores.Marten;

using System;
using System.Linq;
using global::Marten;
using global::Marten.Services;
using Microsoft.Extensions.Logging;
using Npgsql;

/// <summary>
/// Defines the logger facade for marten.
/// </summary>
public sealed partial class MartenLoggerFacade : IMartenLogger, IMartenSessionLogger
{
    private readonly ILogger<MartenLoggerFacade> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenLoggerFacade"/> class.
    /// </summary>
    /// <param name="logger"></param>
    public MartenLoggerFacade(ILogger<MartenLoggerFacade> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IMartenSessionLogger StartSession(IQuerySession session)
    {
        return this;
    }

    /// <inheritdoc />
    public void SchemaChange(string sql)
    {
        LogExecutingDdlChangeSql(sql);
    }

    /// <inheritdoc />
    public void LogSuccess(NpgsqlCommand command)
    {
        var entry = command.Parameters.Count == 0
            ? command.CommandText
            : command.Parameters.Aggregate(
                command.CommandText,
                (current, npgsqlParameter) =>
                {
                    var usedName = npgsqlParameter.ParameterName == ""
                        ? $"${command.Parameters.IndexOf(npgsqlParameter) + 1}"
                        : npgsqlParameter.ParameterName;
                    return current.Replace(usedName, $"({usedName} -> {npgsqlParameter.Value})");
                });
        LogEntry(entry);
    }

    /// <inheritdoc />
    public void LogFailure(NpgsqlCommand command, Exception ex)
    {
        LogPostgresqlCommandFailed();
        var failureEntry = command.Parameters.Aggregate(
            command.CommandText,
            (current, npgsqlParameter) => current.Replace(
                npgsqlParameter.ParameterName,
                $"  {npgsqlParameter.ParameterName} -> {npgsqlParameter.Value}"));
        LogFailureentry(failureEntry, ex);
    }

    /// <inheritdoc />
    public void LogSuccess(NpgsqlBatch batch)
    {
        var entry = batch.BatchCommands.OfType<NpgsqlBatchCommand>().Aggregate("", (s, command) =>
            s + Environment.NewLine + command.Parameters.Where(p => !string.IsNullOrEmpty(p.ParameterName)).Aggregate(
                command.CommandText,
                (current, npgsqlParameter) => current.Replace(
                    npgsqlParameter.ParameterName,
                    $"  {npgsqlParameter.ParameterName} -> {npgsqlParameter.Value}")));
        LogBatchentry(entry);
    }

    /// <inheritdoc />
    public void LogFailure(NpgsqlBatch batch, Exception ex)
    {
        var entry = batch.BatchCommands.OfType<NpgsqlBatchCommand>().Aggregate("", (s, command) =>
            s + Environment.NewLine + command.Parameters.Aggregate(
                command.CommandText,
                (current, npgsqlParameter) => current.Replace(
                    npgsqlParameter.ParameterName,
                    $"  {npgsqlParameter.ParameterName} -> {npgsqlParameter.Value}")));
        LogBatcherror(entry, ex);
    }

    /// <inheritdoc />
    public void LogFailure(Exception ex, string message)
    {
        LogError(message, ex);
    }

    /// <inheritdoc />
    public void RecordSavedChanges(IDocumentSession session, IChangeSet commit)
    {
        LogPersistedUpdateamountUpdatesInsertamountInsertsAndDeleteamountDeletions(commit.Updated.Count(), commit.Inserted.Count(), commit.Deleted.Count());
    }

    /// <inheritdoc />
    public void OnBeforeExecute(NpgsqlCommand command)
    {
        LogBeforePostgresqlCommandCommandtext(command.CommandText);
    }

    /// <inheritdoc />
    public void OnBeforeExecute(NpgsqlBatch batch)
    {
    }

    [LoggerMessage(LogLevel.Information, "Executing DDL change: {Sql}")]
    partial void LogExecutingDdlChangeSql(string sql);

    [LoggerMessage(LogLevel.Information, "{Entry}")]
    partial void LogEntry(string entry);

    [LoggerMessage(LogLevel.Error, "{FailureEntry}")]
    partial void LogFailureentry(string failureEntry, Exception exception);

    [LoggerMessage(LogLevel.Error, "PostgreSql command failed!")]
    partial void LogPostgresqlCommandFailed();

    [LoggerMessage(LogLevel.Information, "{BatchEntry}")]
    partial void LogBatchentry(string batchEntry);

    [LoggerMessage(LogLevel.Error, "{BatchError}")]
    partial void LogBatcherror(string batchError, Exception exception);

    [LoggerMessage(LogLevel.Error, "{Error}")]
    partial void LogError(string error, Exception exception);

    [LoggerMessage(LogLevel.Information, "Persisted {UpdateAmount} updates, {InsertAmount} inserts, and {DeleteAmount} deletions")]
    partial void LogPersistedUpdateamountUpdatesInsertamountInsertsAndDeleteamountDeletions(int updateAmount, int insertAmount, int deleteAmount);

    [LoggerMessage(LogLevel.Error, "Before PostgreSql command: {CommandText}")]
    partial void LogBeforePostgresqlCommandCommandtext(string commandText);
}
