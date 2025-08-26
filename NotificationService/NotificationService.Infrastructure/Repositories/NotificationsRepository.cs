using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationsRepository : INotificationRepository
{
    private readonly NotificationServiceDbContext _context;

    public NotificationsRepository(NotificationServiceDbContext context)
    {
        _context = context;
    }

    public async Task<(Notifications notifications, List<UserNotifications> userNotifications)> AddNotificationsAsync(Notifications notifications, List<UserNotifications> userNotifications)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Notifications.AddAsync(notifications);
            await _context.SaveChangesAsync();

            userNotifications.ForEach(n => n.NotificationId = notifications.Id);

            await _context.UserNotifications.AddRangeAsync(userNotifications);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (notifications, userNotifications);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Notifications?> GetByIdAsync(int notificationId)
    {
        return await _context.Notifications.Where(n => n.Id == notificationId).FirstOrDefaultAsync();
    }
}
