// https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/dapper-limitations

using Dapper;
using Discord;
using System;
using System.Data;

namespace DiscordBotLib.DataAccess.Repositories
{
    abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        // Parameters are converted by Microsoft.Data.Sqlite
        public override void SetValue(IDbDataParameter parameter, T value)
            => parameter.Value = value;
    }

    class DateTimeOffsetHandler : SqliteTypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
            => DateTimeOffset.Parse((string)value);
    }

    class GuidHandler : SqliteTypeHandler<Guid>
    {
        public override Guid Parse(object value)
            => Guid.Parse((string)value);
    }

    class TimeSpanHandler : SqliteTypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value)
            => TimeSpan.Parse((string)value);
    }

    class ColorHandler : SqliteTypeHandler<Color>
    {
        public override Color Parse(object value)
        {
            var rgb = (value as string).Split(',');
            try
            {
                return new Color(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
