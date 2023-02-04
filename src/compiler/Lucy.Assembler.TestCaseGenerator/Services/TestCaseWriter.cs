using CsvHelper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lucy.Assembler.TestCaseGenerator.Services
{
    internal class TestCaseWriter
    {
        internal static async Task Run(ChannelReader<StatementTest> reader, Tracker tracker, int bits, string operation, bool writeSqliteDb)
        {
            var outputDir = PathHelper.FindUpwards("Output");
            await using var csvFile = File.CreateText($"{outputDir}/{operation}{bits}.csv");
            await using var csv = new CsvWriter(csvFile, CultureInfo.InvariantCulture);

            SqliteConnection? db = null;
            if (writeSqliteDb)
            {
                db = new SqliteConnection($"Data Source={outputDir}/asm-test-cases.db");
                await db.OpenAsync();
                await InitTable(db, operation, bits);
            }
            
            while (await reader.WaitToReadAsync())
            {
                SqliteCommand? cmd = null;
                SqliteTransaction? transaction = null;
                if (db != null)
                {
                    transaction = db.BeginTransaction();
                    cmd = db.CreateCommand();
                    cmd.CommandText = $"INSERT INTO op_{operation}{bits} (bits, operation, binary, error) VALUES ($bits, $operation, $binary, $error)";
                    cmd.Parameters.Add("$bits", SqliteType.Integer);
                    cmd.Parameters.Add("$operation", SqliteType.Text);
                    cmd.Parameters.Add("$binary", SqliteType.Text);
                    cmd.Parameters.Add("$error", SqliteType.Text);
                }
                
                var csvRecord = new List<CsvRecord>();

                while (reader.TryRead(out var item))
                {
                    if (cmd != null)
                    {
                        cmd.Parameters[0].Value = bits;
                        cmd.Parameters[1].Value = item.Text;
                        cmd.Parameters[2].Value = (object?)item.Binary ?? DBNull.Value;
                        cmd.Parameters[3].Value = item.Errors.Count == 0 ? DBNull.Value : string.Join("\n", item.Errors);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    
                    if (item.Binary != null)
                        csvRecord.Add(new CsvRecord(item.Text, item.Binary));

                    tracker.Done++;
                }

                if (cmd != null)
                    await cmd.DisposeAsync();

                if (transaction != null)
                {
                    await transaction.CommitAsync();
                    await transaction.DisposeAsync();
                }
                
                await csv.WriteRecordsAsync(csvRecord);
            }
        }

        private static async Task InitTable(SqliteConnection db, string operation, int bits)
        {
            var cmd = db.CreateCommand();
            cmd.CommandText = $"DROP TABLE IF EXISTS op_{operation}{bits};";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = $@"
            CREATE TABLE op_{operation}{bits} (
                id        INTEGER PRIMARY KEY,
                bits      INTEGER NOT NULL,
                operation TEXT NOT NULL,
                binary    TEXT NULL,
                error     TEXT NULL
            )";
            await cmd.ExecuteNonQueryAsync();
        }
    }

    internal class Tracker
    {
        public int Done { get; set; }
    }

    internal record CsvRecord(string Operation, string Binary);
}
