using GameStore.Web.Dtos.Error;

namespace GameStore.Web.Helpers
{
    public static class ErrorExtendedDtoMapper
    {
        public static ErrorDto ToErrorDto(this ErrorExtendedDto errorExtendedDto)
        {
            return new ErrorDto()
            {
                ErrorTypeName = errorExtendedDto.ErrorTypeName,
                Message = errorExtendedDto.Message
            };
        }
    }
}
