using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Pricing.Inventory.Application;
using Pricing.Inventory.Domain.Devices;
using Pricing.Inventory.Infrastructure.Persistence;

namespace Pricing.Inventory.Infrastructure.Persistence.Repositories;

public class DeviceRepository(InventoryDbContext context, IInventoryUnitOfWork unitOfWork) : IDeviceRepository
{
    public async Task<HashSet<string>> FindExistingEanCodesAsync(IReadOnlyList<string> eanCodes, CancellationToken ct)
    {
        const int batchSize = 2000;
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < eanCodes.Count; i += batchSize)
        {
            var batch = eanCodes.Skip(i).Take(batchSize).ToList();
            var existing = await context.Devices
                .Where(d => batch.Contains(d.EanCode))
                .Select(d => d.EanCode)
                .ToListAsync(ct);
            foreach (var code in existing)
                result.Add(code);
        }

        return result;
    }

    public async Task BulkInsertAsync(IReadOnlyList<Device> devices, CancellationToken ct)
    {
        var table = BuildDataTable(devices);

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var connection = (SqlConnection)context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(ct);

            var transaction = (SqlTransaction)context.Database.CurrentTransaction!.GetDbTransaction();

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = "[inventory].[Devices]",
                BatchSize = 5000
            };

            bulkCopy.ColumnMappings.Add("Id", "Id");
            bulkCopy.ColumnMappings.Add("EanCode", "EanCode");
            bulkCopy.ColumnMappings.Add("Name", "Name");
            bulkCopy.ColumnMappings.Add("TypeCode", "TypeCode");
            bulkCopy.ColumnMappings.Add("SubtypeCode", "SubtypeCode");
            bulkCopy.ColumnMappings.Add("ManufacturerCode", "ManufacturerCode");

            await bulkCopy.WriteToServerAsync(table, ct);
        }, ct);
    }

    private static DataTable BuildDataTable(IReadOnlyList<Device> devices)
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(Guid));
        table.Columns.Add("EanCode", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("TypeCode", typeof(string));
        table.Columns.Add("SubtypeCode", typeof(string));
        table.Columns.Add("ManufacturerCode", typeof(string));

        foreach (var device in devices)
        {
            table.Rows.Add(
                device.Id.Value,
                device.EanCode,
                device.Name,
                device.TypeCode,
                device.SubtypeCode ?? (object)DBNull.Value,
                device.ManufacturerCode ?? (object)DBNull.Value);
        }

        return table;
    }
}
