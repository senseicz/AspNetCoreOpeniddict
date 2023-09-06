using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpeniddictServer.Migrations
{
    /// <inheritdoc />
    public partial class Fido2StoredCredentialsPasskeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "AttestationClientDataJson",
                table: "FidoStoredCredential",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AttestationObject",
                table: "FidoStoredCredential",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BE",
                table: "FidoStoredCredential",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BS",
                table: "FidoStoredCredential",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "CredentialId",
                table: "FidoStoredCredential",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevicePublicKeysJson",
                table: "FidoStoredCredential",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportsJson",
                table: "FidoStoredCredential",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "FidoStoredCredential",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPasskey",
                table: "FidoStoredCredential",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.RenameColumn(
                name: "SignatureCounter",
                table: "FidoStoredCredential",
                newName: "SignCount");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttestationClientDataJson",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "AttestationObject",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "BE",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "BS",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "CredentialId",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "DevicePublicKeysJson",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "TransportsJson",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "FidoStoredCredential");

            migrationBuilder.DropColumn(
                name: "IsPasskey",
                table: "FidoStoredCredential");

            migrationBuilder.RenameColumn(
                name: "SignCount",
                table: "FidoStoredCredential",
                newName: "SignatureCounter");
        }
    }
}
