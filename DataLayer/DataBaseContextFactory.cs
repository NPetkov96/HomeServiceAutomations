using Microsoft.EntityFrameworkCore.Design;

namespace DataLayer;

public sealed class DataBaseContextFactory : IDesignTimeDbContextFactory<DataBaseContext>
{
    public DataBaseContext CreateDbContext(string[] args) =>
        DataBaseContext.Create();
}
