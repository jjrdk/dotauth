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
public sealed class MartenLoggerFacade : IMartenLogger, IMartenSessionLogger
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
        _logger.LogInformation("Executing DDL change: {Sql}", sql);
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
        _logger.LogInformation("{Entry}", entry);
    }

    /// <inheritdoc />
    public void LogFailure(NpgsqlCommand command, Exception ex)
    {
        _logger.LogError("PostgreSql command failed!");
        var entry = command.Parameters.Aggregate(
            command.CommandText,
            (current, npgsqlParameter) => current.Replace(
                npgsqlParameter.ParameterName,
                $"  {npgsqlParameter.ParameterName} -> {npgsqlParameter.Value}"));
        _logger.LogError(ex, "{Entry}", entry);
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
        _logger.LogInformation("{BatchEntry}", entry);
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
        _logger.LogError(ex, "{BatchError}", entry);
    }

    /// <inheritdoc />
    public void LogFailure(Exception ex, string message)
    {
        _logger.LogError(ex, "{Error}", message);
    }

    /// <inheritdoc />
    public void RecordSavedChanges(IDocumentSession session, IChangeSet commit)
    {
        _logger.LogInformation(
            "Persisted {UpdateAmount} updates, {InsertAmount} inserts, and {DeleteAmount} deletions",
            commit.Updated.Count(),
            commit.Inserted.Count(),
            commit.Deleted.Count());
    }

    /// <inheritdoc />
    public void OnBeforeExecute(NpgsqlCommand command)
    {
        _logger.LogError("Before PostgreSql command: {CommandText}", command.CommandText);
    }

    /// <inheritdoc />
    public void OnBeforeExecute(NpgsqlBatch batch)
    {
    }
}
