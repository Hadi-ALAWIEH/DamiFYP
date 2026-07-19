using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DamiFYP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DamiUser",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    KeyCloakId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamiUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloodTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    BloodTypeName = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodTypes_DamiUser_UserId",
                        column: x => x.UserId,
                        principalTable: "DamiUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonationPosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    BloodTypeName = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    DamiUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationPosts_DamiUser_DamiUserId",
                        column: x => x.DamiUserId,
                        principalTable: "DamiUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonationRequest",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    BloodTypeName = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Urgency = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    NeededByDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonationRequest_DamiUser_UserId",
                        column: x => x.UserId,
                        principalTable: "DamiUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DonationRequestId = table.Column<long>(type: "bigint", nullable: false),
                    DonationPostId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: true),
                    DistanceKm = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Match_DonationPosts_DonationPostId",
                        column: x => x.DonationPostId,
                        principalTable: "DonationPosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Match_DonationRequest_DonationRequestId",
                        column: x => x.DonationRequestId,
                        principalTable: "DonationRequest",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ConversationId = table.Column<long>(type: "bigint", nullable: false),
                    DamiUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_DamiUser_DamiUserId",
                        column: x => x.DamiUserId,
                        principalTable: "DamiUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConversationParticipantId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_ConversationParticipants_ConversationParticipantId",
                        column: x => x.ConversationParticipantId,
                        principalTable: "ConversationParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodTypes_UserId",
                table: "BloodTypes",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_ConversationId",
                table: "ConversationParticipants",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_DamiUserId",
                table: "ConversationParticipants",
                column: "DamiUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_MatchId",
                table: "Conversations",
                column: "MatchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DamiUser_Email",
                table: "DamiUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonationPosts_DamiUserId",
                table: "DonationPosts",
                column: "DamiUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationRequest_UserId",
                table: "DonationRequest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_DonationPostId",
                table: "Match",
                column: "DonationPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_DonationRequestId",
                table: "Match",
                column: "DonationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_ConversationParticipantId",
                table: "Message",
                column: "ConversationParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloodTypes");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "DonationPosts");

            migrationBuilder.DropTable(
                name: "DonationRequest");

            migrationBuilder.DropTable(
                name: "DamiUser");
        }
    }
}
