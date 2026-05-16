using EventBookingSystem.Areas.Admin.ViewModels;
using EventBookingSystem.Models;
using EventBookingSystem.Repositories.Interfaces;
using EventBookingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystem.Services
{
    public class DashboardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : IDashboardService
    {
        public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
        {
            var events = (await unitOfWork.Events.GetAllAsync(ct)).ToList();
            var bookings = (await unitOfWork.Bookings.GetAllAsync(ct)).ToList();
            var userCount = await userManager.Users.CountAsync(ct);

            return new DashboardViewModel
            {
                Stats =
                [
                    new DashboardStatCardViewModel
                    {
                        Title = "Total Events",
                        Value = events.Count.ToString(),
                        Description = $"{events.Count(e => !e.IsCancelled)} active"
                    },
                    new DashboardStatCardViewModel
                    {
                        Title = "Total Users",
                        Value = userCount.ToString(),
                        Description = "Registered accounts"
                    },
                    new DashboardStatCardViewModel
                    {
                        Title = "Total Bookings",
                        Value = bookings.Count.ToString(),
                        Description = "All-time bookings"
                    },
                    new DashboardStatCardViewModel
                    {
                        Title = "Booking Value",
                        Value = bookings.Sum(b => b.TotalPrice).ToString("C"),
                        Description = "Sum of booking totals"
                    }
                ],
                QuickActions =
                [
                    new DashboardQuickActionViewModel
                    {
                        Label = "Create Event",
                        Controller = "Events",
                        Action = "Create",
                        CssClass = "btn btn-primary"
                    },
                    new DashboardQuickActionViewModel
                    {
                        Label = "View Bookings",
                        Controller = "Bookings",
                        Action = "Index"
                    },
                    new DashboardQuickActionViewModel
                    {
                        Label = "Manage Users",
                        Controller = "Users",
                        Action = "Index"
                    }
                ]
            };
        }
    }
}
