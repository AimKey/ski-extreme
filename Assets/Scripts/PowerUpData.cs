[System.Serializable]
public class PowerUpData
{
    public string powerUpName;
    public int currentLevel = 0;
    public int maxLevel = 3;
    public int basePrice = 100;
    public float timePerLevel = 2f;

    public int GetPrice()
    {
        return basePrice * (currentLevel + 1);
    }

    public float GetAddedTime()
    {
        return timePerLevel;
    }
}
