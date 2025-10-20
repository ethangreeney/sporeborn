using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using UnityEngine;


[TestFixture]
public class ShopTestsTDD
{
    private ShopModel TestShop;
    private PlayerPresenter playerPresenter;
    private GameObject TestPlayer;
    private ShopItem TestItem;
    private CurrencyModel PlayerCurrency;

    // Setup the test environment for testing Shop
    [SetUp]
    public void Setup()
    {
        // Shop
        GameObject ShopObject = new GameObject();
        TestShop = ShopObject.AddComponent<ShopModel>();

        // Player
        TestPlayer = new GameObject();
        playerPresenter = TestPlayer.AddComponent<PlayerPresenter>();
        PlayerCurrency = TestPlayer.AddComponent<CurrencyModel>();
        TestPlayer.AddComponent<PlayerActivatableItem>();

        // Test Activatable Item
        ActivatableItem testActivatableItem = ScriptableObject.CreateInstance<ActivatableItem>();
        
        TestItem = new ShopItem(testActivatableItem, "Test Item", 10, null);

        // Add test item to shop inventory and pool
        TestShop.AddTestItem(TestItem);

    }


    [Test]
    public void PlayerBuysItem_EnoughMoney()
    {
        TestContext.WriteLine("Running test: PlayerBuysItem_EnoughMoney");

        // Player has enough money to purchase item
        PlayerCurrency.AddCurrency(10);

        // Tries to purchase item passing through a test item and Player
        TestShop.TryPurchaseItem(TestItem, playerPresenter, null);

        Assert.AreEqual(0, PlayerCurrency.GetCurrentNectar(), "Player's currency should be deducted by the correct amount");

        // Player should have Item in their inventory
        PlayerActivatableItem PlayerItem = playerPresenter.GetComponent<PlayerActivatableItem>();

        // Passes if Player item is not null && has been added to their equipped item slot
        bool HasItem = PlayerItem.equippedItem != null && PlayerItem.equippedItem == TestItem.ItemType;
        Assert.IsTrue(HasItem, "Checking that item has been added to the player's Equipped Item Slot");
    }

    [Test]
    public void PlayerBuysItem_NotEnoughMoney()
    {
        TestContext.WriteLine("Running test: PlayerBuysItem_NotEnoughMoney");

        // Player doesn't have enough money they shouldn't get that item
        PlayerCurrency.AddCurrency(2);

        // Tries to purchase item passing through a test item and Player
        TestShop.TryPurchaseItem(TestItem, playerPresenter, null);

        Assert.AreEqual(2, PlayerCurrency.GetCurrentNectar(), "Player's currency shouldn't be deducted, insufficient funds to purchase item");

        // Player should not have Item in their inventory
        PlayerActivatableItem PlayerItem = playerPresenter.GetComponent<PlayerActivatableItem>();

        // Passes if Player item is null - meaning nothing has been equipped
        bool HasItem = PlayerItem.equippedItem == null;
        Assert.IsTrue(HasItem, "Checking that item has not been added to the player's Equipped Item Slot");

    }

    [Test]
    public void ItemHasBeenPurchased()
    {
        TestContext.WriteLine("Running test: ItemHasBeenPurchased");

        // Add currency
        PlayerCurrency.AddCurrency(10);

        // Purchase an item
        TestShop.TryPurchaseItem(TestItem, playerPresenter, null);

        // After purchase complete Shop's Inventory has removed that item
        bool HasItem = TestItem.Purchased;
        Assert.IsTrue(HasItem, "Checking that item has been marked as purchased");
    }


    [Test]
    public void ItemRemovedFromShopsItemPool()
    {
        TestContext.WriteLine("Running test: ItemRemovedFromShopsItemPool");

        // Tries to purchase item
        PlayerCurrency.AddCurrency(10);
        
        // Purchase an item
        TestShop.TryPurchaseItem(TestItem, playerPresenter, null);

        // After purchase complete Shop Item Pool has removed that item for future levels
        bool HasItem = TestShop.ShopPool.Contains(TestItem);
        Assert.IsFalse(HasItem, "Checking that item has been removed from the Shop Item Pool");

    }

}
