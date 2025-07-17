using System;

namespace Api;

public record ReconciliationRow(ReconciliationFieldNames Field, Func<string?, object?> Conversion);