﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Extensions.Reactive.Testing;

namespace Uno.Extensions.Reactive.Tests;

[TestClass]
public class Given_ListFeed : FeedTests
{
	// Those are integration tests that demo the basic public API and are not expected to deeply validate feed behavior.

	[TestMethod]
	public async Task When_GetAwaiter()
	{
		var source = new[] { 41, 42, 43 };
		var sut = ListFeed.Async<int>(async ct => source.ToImmutableList());
		var result = await sut;

		result.Should().BeEquivalentTo(source);
	}

	[TestMethod]
	public async Task When_Async()
	{
		var source = new[] { 41, 42, 43 };
		var sut = ListFeed.Async<int>(async ct => source.ToImmutableList());
		var result = await sut.Option(CT);

		result.IsSome(out var items).Should().BeTrue();
		items.Should().BeEquivalentTo(source);
	}

	[TestMethod]
	public async Task When_AsyncEnumerable()
	{
		async IAsyncEnumerable<IImmutableList<int>> GetSource()
		{
			yield return new[] { 40, 41, 42 }.ToImmutableList();
			yield return new[] { 41, 42, 43 }.ToImmutableList();
			yield return new[] { 42, 43, 44 }.ToImmutableList();
		}

		var expected = await GetSource().ToArrayAsync();
		var result = await ListFeed.AsyncEnumerable(GetSource).Record();

		result
			.Select(msg => msg.Current.Data.SomeOrDefault())
			.Should()
			.BeEquivalentTo(expected);
	}

	[TestMethod]
	public async Task When_Create()
	{
		async IAsyncEnumerable<Message<IImmutableList<int>>> GetSource([EnumeratorCancellation] CancellationToken ct)
		{
			var msg = Message<IImmutableList<int>>.Initial;

			yield return msg = msg.With().Data(new[] { 40, 41, 42 }.ToImmutableList());
			yield return msg = msg.With().Data(new[] { 41, 42, 43 }.ToImmutableList());
			yield return msg = msg.With().Data(new[] { 42, 43, 44 }.ToImmutableList());
		}

		var expected = await GetSource(CT).Select(msg => msg.Current.Data.SomeOrDefault()).ToArrayAsync();
		var result = await ListFeed.Create(GetSource).Record();

		result
			.Select(msg => msg.Current.Data.SomeOrDefault())
			.Should()
			.BeEquivalentTo(expected);
	}
}
