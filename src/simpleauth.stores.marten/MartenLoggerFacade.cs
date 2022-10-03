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
        _logger.LogInformation("Executing DDL change: {sql}", sql);
    }

    /// <inheritdoc />
    public void LogSuccess(NpgsqlCommand command)
    {
        var entry = command.Parameters.Aggregate(
            command.CommandText,
            (current, npgsqlParameter) => current.Replace(
                npgsqlParameter.ParameterName,
                $"  {npgsqlParameter.ParameterName} -> {npgsqlParameter.Value}"));
        _logger.LogInformation("{entry}", entry);
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
        _logger.LogError("{entry}", entry);
    }

    /// <inheritdoc />
    public void RecordSavedChanges(IDocumentSession session, IChangeSet commit)
    {
        _logger.LogInformation(
            "Persisted {updateAmount} updates, {insertAmount} inserts, and {deleteAmount} deletions",
            commit.Updated.Count(),
            commit.Inserted.Count(),
            commit.Deleted.Count());
    }

    /// <inheritdoc />
    public void OnBeforeExecute(NpgsqlCommand command)
    {
        _logger.LogError("Before PostgreSql command: {commandText}", command.CommandText);
    }
}