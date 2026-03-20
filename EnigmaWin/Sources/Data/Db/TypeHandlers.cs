using System;
using System.Data;
using Dapper;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Data.Db;

internal sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value) => Guid.Parse((string)value);
    public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value.ToString();
}

internal sealed class NullableGuidTypeHandler : SqlMapper.TypeHandler<Guid?>
{
    public override Guid? Parse(object value) => value is null or DBNull ? null : Guid.Parse((string)value);
    public override void SetValue(IDbDataParameter parameter, Guid? value) => parameter.Value = value?.ToString() ?? (object)DBNull.Value;
}

internal sealed class RoddenRatingTypeHandler : SqlMapper.TypeHandler<RoddenRating>
{
    public override RoddenRating Parse(object value) => Enum.Parse<RoddenRating>((string)value);
    public override void SetValue(IDbDataParameter parameter, RoddenRating value) => parameter.Value = value.ToString();
}
