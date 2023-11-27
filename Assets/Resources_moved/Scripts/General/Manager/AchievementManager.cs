using Steamworks;

static class AchievementManager
{
	public static void CompleteRun(string religion)
	{
		if (!SteamManager.Initialized)
			return;
		
		switch (religion)
		{
			case "Agbara":
				SteamUserStats.GetAchievement("RUN_WON_AGBARA", out bool isAgbaraRunCompleted);
				if (isAgbaraRunCompleted)
					return;
				SteamUserStats.SetAchievement("RUN_WON_AGBARA");
				SteamUserStats.StoreStats();
				break;
			default:
				break;
		}
	}
}
