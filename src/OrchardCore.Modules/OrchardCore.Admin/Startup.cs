using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Controllers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;
        private readonly IShellConfiguration _configuration;

        public Startup(IOptions<AdminOptions> adminOptions, IShellConfiguration configuration)
        {
            _adminOptions = adminOptions.Value;
            _configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddNavigation();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(AdminFilter));
                options.Filters.Add(typeof(AdminMenuFilter));
                options.Conventions.Add(new AdminActionModelConvention());

                // Ordered to be called before any global filter.
                options.Filters.Add(typeof(AdminZoneFilter), -1000);
            });

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IThemeSelector, AdminThemeSelector>();
            services.AddScoped<IAdminThemeService, AdminThemeService>();
            services.Configure<AdminOptions>(_configuration.GetSection("OrchardCore.Admin"));

            services.AddSingleton<IPageRouteModelProvider, AdminPageRouteModelProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaControllerRoute(
                name: "Admin",
                areaName: "OrchardCore.Admin",
                pattern: _adminOptions.AdminUrlPrefix,
                defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Index) }
            );
        }
    }

    public class AdminPagesStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public AdminPagesStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override int Order => 1000;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorPagesOptions>((options) =>
            {
                options.Conventions.Add(new AdminPageRouteModelConvention(_adminOptions.AdminUrlPrefix));
            });
        }
    }
}
