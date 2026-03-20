using System;
using Dapper;

namespace EnigmaWin.Sources.Data.Db;

public sealed class DatabaseInitializer(IDbConnectionFactory factory)
{
    public void Initialize()
    {
        RegisterTypeHandlers();
        RunMigrations();
    }

    private void RunMigrations()
    {
        using var conn = factory.Create();
        var version = conn.ExecuteScalar<int>("PRAGMA user_version");

        if (version < 1)
        {
            conn.Execute(Schema.V1);
            conn.Execute("PRAGMA user_version = 1");
        }
    }

    private static void RegisterTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new NullableGuidTypeHandler());
        SqlMapper.AddTypeHandler(new RoddenRatingTypeHandler());
    }
}
