using FinTrack.Application.Categories;
using FinTrack.Application.Accounts;
using FinTrack.Application.Dashboard;
using FinTrack.Application.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
