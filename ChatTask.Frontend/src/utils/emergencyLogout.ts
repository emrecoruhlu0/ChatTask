/**
 * Acil durum logout fonksiyonu
 * Browser console'dan çalıştırılabilir: window.emergencyLogout()
 */

export const emergencyLogout = () => {
  try {
    // Tüm localStorage'ı temizle
    localStorage.clear();
    
    // SessionStorage'ı da temizle
    sessionStorage.clear();
    
    // Sayfayı yenile
    window.location.reload();
    
    console.log('✅ Emergency logout completed - all data cleared');
  } catch (error) {
    console.error('❌ Emergency logout failed:', error);
  }
};

// Global olarak erişilebilir yap
if (typeof window !== 'undefined') {
  (window as any).emergencyLogout = emergencyLogout;
}
