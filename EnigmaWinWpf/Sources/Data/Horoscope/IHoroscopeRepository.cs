// IHoroscopeRepository.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Data.Horoscope;

public interface IHoroscopeRepository
{
    Task<IEnumerable<Domain.Horoscope>> FetchAllAsync();
    Task<Domain.Horoscope?> FetchAsync(Guid id);
    Task AddAsync(Domain.Horoscope horoscope);
    Task UpdateAsync(Domain.Horoscope horoscope);
    Task DeleteAsync(Guid id);

    Task AddDateTimeAsync(Guid horoscopeId, HoroscopeDateTime dateTime);
    Task SetPreferredAsync(Guid dateTimeId, Guid horoscopeId);
    Task UpdateDateTimeAsync(HoroscopeDateTime dateTime);
    Task DeleteDateTimeAsync(Guid dateTimeId);
}
