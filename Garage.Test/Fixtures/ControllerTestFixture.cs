using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;

namespace Garage.Test.Fixtures;

public class ControllerTestFixture
{
    public Mock<UserManager<IdentityUser>> UserManagerMock { get; }

    public ControllerTestFixture()
    {
        var store = new Mock<IUserStore<IdentityUser>>();

        UserManagerMock = new Mock<UserManager<IdentityUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    public void SetupUser(
        Controller controller,
        string userId,
        bool isAdmin = false)
    {
        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                userId),

            new(
                ClaimTypes.Name,
                "testuser@example.com")
        };

        if (isAdmin)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    "Admin"));
        }

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext = httpContext
            };

        controller.TempData =
            new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());

        UserManagerMock
            .Setup(x => x.GetUserId(
                It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);
    }

    public void SetupNoUser(
        Controller controller)
    {
        var identity =
            new ClaimsIdentity(
                "TestAuthentication");

        var principal =
            new ClaimsPrincipal(identity);

        var httpContext =
            new DefaultHttpContext
            {
                User = principal
            };

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext = httpContext
            };

        controller.TempData =
            new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());

        UserManagerMock
            .Setup(x => x.GetUserId(
                It.IsAny<ClaimsPrincipal>()))
            .Returns((string?)null);
    }
}