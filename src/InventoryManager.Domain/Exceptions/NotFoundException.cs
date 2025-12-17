using System.Diagnostics.CodeAnalysis;

namespace InventoryManager.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class NotFoundException(string message) : Exception(message);