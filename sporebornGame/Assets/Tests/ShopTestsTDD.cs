using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        Assert.Fail("Not implemented yet — TDD placeholder");
        // Create a test Shop
        // GameObject ShopObject = new GameObject();
        // TestShop = ShopObject.AddComponent<ShopModel>(); 
        // // Setting up a test player
        // TestPlayer = new GameObject();
        // playerPresenter = TestPlayer.AddComponent<PlayerPresenter>(); 
        // PlayerCurrency = TestPlayer.AddComponent<CurrencyModel>(); 

        // // Create a Test Item
        // TestItem = new ShopItem(null, "Test Item", 10, null);

    }


    [Test]
    public void PlayerBuysItem_EnoughMoney()
    {
        Assert.Fail("Not implemented yet — TDD placeholder");
        // TestContext.WriteLine("Running test: PlayerBuysItem_EnoughMoney");

        // // Player has enough money to purchase item
        // PlayerCurrency.AddCurrency(10);

        // // Tries to purchase item passing through a test item and Player
        // TestShop.TryPurchaseItem(TestItem, TestPlayer);

        // Assert.AreEqual(0, PlayerCurrency.GetCurrentNectar(), "Player's currency should be deducted by the correct amount");

        // // Player Should have Item in their inventory
        // bool HasItem = TestPlayer.collected.contains();
        // Assert.IsTrue(HasItem, "Checking that item has been added to players Inventory");
    }

    [Test]
    public void PlayerBuysItem_NotEnoughMoney()
    {
        Assert.Fail("Not implemented yet — TDD placeholder");
        // TestContext.WriteLine("Running test: PlayerBuysItem_NotEnoughMoney");

        // // Player doesn't have enough money they shouldn't get that item
        // PlayerCurrency.AddCurrency(2);

        // // Tries to purchase item passing through a test item and Player
        // TestShop.TryPurchaseItem(TestItem, TestPlayer);

        // Assert.AreEqual(2, PlayerCurrency.GetCurrentNectar(), "Player's currency shouldn't be deducted, insufficient funds to purchase item");

        // // Player Should have Item in their inventory
        // bool HasItem = TestPlayer.collected.contains();
        // Assert.IsFalse(HasItem, "Checking that item has not been added to players Inventory");

    }

    [Test]
    public void ItemRemovedFromShopsInventory()
    {

        Assert.Fail("Not implemented yet — TDD placeholder");
        // TestContext.WriteLine("Running test: ItemRemovedFromShopsInventory");

        // // Tries to purchase item
        // PlayerCurrency.AddCurrency(10);
        // //TestShop.TryPurchaseItem(TestItem, TestPlayer);

        // // After purchase complete Shop's Inventory has removed that item
        // bool HasItem = TestShop.ShopStock.contains(TestItem);
        // Assert.IsFalse(HasItem, "Checking that item has been removed from Shop's Inventory");
    }


    [Test]
    public void ItemRemovedFromShopsItemPool()
    {
        Assert.Fail("Not implemented yet — TDD placeholder");
        // TestContext.WriteLine("Running test: ItemRemovedFromShopsItemPool");

        // // Tries to purchase item
        // PlayerCurrency.AddCurrency(10);
        // //TestShop.TryPurchaseItem(TestItem, TestPlayer);

        // // After purchase complete Shop Item Pool has removed that item for future levels
        // bool HasItem = TestShop.ItemPool.contains(TestItem);
        // Assert.IsFalse(HasItem, "Checking that item has been removed from the Shop Item Pool");

    }

}
