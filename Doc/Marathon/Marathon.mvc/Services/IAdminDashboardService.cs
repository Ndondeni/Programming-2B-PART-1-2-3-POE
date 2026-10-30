// Services/Dashboard/IAdminDashboardService.cs
using Marathon.mvc.Models.dto;

namespace Marathon.mvc.Services { 

public interface IAdminDashboardService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
}