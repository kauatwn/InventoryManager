namespace InventoryManager.Application.UseCases.Products.Delete;

public interface IDeleteProductUseCase
{
    void Execute(Guid id);
}