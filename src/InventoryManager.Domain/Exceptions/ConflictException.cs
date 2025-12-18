using System.Diagnostics.CodeAnalysis;

namespace InventoryManager.Domain.Exceptions;

[ExcludeFromCodeCoverage(Justification = "Simple exception wrapper without logic")]
public sealed class ConflictException(string message) : Exception(message);