namespace ChatTask.ChatService.Models;

public class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Domain { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // ÜYELİK YÖNETİMİ - Tek liste
    public List<Member> Members { get; set; } = new();
    
    // TEK LİSTE - Tüm conversation türleri
    public List<Conversation> Conversations { get; set; } = new();
    
    // HELPER PROPERTIES - Filtrelenmiş erişim için
    public IEnumerable<Channel> Channels => 
        Conversations.OfType<Channel>();
    
    public IEnumerable<Group> Groups => 
        Conversations.OfType<Group>();
    
    public IEnumerable<DirectMessage> DirectMessages => 
        Conversations.OfType<DirectMessage>();
    
    public IEnumerable<TaskGroup> TaskGroups => 
        Conversations.OfType<TaskGroup>();
}
