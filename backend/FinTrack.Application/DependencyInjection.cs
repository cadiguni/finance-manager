using FinTrack.Application.Categorization;
using FinTrack.Application.Categories;
using FinTrack.Application.Accounts;
using FinTrack.Application.Dashboard;
using FinTrack.Application.Forecast;
using FinTrack.Application.Imports;
using FinTrack.Application.Installments;
using FinTrack.Application.Recurring;
using FinTrack.Application.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICategoryKeywordRuleService, CategoryKeywordRuleService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IForecastService, ForecastService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IInstallmentService, InstallmentService>();
        services.AddScoped<IRecurringRuleService, RecurringRuleService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
