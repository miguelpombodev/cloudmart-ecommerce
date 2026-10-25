using System.Text.RegularExpressions;
using BuildingBlocks.Exceptions;

namespace Catalog.Domain.ValueObject;

public sealed class Name : BuildingBlocks.Abstractions.ValueObject
{
	private Name()
	{
		Value = null!;
	}

	private Name(string value)
	{
		Value = value;
	}

	public string Value { get; set; }

	public static Name Create(string value)
	{
		string nameStartedWithNumberRegexPattern = @"^\d";

		if (Regex.IsMatch(value, nameStartedWithNumberRegexPattern))
		{
			throw new DomainException("A Name must not start with any number");
		}

		return new Name(value);
	}

	protected override IEnumerable<object?> RetrieveEqualityComponents()
	{
		yield return Value.ToLower();
	}
}
