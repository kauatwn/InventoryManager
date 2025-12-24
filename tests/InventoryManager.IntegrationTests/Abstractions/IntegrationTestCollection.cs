namespace InventoryManager.IntegrationTests.Abstractions;

[CollectionDefinition("IntegrationTests")]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;