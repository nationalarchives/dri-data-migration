using System;

namespace Api;

public record ReconciliationRow(ReconciliationFieldName Field, Func<string?, object?> Conversion);