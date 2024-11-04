using GameStore.Web.Enums;

namespace GameStore.Web.Dtos.Error
{
    public record ErrorDto
    {
        public string Message { get; init; }
        public string ErrorTypeName { get; init; }
    }
}
