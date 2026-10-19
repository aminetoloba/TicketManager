using Dapper;
using Npgsql;
using TicketManager.Api.Dtos;
using TicketManager.Api.Models;

namespace TicketManager.Api.Repositories;

public class TicketReportRepository : ITicketReportRepository
{
    private readonly string _connectionString;

    public TicketReportRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<ProjectTicketSummaryDto>> GetOpenTicketsByProjectAsync()
    {
        const string sql = """
            SELECT p."Id"   AS "ProjectId",
                   p."Name" AS "ProjectName",
                   COUNT(t."Id")::int AS "OpenTickets",
                   (COUNT(t."Id") FILTER (WHERE t."Priority" = @HighPriority))::int AS "HighPriorityOpenTickets"
            FROM "Projects" p
            LEFT JOIN "Tickets" t
                   ON t."ProjectId" = p."Id"
                  AND t."Status" = @OpenStatus
            GROUP BY p."Id", p."Name"
            ORDER BY "OpenTickets" DESC, p."Name";
            """;

        await using var connection = new NpgsqlConnection(_connectionString);

        var result = await connection.QueryAsync<ProjectTicketSummaryDto>(sql, new
        {
            OpenStatus = TicketStatus.Open.ToString(),
            HighPriority = TicketPriority.High.ToString()
        });

        return result.ToList();
    }
}