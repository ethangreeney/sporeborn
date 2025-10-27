public static class MenuManager
{
    private static bool isAnyMenuOpen = false;
    public static bool TryOpenMenu() => isAnyMenuOpen ? false : isAnyMenuOpen = true;
    public static void CloseMenu() => isAnyMenuOpen = false;
}

