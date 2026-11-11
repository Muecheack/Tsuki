namespace cs.Endpoints;

internal static class EndpointValidation
{
    public static IResult? Required(params (string Name, object? Value)[] parameters)
    {
        var errors = parameters
            .Where(parameter => parameter.Value is null)
            .ToDictionary(
                    parameter => parameter.Name,
                    parameter => new[] { $"The {parameter.Name} field is required." });

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }
}
