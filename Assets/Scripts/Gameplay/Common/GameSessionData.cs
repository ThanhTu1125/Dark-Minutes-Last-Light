using UnityEngine;

public enum CharacterType
{
    Male,   // Aiden
    Female  // Elena
}

public enum WeaponType
{
    Pistol,    // Súng ngắn cân bằng (Damage 10, 1 đạn)
    Shotgun,   // Súng ghém tỏa chùm (Damage 8 x 5 viên)
    ArcRifle   // Súng trường năng lượng (Damage 20, tầm xa)
}

public static class GameSessionData
{
    public static CharacterType SelectedCharacter = CharacterType.Male;
    public static WeaponType SelectedWeapon = WeaponType.Pistol;
}
