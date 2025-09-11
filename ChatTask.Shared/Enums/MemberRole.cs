namespace ChatTask.Shared.Enums;

public enum MemberRole
{
    Owner = 1,      // Sahip
    Admin = 2,      // Yönetici
    Member = 3,     // Normal üye
    Guest = 4,      // Misafir (workspace için)
    Observer = 5    // İzleyici (conversation için)
}

