// DatabaseInitializer.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

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

        if (version < 2)
        {
            conn.Execute(Schema.V2);
            conn.Execute("PRAGMA user_version = 2");
        }
    }

    private static void RegisterTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new NullableGuidTypeHandler());
        SqlMapper.AddTypeHandler(new RoddenRatingTypeHandler());
    }
}
