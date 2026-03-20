using System;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace EnigmaWin.Sources.Data.Db;

public interface IDbConnectionFactory
{
    SqliteConnection Create();
}

internal sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbDir = Path.Combine(appData, "EnigmaWin");
        Directory.CreateDirectory(dbDir);
        var dbPath = Path.Combine(dbDir, "enigmawin.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public SqliteConnection Create()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        conn.Execute("PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL;");
        return conn;
    }
}
