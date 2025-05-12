using Microsoft.Data.SqlClient;
using Test1B.DTOs;
using Test1B.Exceptions;

namespace Test1B.Services;

public class VisitsService : IVisitsService
{
    private readonly IConfiguration _configuration;
    public VisitsService(IConfiguration configuration) => _configuration = configuration;
    
    public async Task<VisitResponseDTO> GetVisitByIdAsync(int id)
    {
        const string checkVisitExistsSQL_Command = """
                                                        SELECT 1 FROM Visit WHERE visit_id = @VisitId;
                                                   """;
        const string getVisitSQL_Command = """
                                               SELECT 
                                                   v.date,
                                                   c.first_name,
                                                   c.last_name,
                                                   c.date_of_birth,
                                                   m.mechanic_id,
                                                   m.licence_number,
                                                   s.name AS service_name,
                                                   vs.service_fee
                                               FROM visit v
                                               JOIN client c ON v.client_id = c.client_id
                                               JOIN mechanic m ON v.mechanic_id = m.mechanic_id
                                               JOIN visit_service vs ON vs.visit_id = v.visit_id
                                               JOIN service s ON s.service_id = vs.service_id
                                               WHERE v.visit_id = @VisitId; 
                                           """;

        VisitResponseDTO? response = null;
        using (var conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await conn.OpenAsync();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    using (var checkVisitExistsCmd = new SqlCommand(checkVisitExistsSQL_Command, conn, transaction))
                    {
                        checkVisitExistsCmd.Parameters.AddWithValue("@VisitId", id);
                        if (await checkVisitExistsCmd.ExecuteScalarAsync() == null)
                            throw new ConflictException($"Visit {id} was not found.");
                    }
                    
                    using (var getVisitCmd = new SqlCommand(getVisitSQL_Command, conn, transaction))
                    {
                        getVisitCmd.Parameters.AddWithValue("@VisitId", id);
                        using (var reader = await getVisitCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response = new VisitResponseDTO()
                                {
                                    Date = reader.GetDateTime(0),
                                    ClientInfo = new ClientDTO()
                                    {
                                        FirstName = reader.GetString(1),
                                        LastName = reader.GetString(2),
                                        DateOfBirth = reader.GetDateTime(3)
                                    },
                                    MechanicInfo = new MechanicDTO()
                                    {
                                        MechanicID = reader.GetInt32(4),
                                        LicenseNumber = reader.GetString(5)
                                    },
                                    VisitServices = new List<VisitServiceDTO>()
                                };
                                
                                response.VisitServices.Add(new VisitServiceDTO()
                                {
                                    Name = reader.GetString(6),
                                    ServiceFee = reader.GetDecimal(7)
                                });

                            }
                            
                        }
                    }
                    
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        if (response is null) throw new NotFoundException($"Visit {id} was not found.");

        return response;
    }

    public async Task<int> CreateNewVisitAsync(VisitRequestDTO requestDto)
    {
        const string checkVisitExistSQL_Command = "SELECT 1 FROM Visit WHERE visit_id = @VisitId;";
        const string checkClientNotExistSQL_Command = "SELECT 1 FROM Client WHERE client_id = @ClientId;";
        const string checkMechanicNotExistSQL_Command = "SELECT 1 FROM Mechanic WHERE license_number = @LicenseNumber;";
        const string checkServiceNotExistSQL_Command = "SELECT 1 FROM Service WHERE name = @ServiceName;";
        
        const string createVisitSQL_Command = 
            """
                INSERT INTO visit (visit_id, client_id, mechanic_id, date)
                VALUES (@VisitId, @ClientId, @MechanicId, GETDATE());
            """;
        
        using (var conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        using (var transaction = conn.BeginTransaction())
        {
            try
            {
                using (var checkVisitExistsCmd = new SqlCommand(checkVisitExistSQL_Command, conn, transaction))
                {
                    checkVisitExistsCmd.Parameters.AddWithValue("@VisitID", requestDto.VisitId);
                    if (await checkVisitExistsCmd.ExecuteScalarAsync() != null)
                        throw new ConflictException($"Visit {requestDto.VisitId} already exists.");
                }

                using (var checkClientNotExistCmd = new SqlCommand(checkClientNotExistSQL_Command, conn, transaction))
                {
                    checkClientNotExistCmd.Parameters.AddWithValue("@ClientId", requestDto.ClientId);
                    if (await checkClientNotExistCmd.ExecuteScalarAsync() == null)
                        throw new NotFoundException($"Client {requestDto.ClientId} was not found.");
                }

                using (var checkMechanicCmd = new SqlCommand(checkMechanicNotExistSQL_Command, conn, transaction))
                {
                    checkMechanicCmd.Parameters.AddWithValue("@LicenseNumber", requestDto.MechanicInfo.LicenseNumber);
                    if (await checkMechanicCmd.ExecuteScalarAsync() == null)
                    {
                        throw new NotFoundException(
                            $"Mechanic with license number {requestDto.MechanicInfo.LicenseNumber} does not exist.");

                    }
                }

                using (var checkServiceCmd = new SqlCommand(checkServiceNotExistSQL_Command, conn, transaction))
                {
                    foreach (var service in requestDto.VisitServices)
                    {
                        checkServiceCmd.Parameters.AddWithValue("@ServiceName", service.Name);
                        if (await checkServiceCmd.ExecuteScalarAsync() == null)
                            throw new NotFoundException($"Service {service.Name} was not found.");
                    }
                }

                using (var addVisitCmd = new SqlCommand(createVisitSQL_Command, conn, transaction))
                {
                    addVisitCmd.Parameters.AddWithValue("@VisitId", requestDto.VisitId);
                    addVisitCmd.Parameters.AddWithValue("@ClientId", requestDto.ClientId);
                    addVisitCmd.Parameters.AddWithValue("@MechanicId", requestDto.MechanicInfo.MechanicID);

                    var insertedID = Convert.ToInt32(await addVisitCmd.ExecuteScalarAsync());

                    transaction.Commit();
                    return insertedID;
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}