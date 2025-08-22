using System.Runtime.Serialization;

namespace NotificationService.Domain.Enums;

public enum NotificationType
{   
    [EnumMember(Value ="Order")]
    Order
}
