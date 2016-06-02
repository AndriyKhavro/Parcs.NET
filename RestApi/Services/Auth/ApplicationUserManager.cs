using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RestApi.Entities;

namespace RestApi.Services.Auth
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<User, int>
    {
        // Configure the application user manager
        public ApplicationUserManager(IUserStore<User, int> store, IdentityFactoryOptions<ApplicationUserManager> options)
            : base(store)
        {
            Initialize(options);
        }

        public void Initialize(IdentityFactoryOptions<ApplicationUserManager> options)
        {
            UserValidator = new UserValidator<User, int>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = false
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                UserTokenProvider = new DataProtectorTokenProvider<User, int>(dataProtectionProvider.Create("PasswordReset"));
            }
        }
    }
}
