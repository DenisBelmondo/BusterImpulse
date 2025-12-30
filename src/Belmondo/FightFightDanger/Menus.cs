namespace Belmondo.FightFightDanger;

public static class Menus
{
    public enum ID
    {
        MainMenu,
        SnacksMenu,
        CharmsMenu,
    }

    public enum Item
    {
        Snacks,
        Charms,
        Quit,
    };

    public static void InitializeMainMenu(Menu menu)
    {
        menu.ID = (int)ID.MainMenu;

        menu.Items = [
            new()
            {
                ID = (int)Item.Snacks,
                Label = "Snacks",
            },

            new()
            {
                ID = (int)Item.Charms,
                Label = "Charms",
            },

            new()
            {
                ID = (int)Item.Quit,
                Label = "Quit",
            },
        ];
    }

    public static void InitializeSnacksMenu(Menu menu, in Inventory inventory)
    {
        menu.ID = (int)ID.SnacksMenu;
        menu.Items.Clear();

        foreach ((var snackType, var quantity) in inventory.Snacks)
        {
            menu.Items.Add(new()
            {
                ID = (int)snackType,
                Label = Names.Get(snackType),
                Quantity = quantity,
            });
        }
    }
}
