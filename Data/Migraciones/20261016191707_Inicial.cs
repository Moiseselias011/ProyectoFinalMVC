using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoFinalMVC.Data.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tecnologia",
                columns: table => new
                {
                    IdTecnologia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Dominio = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "programacion")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tecnologia", x => x.IdTecnologia);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HashContrasena = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Usuario"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RutaFoto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "ListaPersonal",
                columns: table => new
                {
                    IdListaPersonal = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueñoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaPersonal", x => x.IdListaPersonal);
                    table.ForeignKey(
                        name: "FK_ListaPersonal_Usuario_DueñoId",
                        column: x => x.DueñoId,
                        principalTable: "Usuario",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tutorial",
                columns: table => new
                {
                    Idtutorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UrlImagen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Idusuario = table.Column<int>(type: "int", nullable: false),
                    TecnologiaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tutorial", x => x.Idtutorial);
                    table.ForeignKey(
                        name: "FK_Tutorial_Tecnologia_TecnologiaId",
                        column: x => x.TecnologiaId,
                        principalTable: "Tecnologia",
                        principalColumn: "IdTecnologia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tutorial_Usuario_Idusuario",
                        column: x => x.Idusuario,
                        principalTable: "Usuario",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comentario",
                columns: table => new
                {
                    Idcomentario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenido = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    idtutorial = table.Column<int>(type: "int", nullable: false),
                    Idusuario = table.Column<int>(type: "int", nullable: false),
                    ComentarioPadreId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentario", x => x.Idcomentario);
                    table.ForeignKey(
                        name: "FK_Comentario_Comentario_ComentarioPadreId",
                        column: x => x.ComentarioPadreId,
                        principalTable: "Comentario",
                        principalColumn: "Idcomentario");
                    table.ForeignKey(
                        name: "FK_Comentario_Tutorial_idtutorial",
                        column: x => x.idtutorial,
                        principalTable: "Tutorial",
                        principalColumn: "Idtutorial",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentario_Usuario_Idusuario",
                        column: x => x.Idusuario,
                        principalTable: "Usuario",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Like",
                columns: table => new
                {
                    IdLike = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TutorialId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Like", x => x.IdLike);
                    table.ForeignKey(
                        name: "FK_Like_Tutorial_TutorialId",
                        column: x => x.TutorialId,
                        principalTable: "Tutorial",
                        principalColumn: "Idtutorial",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Like_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TutorialGuardado",
                columns: table => new
                {
                    IdTutorialGuardado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdListaPersonal = table.Column<int>(type: "int", nullable: false),
                    TutorialId = table.Column<int>(type: "int", nullable: false),
                    FechaGuardado = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorialGuardado", x => x.IdTutorialGuardado);
                    table.ForeignKey(
                        name: "FK_TutorialGuardado_ListaPersonal_IdListaPersonal",
                        column: x => x.IdListaPersonal,
                        principalTable: "ListaPersonal",
                        principalColumn: "IdListaPersonal",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TutorialGuardado_Tutorial_TutorialId",
                        column: x => x.TutorialId,
                        principalTable: "Tutorial",
                        principalColumn: "Idtutorial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LikeDeComentario",
                columns: table => new
                {
                    IdLikeDeComentario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ComentarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeDeComentario", x => x.IdLikeDeComentario);
                    table.ForeignKey(
                        name: "FK_LikeDeComentario_Comentario_ComentarioId",
                        column: x => x.ComentarioId,
                        principalTable: "Comentario",
                        principalColumn: "Idcomentario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikeDeComentario_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_ComentarioPadreId",
                table: "Comentario",
                column: "ComentarioPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_idtutorial",
                table: "Comentario",
                column: "idtutorial");

            migrationBuilder.CreateIndex(
                name: "IX_Comentario_Idusuario",
                table: "Comentario",
                column: "Idusuario");

            migrationBuilder.CreateIndex(
                name: "IX_Like_TutorialId",
                table: "Like",
                column: "TutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_Like_UsuarioId_TutorialId",
                table: "Like",
                columns: new[] { "UsuarioId", "TutorialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LikeDeComentario_ComentarioId",
                table: "LikeDeComentario",
                column: "ComentarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LikeDeComentario_UsuarioId_ComentarioId",
                table: "LikeDeComentario",
                columns: new[] { "UsuarioId", "ComentarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListaPersonal_DueñoId_Nombre",
                table: "ListaPersonal",
                columns: new[] { "DueñoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tecnologia_Nombre",
                table: "Tecnologia",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tecnologia_Slug",
                table: "Tecnologia",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tutorial_FechaPublicacion",
                table: "Tutorial",
                column: "FechaPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_Tutorial_Idusuario",
                table: "Tutorial",
                column: "Idusuario");

            migrationBuilder.CreateIndex(
                name: "IX_Tutorial_TecnologiaId",
                table: "Tutorial",
                column: "TecnologiaId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorialGuardado_IdListaPersonal_TutorialId",
                table: "TutorialGuardado",
                columns: new[] { "IdListaPersonal", "TutorialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorialGuardado_TutorialId",
                table: "TutorialGuardado",
                column: "TutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_NombreUsuario",
                table: "Usuario",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Like");

            migrationBuilder.DropTable(
                name: "LikeDeComentario");

            migrationBuilder.DropTable(
                name: "TutorialGuardado");

            migrationBuilder.DropTable(
                name: "Comentario");

            migrationBuilder.DropTable(
                name: "ListaPersonal");

            migrationBuilder.DropTable(
                name: "Tutorial");

            migrationBuilder.DropTable(
                name: "Tecnologia");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
