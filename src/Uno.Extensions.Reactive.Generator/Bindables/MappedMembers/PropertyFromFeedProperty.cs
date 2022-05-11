﻿using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Uno.Extensions.Reactive.Generator;

// Note: This also applies for State
internal record PropertyFromFeedProperty(IPropertySymbol _property, ITypeSymbol _valueType) : IMappedMember
{
	private readonly IPropertySymbol _property = _property;
	private readonly ITypeSymbol _valueType = _valueType;

	/// <inheritdoc />
	public string Name => _property.Name;

	/// <inheritdoc />
	public string GetBackingField()
		=> $"private {NS.Bindings}.Bindable<{_valueType}> _{_property.GetCamelCaseName()};";

	/// <inheritdoc />
	public string GetDeclaration()
		=> new Property(_property.DeclaredAccessibility, _valueType, _property.GetPascalCaseName())
		{
			Getter = $"_{_property.GetCamelCaseName()}.GetValue()",
			Setter = $"_{_property.GetCamelCaseName()}.SetValue(value)",
		};

	/// <inheritdoc />
	public string? GetInitialization()
		=> $"_{_property.GetCamelCaseName()} = new {NS.Bindings}.Bindable<{ _valueType}>(base.Property<{_valueType}>(nameof({_property.Name}), {N.Ctor.Model}.{_property.Name} ?? throw new NullReferenceException(\"The feed property '{_property.Name}' is null. Public feeds fields must be initialized in the constructor.\")));";
}
