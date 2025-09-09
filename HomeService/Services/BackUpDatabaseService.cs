
using DataLayer;
using Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static HomeService.Program;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HomeService.Services
{
    public class BackUpDatabaseService : ScheduledTask
    {
        public BackUpDatabaseService() : base(Configuration.Appsettings.GetSection("BackUpDatabaseService").GetValue<string>("CronPattern"),
                  Configuration.Appsettings.GetSection("BackUpDatabaseService").GetValue<bool>("ServiceActive"))
        {

        }

        protected async override Task ExecuteTask()
        {
            try
            {
                using (var db = new DataBaseContext())
                {
                    var connection = (SqlConnection)db.Database.GetDbConnection();
                    var databaseName = connection.Database;
                    var backupFileName = $@"C:\Users\Nikolay Petkov\OneDrive\Desktop\SQL Backup\{databaseName}_{DateTime.Now.ToString("dd-MM-yyyy")}.bak";
                    var sqlCommand = $@"BACKUP DATABASE [{databaseName}] TO DISK = N'{backupFileName}' WITH INIT, NAME = N'{databaseName} - Full Backup', FORMAT;";

                    using (var comman = new SqlCommand(sqlCommand, connection))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        comman.ExecuteNonQuery();

                        WriteLog.Log($"Database backup completed: {backupFileName} !");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(ex.Message, ex.StackTrace!);
            }
        }
    }
}
