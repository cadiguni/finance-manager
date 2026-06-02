using FinTrack.Application.Common;

namespace FinTrack.Application.Installments;

public interface IInstallmentService
{
    Task<Result<InstallmentGroupDto>> CreatePurchaseAsync(
        Guid userId,
        CreateInstallmentPurchaseRequest request,
        CancellationToken cancellationToken);
}
