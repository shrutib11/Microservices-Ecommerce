using System.Runtime.Serialization;

namespace ProductService.Domain.Enums;

public enum MediaType
{
    [EnumMember(Value = "Image")]
    Image,

    [EnumMember(Value = "Video")]
    Video
}
