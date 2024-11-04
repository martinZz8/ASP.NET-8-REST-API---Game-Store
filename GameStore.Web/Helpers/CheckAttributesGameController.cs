namespace GameStore.Web.Helpers
{
    public static class CheckAttributesGameController
    {
        public static readonly string[] AvailableOrderByNames = new string[] { "Name", "Price", "CreateDate" };

        public static bool CheckOrderBy(string? orderByName, bool passNull = false)
        {
            if (passNull && orderByName == null)
                return true;

            return CheckValidityBasedOnArray(orderByName, AvailableOrderByNames);
        }

        private static bool CheckValidityBasedOnArray(string nameToCheck, IEnumerable<string> availableNames, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            foreach (string availableName in availableNames)
            {
                if (availableName.Equals(nameToCheck, stringComparison))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
