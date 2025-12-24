namespace InventoryManager.Application.UseCases.Products.Delete;

public interface IDeleteProductUseCase
{
    Task ExecuteAsync(Guid id);
}