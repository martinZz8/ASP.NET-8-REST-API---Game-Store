using GameStore.Web.Consts;
using System.Security.Claims;
using System.Security.Principal;

namespace GameStore.Web.Helpers
{
    // from: https://stackoverflow.com/questions/45315274/get-claims-from-a-webapi-controller-jwt-token
    public static class UserJWTHelper
    {
        public static string? GetUserId(HttpContext httpContext)
        {
            return GetFirstClaimValue(httpContext, JWTUserClaimsConsts.NAME_IDENTIFIER);
        }

        // Note: Use const value from "JWTUserClaimsConsts" as "claimName" parameter
        public static string? GetFirstClaimValue(HttpContext httpContext, string claimName)
        {
            // Note: We could also get "Claims" enumerable from "httpContext.User.Claims"
            ClaimsIdentity claimsIdentity = httpContext.User.Identity as ClaimsIdentity;
            Claim claim = claimsIdentity.Claims.FirstOrDefault(it => it.Type.Equals(claimName));

            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }

        public static string[] GetAllClaimValues(HttpContext httpContext, string claimName)
        {
            List<string> claimValues = new List<string>();
            ClaimsIdentity claimsIdentity = httpContext.User.Identity as ClaimsIdentity;

            foreach (Claim claim in claimsIdentity.Claims.Where(it => it.Type.Equals(claimName)))
            {
                claimValues.Add(claim.Value);
            }            

            return claimValues.ToArray();
        }
    }
}
