namespace GameStore.Web.Models.Interfaces
{
    public interface IEntityWithDates
    {
        DateTime CreateDate { get; set; }
        DateTime UpdateDate { get; set; }
    }
}
