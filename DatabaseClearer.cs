using Microsoft.EntityFrameworkCore;
using ChatTask.ChatService.Context;
using ChatTask.UserService.Context;
using ChatTask.TaskService.Context;

namespace ChatTask.DatabaseClearer
{
    /// <summary>
    /// Veritabanı tablolarındaki verileri temizlemek için yardımcı sınıf
    /// </summary>
    public class DatabaseClearer
    {
        private readonly ChatDbContext _chatContext;
        private readonly UserDbContext _userContext;
        private readonly TaskDbContext _taskContext;

        public DatabaseClearer(
            ChatDbContext chatContext,
            UserDbContext userContext,
            TaskDbContext taskContext)
        {
            _chatContext = chatContext;
            _userContext = userContext;
            _taskContext = taskContext;
        }

        /// <summary>
        /// Tüm veritabanlarındaki verileri temizler
        /// </summary>
        public async Task ClearAllDatabasesAsync()
        {
            Console.WriteLine("Veritabanları temizleniyor...");

            await ClearChatDatabaseAsync();
            await ClearUserDatabaseAsync();
            await ClearTaskDatabaseAsync();

            Console.WriteLine("Tüm veritabanları başarıyla temizlendi!");
        }

        /// <summary>
        /// Chat veritabanındaki verileri temizler
        /// </summary>
        public async Task ClearChatDatabaseAsync()
        {
            try
            {
                Console.WriteLine("Chat veritabanı temizleniyor...");

                // Foreign key constraint'leri geçici olarak devre dışı bırak
                await _chatContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'");

                // Tüm tablolardaki verileri temizle
                await _chatContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'DELETE FROM ?'");

                // Foreign key constraint'leri tekrar aktif et
                await _chatContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'");

                // Identity sütunlarını sıfırla
                await _chatContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'DBCC CHECKIDENT (''?'', RESEED, 0)'");

                Console.WriteLine("Chat veritabanı temizlendi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chat veritabanı temizlenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// User veritabanındaki verileri temizler
        /// </summary>
        public async Task ClearUserDatabaseAsync()
        {
            try
            {
                Console.WriteLine("User veritabanı temizleniyor...");

                // Foreign key constraint'leri geçici olarak devre dışı bırak
                await _userContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'");

                // Tüm tablolardaki verileri temizle
                await _userContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'DELETE FROM ?'");

                // Foreign key constraint'leri tekrar aktif et
                await _userContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'");

                // Identity sütunlarını sıfırla
                await _userContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'DBCC CHECKIDENT (''?'', RESEED, 0)'");

                Console.WriteLine("User veritabanı temizlendi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User veritabanı temizlenirken hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Task veritabanındaki verileri temizler
        /// </summary>
        public async Task ClearTaskDatabaseAsync()
        {
            try
            {
                Console.WriteLine("Task veritabanı temizleniyor...");

                // Foreign key constraint'leri geçici olarak devre dışı bırak
                await _taskContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'");

                // Tüm tablolardaki verileri temizle
                await _taskContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'DELETE FROM ?'");

                // Foreign key constraint'leri tekrar aktif et
                await _taskContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'");

                // Identity sütunlarını sıfırla
                await _taskContext.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'DBCC CHECKIDENT (''?'', RESEED, 0)'");

                Console.WriteLine("Task veritabanı temizlendi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task veritabanı temizlenirken hata: {ex.Message}");
            }
        }
    }
}
