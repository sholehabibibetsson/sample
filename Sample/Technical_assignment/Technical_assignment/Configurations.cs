using Microsoft.Extensions.Configuration;

namespace Technical_assignment
{
    public static class Settings
    {
        public static SqlConnectionStringsOptions SqlConnectionStrings;
        public static CronOptions Cron;
        public static void LoadConfigurations(this IConfiguration configuration)
        {
            SqlConnectionStrings = configuration.GetSection(SqlConnectionStringsOptions.SqlConnectionStrings)
                                                     .Get<SqlConnectionStringsOptions>();
            Cron = configuration.GetSection(CronOptions.Cron)
                                                  .Get<CronOptions>();
        }
    }

    public class SqlConnectionStringsOptions
    {
        public const string SqlConnectionStrings = "SqlConnectionStrings";

        public string Dof { get; set; } = String.Empty;
    }

    public class CronOptions
    {
        public const string Cron = "CronExpression";

        public string Expression { get; set; } = String.Empty;
    }
}