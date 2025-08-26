using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Interfaces;
using NotificationService.Domain.Models;

namespace NotificationService.Infrastructure.Repositories;

public class UserNotificationsRepository : IUserNotificationRepository
{
    private readonly NotificationServiceDbContext _context;

    public UserNotificationsRepository(NotificationServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserNotifications>> GetUserNotificationsAsync(int userId)
    {
        return await _context.UserNotifications
            .Include(un => un.Notifications)
            .Where(un => un.UserId == userId)
            .OrderByDescending(un => un.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadNotificationsCountAsync(int userId)
    {
        return await _context.UserNotifications
            .Where(un => un.UserId == userId && !un.IsRead)
            .CountAsync();
    }

    public async Task<UserNotifications?> GetNotificationByUserIdAsync(int notificationId, int userId)
    {
        return await _context.UserNotifications.Where(n => n.NotificationId == notificationId && n.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task UpdateUserNotificationAsync(UserNotifications userNotification)
    {
        _context.UserNotifications.Update(userNotification);
        await _context.SaveChangesAsync();
    }
}
