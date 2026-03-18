using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Localization;
using TOEIC.Authorization;

namespace TOEIC.Web.Startup;

public class TOEICNavigationProvider : NavigationProvider
{
    public override void SetNavigation(INavigationProviderContext context)
    {
        context.Manager.MainMenu
            .AddItem(
                new MenuItemDefinition(
                    PageNames.Home,
                    new FixedLocalizableString("Trang de thi"),
                    url: "Test",
                    icon: "fas fa-home",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Test)
                )
            )
            .AddItem(
                new MenuItemDefinition(
                    PageNames.Tenants,
                    L("Tenants"),
                    url: "Tenants",
                    icon: "fas fa-building",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Tenants)
                )
            )
            .AddItem(
                new MenuItemDefinition(
                    PageNames.Users,
                    L("Users"),
                    url: "Users",
                    icon: "fas fa-users",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Users)
                )
            )
            .AddItem(
                new MenuItemDefinition(
                    PageNames.Roles,
                    L("Roles"),
                    url: "Roles",
                    icon: "fas fa-theater-masks",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Roles)
                )
            )
            .AddItem(
                new MenuItemDefinition(
                    PageNames.Exams,
                    new FixedLocalizableString("Quan ly de thi"),
                    url: "Exams",
                    icon: "fas fa-file-alt",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Exams)
                )
            )
            .AddItem(
                new MenuItemDefinition(
                    PageNames.Test,
                    new FixedLocalizableString("Thi TOEIC"),
                    url: "Test",
                    icon: "fas fa-pen-fancy",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Test)
                )
            )
            .AddItem(
                new MenuItemDefinition(
                    PageNames.TestHistory,
                    new FixedLocalizableString("Lich su thi"),
                    url: "Test/History",
                    icon: "fas fa-history",
                    permissionDependency: new SimplePermissionDependency(PermissionNames.Pages_Test_History)
                )
            );
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, TOEICConsts.LocalizationSourceName);
    }
}
