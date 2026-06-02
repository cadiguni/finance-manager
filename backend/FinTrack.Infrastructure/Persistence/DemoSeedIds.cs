namespace FinTrack.Infrastructure.Persistence;

internal static class DemoSeedIds
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid MainAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid CashAccountId = Guid.Parse("22222222-2222-2222-2222-222222222223");

    public static readonly Guid SalaryCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333331");
    public static readonly Guid FoodCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333332");
    public static readonly Guid HousingCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid TransportCategoryId = Guid.Parse("33333333-3333-3333-3333-333333333334");

    public static readonly Guid SalaryTransactionId = Guid.Parse("44444444-4444-4444-4444-444444444441");
    public static readonly Guid GroceriesTransactionId = Guid.Parse("44444444-4444-4444-4444-444444444442");
    public static readonly Guid RentTransactionId = Guid.Parse("44444444-4444-4444-4444-444444444443");
    public static readonly Guid TransportTransactionId = Guid.Parse("44444444-4444-4444-4444-444444444444");
}
