using System.Diagnostics.CodeAnalysis;

namespace InventoryManager.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class ConflictException(string message) : Exception(message);