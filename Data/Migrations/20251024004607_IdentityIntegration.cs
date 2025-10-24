using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaExpertoONGJuventudSinLimites.Data.Migrations
{
    /// <inheritdoc />
    public partial class IdentityIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "ConfiguracionMotor",
                schema: "dbo",
                columns: table => new
                {
                    Clave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionMotor", x => x.Clave);
                });

            migrationBuilder.CreateTable(
                name: "DiccionarioObservaciones",
                schema: "dbo",
                columns: table => new
                {
                    DiccionarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Expresion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Ponderacion = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Ambito = table.Column<byte>(type: "tinyint", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiccionarioObservaciones", x => x.DiccionarioId);
                });

            migrationBuilder.CreateTable(
                name: "EjecucionMotor",
                schema: "dbo",
                columns: table => new
                {
                    EjecucionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InicioUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FinUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Ambito = table.Column<byte>(type: "tinyint", nullable: false),
                    ResultadoResumen = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Exitos = table.Column<int>(type: "int", nullable: false),
                    Errores = table.Column<int>(type: "int", nullable: false),
                    TransaccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjecucionMotor", x => x.EjecucionId);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                schema: "dbo",
                columns: table => new
                {
                    PersonaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.PersonaId);
                });

            migrationBuilder.CreateTable(
                name: "Programa",
                schema: "dbo",
                columns: table => new
                {
                    ProgramaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    InferenciaActiva = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programa", x => x.ProgramaId);
                });

            migrationBuilder.CreateTable(
                name: "Regla",
                schema: "dbo",
                columns: table => new
                {
                    ReglaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Severidad = table.Column<byte>(type: "tinyint", nullable: false),
                    Objetivo = table.Column<byte>(type: "tinyint", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Prioridad = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regla", x => x.ReglaId);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Participante",
                schema: "dbo",
                columns: table => new
                {
                    ParticipanteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "date", nullable: false),
                    FechaBaja = table.Column<DateTime>(type: "date", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participante", x => x.ParticipanteId);
                    table.ForeignKey(
                        name: "FK_Participante_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalSchema: "dbo",
                        principalTable: "Persona",
                        principalColumn: "PersonaId");
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuario_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalSchema: "dbo",
                        principalTable: "Persona",
                        principalColumn: "PersonaId");
                });

            migrationBuilder.CreateTable(
                name: "Actividad",
                schema: "dbo",
                columns: table => new
                {
                    ActividadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Lugar = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actividad", x => x.ActividadId);
                    table.ForeignKey(
                        name: "FK_Actividad_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionMotorOverride",
                schema: "dbo",
                columns: table => new
                {
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionMotorOverride", x => new { x.ProgramaId, x.Clave });
                    table.ForeignKey(
                        name: "FK_ConfiguracionMotorOverride_ConfiguracionMotor_Clave",
                        column: x => x.Clave,
                        principalSchema: "dbo",
                        principalTable: "ConfiguracionMotor",
                        principalColumn: "Clave");
                    table.ForeignKey(
                        name: "FK_ConfiguracionMotorOverride_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                });

            migrationBuilder.CreateTable(
                name: "DiccionarioObservacionesPrograma",
                schema: "dbo",
                columns: table => new
                {
                    DiccionarioId = table.Column<int>(type: "int", nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiccionarioObservacionesPrograma", x => new { x.DiccionarioId, x.ProgramaId });
                    table.ForeignKey(
                        name: "FK_DiccionarioObservacionesPrograma_DiccionarioObservaciones_DiccionarioId",
                        column: x => x.DiccionarioId,
                        principalSchema: "dbo",
                        principalTable: "DiccionarioObservaciones",
                        principalColumn: "DiccionarioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiccionarioObservacionesPrograma_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                });

            migrationBuilder.CreateTable(
                name: "MetricasProgramaMes",
                schema: "dbo",
                columns: table => new
                {
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    AnioMes = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: false),
                    ActividadesPlanificadas = table.Column<int>(type: "int", nullable: false),
                    ActividadesEjecutadas = table.Column<int>(type: "int", nullable: false),
                    PorcCumplimiento = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RetrasoPromedioDias = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    PorcAsistenciaProm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricasProgramaMes", x => new { x.ProgramaId, x.AnioMes });
                    table.ForeignKey(
                        name: "FK_MetricasProgramaMes_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                });

            migrationBuilder.CreateTable(
                name: "POA_Plantilla",
                schema: "dbo",
                columns: table => new
                {
                    PlantillaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "date", nullable: true),
                    VigenteHasta = table.Column<DateTime>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_Plantilla", x => x.PlantillaId);
                    table.ForeignKey(
                        name: "FK_POA_Plantilla_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                });

            migrationBuilder.CreateTable(
                name: "POA_SnapshotMensual",
                schema: "dbo",
                columns: table => new
                {
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    AnioMes = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: false),
                    PlantillaVersion = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_SnapshotMensual", x => new { x.ProgramaId, x.AnioMes });
                    table.ForeignKey(
                        name: "FK_POA_SnapshotMensual_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                });

            migrationBuilder.CreateTable(
                name: "ReglaParametro",
                schema: "dbo",
                columns: table => new
                {
                    ReglaParametroId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReglaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglaParametro", x => x.ReglaParametroId);
                    table.ForeignKey(
                        name: "FK_ReglaParametro_Regla_ReglaId",
                        column: x => x.ReglaId,
                        principalSchema: "dbo",
                        principalTable: "Regla",
                        principalColumn: "ReglaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReglaParametroOverride",
                schema: "dbo",
                columns: table => new
                {
                    ReglaId = table.Column<int>(type: "int", nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglaParametroOverride", x => new { x.ReglaId, x.ProgramaId, x.Nombre });
                    table.ForeignKey(
                        name: "FK_ReglaParametroOverride_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReglaParametroOverride_Regla_ReglaId",
                        column: x => x.ReglaId,
                        principalSchema: "dbo",
                        principalTable: "Regla",
                        principalColumn: "ReglaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolClaim",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolClaim_Rol_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiesgoParticipantePrograma",
                schema: "dbo",
                columns: table => new
                {
                    RiesgoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipanteId = table.Column<int>(type: "int", nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    FechaCorte = table.Column<DateTime>(type: "date", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Banda = table.Column<byte>(type: "tinyint", nullable: false),
                    ExplicacionCorta = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiesgoParticipantePrograma", x => x.RiesgoId);
                    table.ForeignKey(
                        name: "FK_RiesgoParticipantePrograma_Participante_ParticipanteId",
                        column: x => x.ParticipanteId,
                        principalSchema: "dbo",
                        principalTable: "Participante",
                        principalColumn: "ParticipanteId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiesgoParticipantePrograma_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                schema: "dbo",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaEventoUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    UsuarioActorId = table.Column<int>(type: "int", nullable: true),
                    Operacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tabla = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ClavePrimaria = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DatosAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatosNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransaccionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_Logs_Usuario_UsuarioActorId",
                        column: x => x.UsuarioActorId,
                        principalSchema: "dbo",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioClaim",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioClaim_Usuario_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioLogin",
                schema: "dbo",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioLogin", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UsuarioLogin_Usuario_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioPrograma",
                schema: "dbo",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    Desde = table.Column<DateTime>(type: "date", nullable: false),
                    Hasta = table.Column<DateTime>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPrograma", x => new { x.UsuarioId, x.ProgramaId, x.Desde });
                    table.ForeignKey(
                        name: "FK_UsuarioPrograma_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioPrograma_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRol",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AsignadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRol", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UsuarioRol_Rol_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRol_Usuario_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioToken",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioToken", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UsuarioToken_Usuario_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActividadParticipante",
                schema: "dbo",
                columns: table => new
                {
                    ActividadId = table.Column<int>(type: "int", nullable: false),
                    ParticipanteId = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<byte>(type: "tinyint", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadParticipante", x => new { x.ActividadId, x.ParticipanteId });
                    table.ForeignKey(
                        name: "FK_ActividadParticipante_Actividad_ActividadId",
                        column: x => x.ActividadId,
                        principalSchema: "dbo",
                        principalTable: "Actividad",
                        principalColumn: "ActividadId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActividadParticipante_Participante_ParticipanteId",
                        column: x => x.ParticipanteId,
                        principalSchema: "dbo",
                        principalTable: "Participante",
                        principalColumn: "ParticipanteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alerta",
                schema: "dbo",
                columns: table => new
                {
                    AlertaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReglaId = table.Column<int>(type: "int", nullable: false),
                    Severidad = table.Column<byte>(type: "tinyint", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    ActividadId = table.Column<int>(type: "int", nullable: true),
                    ParticipanteId = table.Column<int>(type: "int", nullable: true),
                    GeneradaEn = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ResueltaPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ResueltaEn = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerta", x => x.AlertaId);
                    table.ForeignKey(
                        name: "FK_Alerta_Actividad_ActividadId",
                        column: x => x.ActividadId,
                        principalSchema: "dbo",
                        principalTable: "Actividad",
                        principalColumn: "ActividadId");
                    table.ForeignKey(
                        name: "FK_Alerta_Participante_ParticipanteId",
                        column: x => x.ParticipanteId,
                        principalSchema: "dbo",
                        principalTable: "Participante",
                        principalColumn: "ParticipanteId");
                    table.ForeignKey(
                        name: "FK_Alerta_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                    table.ForeignKey(
                        name: "FK_Alerta_Regla_ReglaId",
                        column: x => x.ReglaId,
                        principalSchema: "dbo",
                        principalTable: "Regla",
                        principalColumn: "ReglaId");
                });

            migrationBuilder.CreateTable(
                name: "Asistencia",
                schema: "dbo",
                columns: table => new
                {
                    AsistenciaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActividadId = table.Column<int>(type: "int", nullable: false),
                    ParticipanteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencia", x => x.AsistenciaId);
                    table.ForeignKey(
                        name: "FK_Asistencia_Actividad_ActividadId",
                        column: x => x.ActividadId,
                        principalSchema: "dbo",
                        principalTable: "Actividad",
                        principalColumn: "ActividadId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asistencia_Participante_ParticipanteId",
                        column: x => x.ParticipanteId,
                        principalSchema: "dbo",
                        principalTable: "Participante",
                        principalColumn: "ParticipanteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvidenciaActividad",
                schema: "dbo",
                columns: table => new
                {
                    EvidenciaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActividadId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    ArchivoPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SubidoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    SubidoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenciaActividad", x => x.EvidenciaId);
                    table.ForeignKey(
                        name: "FK_EvidenciaActividad_Actividad_ActividadId",
                        column: x => x.ActividadId,
                        principalSchema: "dbo",
                        principalTable: "Actividad",
                        principalColumn: "ActividadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchRegla",
                schema: "dbo",
                columns: table => new
                {
                    MatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjecucionId = table.Column<int>(type: "int", nullable: false),
                    ReglaId = table.Column<int>(type: "int", nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    ActividadId = table.Column<int>(type: "int", nullable: true),
                    ParticipanteId = table.Column<int>(type: "int", nullable: true),
                    Mensaje = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    GeneroAlerta = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchRegla", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_MatchRegla_Actividad_ActividadId",
                        column: x => x.ActividadId,
                        principalSchema: "dbo",
                        principalTable: "Actividad",
                        principalColumn: "ActividadId");
                    table.ForeignKey(
                        name: "FK_MatchRegla_EjecucionMotor_EjecucionId",
                        column: x => x.EjecucionId,
                        principalSchema: "dbo",
                        principalTable: "EjecucionMotor",
                        principalColumn: "EjecucionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchRegla_Participante_ParticipanteId",
                        column: x => x.ParticipanteId,
                        principalSchema: "dbo",
                        principalTable: "Participante",
                        principalColumn: "ParticipanteId");
                    table.ForeignKey(
                        name: "FK_MatchRegla_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                    table.ForeignKey(
                        name: "FK_MatchRegla_Regla_ReglaId",
                        column: x => x.ReglaId,
                        principalSchema: "dbo",
                        principalTable: "Regla",
                        principalColumn: "ReglaId");
                });

            migrationBuilder.CreateTable(
                name: "POA_Instancia",
                schema: "dbo",
                columns: table => new
                {
                    InstanciaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramaId = table.Column<int>(type: "int", nullable: false),
                    PlantillaId = table.Column<int>(type: "int", nullable: false),
                    PeriodoAnio = table.Column<short>(type: "smallint", nullable: false),
                    PeriodoMes = table.Column<byte>(type: "tinyint", nullable: true),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_Instancia", x => x.InstanciaId);
                    table.ForeignKey(
                        name: "FK_POA_Instancia_POA_Plantilla_PlantillaId",
                        column: x => x.PlantillaId,
                        principalSchema: "dbo",
                        principalTable: "POA_Plantilla",
                        principalColumn: "PlantillaId");
                    table.ForeignKey(
                        name: "FK_POA_Instancia_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                });

            migrationBuilder.CreateTable(
                name: "POA_PlantillaSeccion",
                schema: "dbo",
                columns: table => new
                {
                    SeccionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantillaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_PlantillaSeccion", x => x.SeccionId);
                    table.ForeignKey(
                        name: "FK_POA_PlantillaSeccion_POA_Plantilla_PlantillaId",
                        column: x => x.PlantillaId,
                        principalSchema: "dbo",
                        principalTable: "POA_Plantilla",
                        principalColumn: "PlantillaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiesgoDetalle",
                schema: "dbo",
                columns: table => new
                {
                    RiesgoDetalleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RiesgoId = table.Column<int>(type: "int", nullable: false),
                    NombreFeature = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ValorNumerico = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    ValorTexto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PesoContribucion = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiesgoDetalle", x => x.RiesgoDetalleId);
                    table.ForeignKey(
                        name: "FK_RiesgoDetalle_RiesgoParticipantePrograma_RiesgoId",
                        column: x => x.RiesgoId,
                        principalSchema: "dbo",
                        principalTable: "RiesgoParticipantePrograma",
                        principalColumn: "RiesgoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POA_Campo",
                schema: "dbo",
                columns: table => new
                {
                    CampoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantillaId = table.Column<int>(type: "int", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Etiqueta = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoDato = table.Column<byte>(type: "tinyint", nullable: false),
                    Requerido = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Alcance = table.Column<byte>(type: "tinyint", nullable: false),
                    SeccionId = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_Campo", x => x.CampoId);
                    table.ForeignKey(
                        name: "FK_POA_Campo_POA_PlantillaSeccion_SeccionId",
                        column: x => x.SeccionId,
                        principalSchema: "dbo",
                        principalTable: "POA_PlantillaSeccion",
                        principalColumn: "SeccionId");
                    table.ForeignKey(
                        name: "FK_POA_Campo_POA_Plantilla_PlantillaId",
                        column: x => x.PlantillaId,
                        principalSchema: "dbo",
                        principalTable: "POA_Plantilla",
                        principalColumn: "PlantillaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POA_Archivo",
                schema: "dbo",
                columns: table => new
                {
                    ArchivoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanciaId = table.Column<int>(type: "int", nullable: false),
                    CampoId = table.Column<int>(type: "int", nullable: true),
                    ArchivoPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SubidoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    SubidoEn = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_Archivo", x => x.ArchivoId);
                    table.ForeignKey(
                        name: "FK_POA_Archivo_POA_Campo_CampoId",
                        column: x => x.CampoId,
                        principalSchema: "dbo",
                        principalTable: "POA_Campo",
                        principalColumn: "CampoId");
                    table.ForeignKey(
                        name: "FK_POA_Archivo_POA_Instancia_InstanciaId",
                        column: x => x.InstanciaId,
                        principalSchema: "dbo",
                        principalTable: "POA_Instancia",
                        principalColumn: "InstanciaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POA_CampoOpcion",
                schema: "dbo",
                columns: table => new
                {
                    OpcionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampoId = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Etiqueta = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_CampoOpcion", x => x.OpcionId);
                    table.ForeignKey(
                        name: "FK_POA_CampoOpcion_POA_Campo_CampoId",
                        column: x => x.CampoId,
                        principalSchema: "dbo",
                        principalTable: "POA_Campo",
                        principalColumn: "CampoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POA_CampoValidacion",
                schema: "dbo",
                columns: table => new
                {
                    ValidacionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    Parametro = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_CampoValidacion", x => x.ValidacionId);
                    table.ForeignKey(
                        name: "FK_POA_CampoValidacion_POA_Campo_CampoId",
                        column: x => x.CampoId,
                        principalSchema: "dbo",
                        principalTable: "POA_Campo",
                        principalColumn: "CampoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POA_Valor",
                schema: "dbo",
                columns: table => new
                {
                    ValorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanciaId = table.Column<int>(type: "int", nullable: false),
                    CampoId = table.Column<int>(type: "int", nullable: false),
                    ProgramaId = table.Column<int>(type: "int", nullable: true),
                    ActividadId = table.Column<int>(type: "int", nullable: true),
                    ParticipanteId = table.Column<int>(type: "int", nullable: true),
                    ValorTexto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorNumero = table.Column<int>(type: "int", nullable: true),
                    ValorDecimal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValorFecha = table.Column<DateTime>(type: "date", nullable: true),
                    ValorBool = table.Column<bool>(type: "bit", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POA_Valor", x => x.ValorId);
                    table.ForeignKey(
                        name: "FK_POA_Valor_Actividad_ActividadId",
                        column: x => x.ActividadId,
                        principalSchema: "dbo",
                        principalTable: "Actividad",
                        principalColumn: "ActividadId");
                    table.ForeignKey(
                        name: "FK_POA_Valor_POA_Campo_CampoId",
                        column: x => x.CampoId,
                        principalSchema: "dbo",
                        principalTable: "POA_Campo",
                        principalColumn: "CampoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_POA_Valor_POA_Instancia_InstanciaId",
                        column: x => x.InstanciaId,
                        principalSchema: "dbo",
                        principalTable: "POA_Instancia",
                        principalColumn: "InstanciaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_POA_Valor_Participante_ParticipanteId",
                        column: x => x.ParticipanteId,
                        principalSchema: "dbo",
                        principalTable: "Participante",
                        principalColumn: "ParticipanteId");
                    table.ForeignKey(
                        name: "FK_POA_Valor_Programa_ProgramaId",
                        column: x => x.ProgramaId,
                        principalSchema: "dbo",
                        principalTable: "Programa",
                        principalColumn: "ProgramaId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actividad_Programa_Fecha",
                schema: "dbo",
                table: "Actividad",
                columns: new[] { "ProgramaId", "FechaInicio" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AP_Participante",
                schema: "dbo",
                table: "ActividadParticipante",
                column: "ParticipanteId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_ActividadId",
                schema: "dbo",
                table: "Alerta",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_Estado_Severidad",
                schema: "dbo",
                table: "Alerta",
                columns: new[] { "Estado", "Severidad" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_ParticipanteId",
                schema: "dbo",
                table: "Alerta",
                column: "ParticipanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_ReglaId",
                schema: "dbo",
                table: "Alerta",
                column: "ReglaId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_Targets",
                schema: "dbo",
                table: "Alerta",
                columns: new[] { "ProgramaId", "ActividadId", "ParticipanteId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_Participante_Fecha",
                schema: "dbo",
                table: "Asistencia",
                columns: new[] { "ParticipanteId", "Fecha" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Asistencia_Act_Part_Fecha",
                schema: "dbo",
                table: "Asistencia",
                columns: new[] { "ActividadId", "ParticipanteId", "Fecha" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigMotorOverride_Programa",
                schema: "dbo",
                table: "ConfiguracionMotorOverride",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionMotorOverride_Clave",
                schema: "dbo",
                table: "ConfiguracionMotorOverride",
                column: "Clave");

            migrationBuilder.CreateIndex(
                name: "IX_DiccionarioObservacionesPrograma_ProgramaId",
                schema: "dbo",
                table: "DiccionarioObservacionesPrograma",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidencia_Actividad",
                schema: "dbo",
                table: "EvidenciaActividad",
                columns: new[] { "ActividadId", "Tipo" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UsuarioActorId",
                schema: "dbo",
                table: "Logs",
                column: "UsuarioActorId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRegla_ActividadId",
                schema: "dbo",
                table: "MatchRegla",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRegla_EjecucionId",
                schema: "dbo",
                table: "MatchRegla",
                column: "EjecucionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRegla_ParticipanteId",
                schema: "dbo",
                table: "MatchRegla",
                column: "ParticipanteId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRegla_ProgramaId",
                schema: "dbo",
                table: "MatchRegla",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRegla_ReglaId",
                schema: "dbo",
                table: "MatchRegla",
                column: "ReglaId");

            migrationBuilder.CreateIndex(
                name: "IX_Participante_PersonaId",
                schema: "dbo",
                table: "Participante",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_POA_Archivo_CampoId",
                schema: "dbo",
                table: "POA_Archivo",
                column: "CampoId");

            migrationBuilder.CreateIndex(
                name: "IX_POA_Archivo_InstanciaId",
                schema: "dbo",
                table: "POA_Archivo",
                column: "InstanciaId");

            migrationBuilder.CreateIndex(
                name: "IX_POA_Campo_SeccionId",
                schema: "dbo",
                table: "POA_Campo",
                column: "SeccionId");

            migrationBuilder.CreateIndex(
                name: "UX_PCampo_Plantilla_Clave",
                schema: "dbo",
                table: "POA_Campo",
                columns: new[] { "PlantillaId", "Clave" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_POA_CampoOpcion_CampoId",
                schema: "dbo",
                table: "POA_CampoOpcion",
                column: "CampoId");

            migrationBuilder.CreateIndex(
                name: "IX_POA_CampoValidacion_CampoId",
                schema: "dbo",
                table: "POA_CampoValidacion",
                column: "CampoId");

            migrationBuilder.CreateIndex(
                name: "IX_PInst_Programa_Periodo",
                schema: "dbo",
                table: "POA_Instancia",
                columns: new[] { "ProgramaId", "PeriodoAnio", "PeriodoMes" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_POA_Instancia_PlantillaId",
                schema: "dbo",
                table: "POA_Instancia",
                column: "PlantillaId");

            migrationBuilder.CreateIndex(
                name: "UX_Plantilla_Programa_Version",
                schema: "dbo",
                table: "POA_Plantilla",
                columns: new[] { "ProgramaId", "Version" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_POA_PlantillaSeccion_PlantillaId",
                schema: "dbo",
                table: "POA_PlantillaSeccion",
                column: "PlantillaId");

            migrationBuilder.CreateIndex(
                name: "IX_POA_Valor_CampoId",
                schema: "dbo",
                table: "POA_Valor",
                column: "CampoId");

            migrationBuilder.CreateIndex(
                name: "IX_PValor_Actividad",
                schema: "dbo",
                table: "POA_Valor",
                column: "ActividadId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PValor_Participante",
                schema: "dbo",
                table: "POA_Valor",
                column: "ParticipanteId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PValor_Programa",
                schema: "dbo",
                table: "POA_Valor",
                column: "ProgramaId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PValor_Target_Inst",
                schema: "dbo",
                table: "POA_Valor",
                columns: new[] { "InstanciaId", "CampoId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Programa_Clave_Unique",
                schema: "dbo",
                table: "Programa",
                column: "Clave",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Regla_Clave_Unique",
                schema: "dbo",
                table: "Regla",
                column: "Clave",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_ReglaParam_Regla_Nombre",
                schema: "dbo",
                table: "ReglaParametro",
                columns: new[] { "ReglaId", "Nombre" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ReglaParametroOverride_ProgramaId",
                schema: "dbo",
                table: "ReglaParametroOverride",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoDetalle_RiesgoId",
                schema: "dbo",
                table: "RiesgoDetalle",
                column: "RiesgoId");

            migrationBuilder.CreateIndex(
                name: "IX_Riesgo_Part_Prog_Fecha",
                schema: "dbo",
                table: "RiesgoParticipantePrograma",
                columns: new[] { "ParticipanteId", "ProgramaId", "FechaCorte" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RiesgoParticipantePrograma_ProgramaId",
                schema: "dbo",
                table: "RiesgoParticipantePrograma",
                column: "ProgramaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rol_Nombre_Unique",
                schema: "dbo",
                table: "Rol",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "dbo",
                table: "Rol",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RolClaim_RoleId",
                schema: "dbo",
                table: "RolClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "dbo",
                table: "Usuario",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email_Unique",
                schema: "dbo",
                table: "Usuario",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_PersonaId",
                schema: "dbo",
                table: "Usuario",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "dbo",
                table: "Usuario",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioClaim_UserId",
                schema: "dbo",
                table: "UsuarioClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioLogin_UserId",
                schema: "dbo",
                table: "UsuarioLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPrograma_Programa",
                schema: "dbo",
                table: "UsuarioPrograma",
                column: "ProgramaId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPrograma_Usuario",
                schema: "dbo",
                table: "UsuarioPrograma",
                column: "UsuarioId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRol_RoleId",
                schema: "dbo",
                table: "UsuarioRol",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActividadParticipante",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Alerta",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Asistencia",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ConfiguracionMotorOverride",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiccionarioObservacionesPrograma",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EvidenciaActividad",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Logs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MatchRegla",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MetricasProgramaMes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_Archivo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_CampoOpcion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_CampoValidacion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_SnapshotMensual",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_Valor",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReglaParametro",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReglaParametroOverride",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RiesgoDetalle",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RolClaim",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsuarioClaim",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsuarioLogin",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsuarioPrograma",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsuarioRol",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsuarioToken",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ConfiguracionMotor",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DiccionarioObservaciones",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EjecucionMotor",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Actividad",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_Campo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_Instancia",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Regla",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RiesgoParticipantePrograma",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Rol",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Usuario",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_PlantillaSeccion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Participante",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "POA_Plantilla",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Persona",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Programa",
                schema: "dbo");
        }
    }
}
