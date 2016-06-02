using Microsoft.AspNet.Identity.EntityFramework;

namespace RestApi.Entities
{
    public class User : IdentityUser<int, UserLogin, UserRole, UserClaim>
    {
    }

    public class UserClaim : IdentityUserClaim<int>
    {
    }

    public class UserRole : IdentityUserRole<int>
    {
    }

    public class UserLogin : IdentityUserLogin<int>
    {
    }
}
