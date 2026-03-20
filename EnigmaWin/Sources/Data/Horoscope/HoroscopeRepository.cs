using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Data.Horoscope;

internal sealed class HoroscopeRepository(IDbConnectionFactory factory) : IHoroscopeRepository
{
    public async Task<IEnumerable<Domain.Horoscope>> FetchAllAsync()
    {
        using var conn = factory.Create();
        return await conn.QueryAsync<Domain.Horoscope>(
            "SELECT * FROM Horoscope ORDER BY Name");
    }

    public async Task<Domain.Horoscope?> FetchAsync(Guid id)
    {
        using var conn = factory.Create();
        var horoscope = await conn.QuerySingleOrDefaultAsync<Domain.Horoscope>(
            "SELECT * FROM Horoscope WHERE Id = @Id", new { Id = id });
        if (horoscope is null) return null;

        var dateTimes = await conn.QueryAsync<HoroscopeDateTime>(
            "SELECT * FROM HoroscopeDateTime WHERE HoroscopeId = @Id", new { Id = id });
        return horoscope with { DateTimes = dateTimes.ToList() };
    }

    public async Task AddAsync(Domain.Horoscope horoscope)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("""
            INSERT INTO Horoscope (Id, Name, Category, Notes, Source, RoddenRating, PlaceName, Latitude, Longitude)
            VALUES (@Id, @Name, @Category, @Notes, @Source, @RoddenRating, @PlaceName, @Latitude, @Longitude)
            """, horoscope);
    }

    public async Task UpdateAsync(Domain.Horoscope horoscope)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("""
            UPDATE Horoscope SET
                Name         = @Name,
                Category     = @Category,
                Notes        = @Notes,
                Source       = @Source,
                RoddenRating = @RoddenRating,
                PlaceName    = @PlaceName,
                Latitude     = @Latitude,
                Longitude    = @Longitude
            WHERE Id = @Id
            """, horoscope);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("DELETE FROM Horoscope WHERE Id = @Id", new { Id = id });
    }

    public async Task AddDateTimeAsync(Guid horoscopeId, HoroscopeDateTime dateTime)
    {
        using var conn = factory.Create();
        using var tx = conn.BeginTransaction();

        if (dateTime.IsPreferred)
            await conn.ExecuteAsync(
                "UPDATE HoroscopeDateTime SET IsPreferred = 0 WHERE HoroscopeId = @HoroscopeId",
                new { HoroscopeId = horoscopeId }, tx);

        await conn.ExecuteAsync("""
            INSERT INTO HoroscopeDateTime
                (Id, HoroscopeId, JulianDate, TimeZoneIdentifier, TimeIsUnknown, IsPreferred, Label, OriginalInput)
            VALUES
                (@Id, @HoroscopeId, @JulianDate, @TimeZoneIdentifier, @TimeIsUnknown, @IsPreferred, @Label, @OriginalInput)
            """, dateTime with { HoroscopeId = horoscopeId }, tx);

        tx.Commit();
    }

    public async Task SetPreferredAsync(Guid dateTimeId, Guid horoscopeId)
    {
        using var conn = factory.Create();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync(
            "UPDATE HoroscopeDateTime SET IsPreferred = 0 WHERE HoroscopeId = @HoroscopeId",
            new { HoroscopeId = horoscopeId }, tx);

        await conn.ExecuteAsync(
            "UPDATE HoroscopeDateTime SET IsPreferred = 1 WHERE Id = @Id",
            new { Id = dateTimeId }, tx);

        tx.Commit();
    }

    public async Task UpdateDateTimeAsync(HoroscopeDateTime dateTime)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("""
            UPDATE HoroscopeDateTime SET
                JulianDate         = @JulianDate,
                TimeZoneIdentifier = @TimeZoneIdentifier,
                TimeIsUnknown      = @TimeIsUnknown,
                IsPreferred        = @IsPreferred,
                Label              = @Label,
                OriginalInput      = @OriginalInput
            WHERE Id = @Id
            """, dateTime);
    }

    public async Task DeleteDateTimeAsync(Guid dateTimeId)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync("DELETE FROM HoroscopeDateTime WHERE Id = @Id", new { Id = dateTimeId });
    }
}
