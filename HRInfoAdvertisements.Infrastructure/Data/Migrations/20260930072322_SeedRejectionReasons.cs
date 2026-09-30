using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRInfoAdvertisements.Infrastructure.Data.Migrations
{
    public partial class SeedRejectionReasons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RejectionReasons",
                columns: new[]
                {
                    "RejectionReasonID",
                    "ReasonCode",
                    "ReasonText",
                    "ReasonTextAr",
                    "IsActive",
                    "DisplayOrder"
                },
                values: new object[,]
                {
                    {
                        1,
                        "INCOMPLETE_INFORMATION",
                        "Incomplete advertisement information",
                        "معلومات الإعلان غير مكتملة",
                        true,
                        1
                    },
                    {
                        2,
                        "INCORRECT_INFORMATION",
                        "Incorrect or misleading information",
                        "معلومات غير صحيحة أو مضللة",
                        true,
                        2
                    },
                    {
                        3,
                        "DUPLICATE_ADVERTISEMENT",
                        "Duplicate advertisement",
                        "إعلان مكرر",
                        true,
                        3
                    },
                    {
                        4,
                        "INAPPROPRIATE_CONTENT",
                        "Inappropriate content",
                        "محتوى غير مناسب",
                        true,
                        4
                    },
                    {
                        5,
                        "PROHIBITED_CONTENT",
                        "Prohibited content",
                        "محتوى محظور",
                        true,
                        5
                    },
                    {
                        6,
                        "INVALID_CONTACT",
                        "Invalid or unreachable contact information",
                        "معلومات اتصال غير صحيحة أو غير متاحة",
                        true,
                        6
                    },
                    {
                        7,
                        "INVALID_PRICE",
                        "Invalid or misleading price information",
                        "معلومات السعر غير صحيحة أو مضللة",
                        true,
                        7
                    },
                    {
                        8,
                        "LOCATION_MISMATCH",
                        "Advertisement location does not match the submitted information",
                        "موقع الإعلان لا يتطابق مع المعلومات المقدمة",
                        true,
                        8
                    },
                    {
                        9,
                        "INSUFFICIENT_DOCUMENTATION",
                        "Required supporting information or documentation is missing",
                        "المعلومات أو المستندات المطلوبة غير متوفرة",
                        true,
                        9
                    },
                    {
                        10,
                        "OTHER",
                        "Other reason",
                        "سبب آخر",
                        true,
                        10
                    }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RejectionReasons",
                keyColumn: "RejectionReasonID",
                keyValues: new object[]
                {
                    1,
                    2,
                    3,
                    4,
                    5,
                    6,
                    7,
                    8,
                    9,
                    10
                });
        }
    }
}