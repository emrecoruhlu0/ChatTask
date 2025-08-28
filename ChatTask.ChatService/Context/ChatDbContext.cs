public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }
    
    public DbSet<Chat> Chats { get; set; }
}