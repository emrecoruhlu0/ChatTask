using ChatTask.Shared.Enums;

namespace ChatTask.Shared.Helpers;

public static class MemberIdHelper
{
    // Member ID oluşturma
    public static Guid CreateMemberId(Guid userId, Guid parentId, MemberRole role)
    {
        var userIdBytes = userId.ToByteArray();
        var parentIdBytes = parentId.ToByteArray();
        
        // Role'ü son byte'a ekle (0-255 arası değer)
        var combinedBytes = new byte[16];
        for (int i = 0; i < 15; i++)
        {
            combinedBytes[i] = (byte)(userIdBytes[i] ^ parentIdBytes[i]);
        }
        
        // Son byte = Role (0-255 arası)
        combinedBytes[15] = (byte)role;
        
        return new Guid(combinedBytes);
    }
    
    // Role çıkarma (çok basit)
    public static MemberRole ExtractRole(Guid memberId)
    {
        var memberBytes = memberId.ToByteArray();
        return (MemberRole)memberBytes[15]; // Son byte
    }
    
    // UserId çıkarma
    public static Guid ExtractUserId(Guid memberId)
    {
        var memberBytes = memberId.ToByteArray();
        var parentId = ExtractParentId(memberId);
        var parentBytes = parentId.ToByteArray();
        
        var userIdBytes = new byte[16];
        for (int i = 0; i < 15; i++)
        {
            userIdBytes[i] = (byte)(memberBytes[i] ^ parentBytes[i]);
        }
        userIdBytes[15] = 0; // Role byte'ını sıfırla
        
        return new Guid(userIdBytes);
    }
    
    // ParentId çıkarma
    public static Guid ExtractParentId(Guid memberId)
    {
        var memberBytes = memberId.ToByteArray();
        var parentBytes = new byte[16];
        
        // İlk 15 byte'ı kopyala
        Array.Copy(memberBytes, 0, parentBytes, 0, 15);
        parentBytes[15] = 0; // Son byte'ı sıfırla
        
        return new Guid(parentBytes);
    }
    
    // Entity tipini belirleme - KALDIRILDI (GUID'den entity tipi belirlenemez)
}
