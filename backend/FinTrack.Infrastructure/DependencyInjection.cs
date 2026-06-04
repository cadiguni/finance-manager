using FinTrack.Application.Accounts;
using FinTrack.Application.Categorization;
using FinTrack.Application.Categories;
using FinTrack.Application.Dashboard;
using FinTrack.Application.Imports;
using FinTrack.Application.Installments;
using FinTrack.Application.Recurring;
using FinTrack.Application.Transactions;
using FinTrack.Infrastructure.Accounts;
using FinTrack.Infrastructure.Categorization;
using FinTrack.Infrastructure.Categories;
using FinTrack.Infrastructure.Dashboard;
using FinTrack.Infrastructure.Imports;
using FinTrack.Infrastructure.Installments;
using FinTrack.Infrastructure.Persistence;
using FinTrack.Infrastructure.Recurring;
using FinTrack.Infrastructure.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FinTrackDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryKeywordRuleRepository, CategoryKeywordRuleRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        services.AddScoped<IInstallmentRepository, InstallmentRepository>();
        services.AddScoped<IRecurringRuleRepository, RecurringRuleRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
