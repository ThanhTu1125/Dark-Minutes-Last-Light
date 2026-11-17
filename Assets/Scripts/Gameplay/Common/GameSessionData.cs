using UnityEngine;

public enum CharacterType
{
    Male = 0,   // Aiden (Chiến binh nam)
    Female = 1  // Elena (Nữ điệp viên)
}

public enum WeaponType
{
    Blade = 0,    // Thanh Kiếm Ánh Sáng (Eclipse Blade) - Cận chiến nhanh, cân bằng
    Axe = 1,      // Rìu Huyết Thần (Crimson Cleaver) - Trọng kích, càn quét đám đông
    Staff = 2,    // Trượng Ma Năng (Void Scepter) - Phép thuật tầm xa, tự tìm mục tiêu

    // Tương thích ngược với code cũ
    Pistol = 0,
    Shotgun = 1,
    ArcRifle = 2
}

public static class GameSessionData
{
    public static CharacterType SelectedCharacter = CharacterType.Male;
    public static WeaponType SelectedWeapon = WeaponType.Blade;

    // Thống kê trận đấu
    public static int EnemiesKilled = 0;
    public static float SurvivalTime = 0f;

    public static void ResetStats()
    {
        EnemiesKilled = 0;
        SurvivalTime = 0f;
    }
}
