using System.Runtime.Serialization;

namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    [EnumMember(Value = "Shipped")]
    Shipped,

    [EnumMember(Value = "Delivered")]
    Delivered
}
