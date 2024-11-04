namespace GameStore.Web.Enums
{
    // Note: Get name of enum entry either using:
    // - Enum.Name(ErrorTypeEnum.CREATE)
    // - ErrorTypeEnum.CREATE.ToString()
    public enum ErrorTypeEnum
    {
        Undefined = 0,        
        CREATE = 1,
        READ = 2,
        UPDATE = 3,
        DELETE = 4,        
        REGISTER = 5,
        LOGIN = 6
    }
}
