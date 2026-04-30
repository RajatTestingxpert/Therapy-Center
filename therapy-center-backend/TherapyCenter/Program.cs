using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.Text;
using System.Text.Json.Serialization;
using TherapyCenter.Data;
using TherapyCenter.Repositories.Implementations;
using TherapyCenter.Repositories.Interfaces;
using TherapyCenter.Services.Implementations;
using TherapyCenter.Services.Interfaces;

namespace TherapyCenter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── 1. MySQL via Pomelo ───────────────────────────────────────────────────────
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // ── 2. JWT Authentication ─────────────────────────────────────────────────────
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            // ── 3. Authorization Policies ─────────────────────────────────────────────────
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("StaffOnly", p => p.RequireRole("Admin", "Receptionist"));
                options.AddPolicy("DoctorOnly", p => p.RequireRole("Doctor"));
                options.AddPolicy("PatientAccess", p => p.RequireRole("Patient", "Guardian"));
                options.AddPolicy("AllStaff", p => p.RequireRole("Admin", "Receptionist", "Doctor"));
            });

            // ── 4. Repositories ───────────────────────────────────────────────────────────
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<ITherapyRepository, TherapyRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IFindingRepository, FindingRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<ISlotRepository, SlotRepository>();

            // ── 5. Services ───────────────────────────────────────────────────────────────
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<ISlotService, SlotService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Make older databases compatible before EF Core queries start running.
                EnsurePatientUserLinkSchemaAsync(db).GetAwaiter().GetResult();

                // The database is self-healed above; avoid replaying a migration that may
                // try to add the same column again on older databases.
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static async Task EnsurePatientUserLinkSchemaAsync(AppDbContext db)
        {
            var connection = db.Database.GetDbConnection();
            await db.Database.OpenConnectionAsync();

            try
            {
                if (!await ColumnExistsAsync(connection, "Patients", "UserId"))
                {
                    await ExecuteAsync(connection, "ALTER TABLE `Patients` ADD COLUMN `UserId` int NULL");
                }

                if (!await IndexExistsAsync(connection, "Patients", "IX_Patients_UserId"))
                {
                    await ExecuteAsync(connection, "CREATE UNIQUE INDEX `IX_Patients_UserId` ON `Patients`(`UserId`)");
                }

                if (!await ForeignKeyExistsAsync(connection, "Patients", "FK_Patients_Users_UserId"))
                {
                    await ExecuteAsync(connection,
                        "ALTER TABLE `Patients` ADD CONSTRAINT `FK_Patients_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`UserId`) ON DELETE CASCADE");
                }
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }

        private static async Task<bool> ColumnExistsAsync(DbConnection connection, string tableName, string columnName)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = DATABASE()
                  AND table_name = @tableName
                  AND column_name = @columnName
                LIMIT 1;";
            AddParameter(command, "@tableName", tableName);
            AddParameter(command, "@columnName", columnName);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private static async Task<bool> IndexExistsAsync(DbConnection connection, string tableName, string indexName)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 1
                FROM information_schema.statistics
                WHERE table_schema = DATABASE()
                  AND table_name = @tableName
                  AND index_name = @indexName
                LIMIT 1;";
            AddParameter(command, "@tableName", tableName);
            AddParameter(command, "@indexName", indexName);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private static async Task<bool> ForeignKeyExistsAsync(DbConnection connection, string tableName, string constraintName)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 1
                FROM information_schema.table_constraints
                WHERE constraint_schema = DATABASE()
                  AND table_name = @tableName
                  AND constraint_name = @constraintName
                  AND constraint_type = 'FOREIGN KEY'
                LIMIT 1;";
            AddParameter(command, "@tableName", tableName);
            AddParameter(command, "@constraintName", constraintName);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private static async Task ExecuteAsync(DbConnection connection, string sql)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        private static void AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
