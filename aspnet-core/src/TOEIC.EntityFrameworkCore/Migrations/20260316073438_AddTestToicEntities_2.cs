using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOEIC.Migrations
{
    /// <inheritdoc />
    public partial class AddTestToicEntities_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppExams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExams_AbpUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AbpUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppExamResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedScore = table.Column<int>(type: "int", nullable: false),
                    TotalCorrect = table.Column<int>(type: "int", nullable: false),
                    TotalWrong = table.Column<int>(type: "int", nullable: false),
                    TotalSkipped = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExamResults_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppExamResults_AppExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "AppExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppPassages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPassages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPassages_AppExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "AppExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<int>(type: "int", nullable: false),
                    QuestionNo = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsShuffle = table.Column<bool>(type: "bit", nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    PassageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppQuestions_AppExams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "AppExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppQuestions_AppPassages_PassageId",
                        column: x => x.PassageId,
                        principalTable: "AppPassages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppStudentAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SelectedOption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    ExamResultId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentAnswers_AppExamResults_ExamResultId",
                        column: x => x.ExamResultId,
                        principalTable: "AppExamResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppStudentAnswers_AppQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "AppQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExamResults_ExamId",
                table: "AppExamResults",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExamResults_UserId",
                table: "AppExamResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppExams_CreatedBy",
                table: "AppExams",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppPassages_ExamId",
                table: "AppPassages",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppQuestions_ExamId",
                table: "AppQuestions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_AppQuestions_PassageId",
                table: "AppQuestions",
                column: "PassageId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentAnswers_ExamResultId",
                table: "AppStudentAnswers",
                column: "ExamResultId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentAnswers_QuestionId",
                table: "AppStudentAnswers",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStudentAnswers");

            migrationBuilder.DropTable(
                name: "AppExamResults");

            migrationBuilder.DropTable(
                name: "AppQuestions");

            migrationBuilder.DropTable(
                name: "AppPassages");

            migrationBuilder.DropTable(
                name: "AppExams");
        }
    }
}
