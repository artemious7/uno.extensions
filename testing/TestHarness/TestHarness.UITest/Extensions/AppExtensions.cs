﻿
namespace TestHarness.UITest.Extensions;

public static class AppExtensions
{
	public static void DragCoordinates(this IApp app, PointF from, PointF to) => app.DragCoordinates(from.X, from.Y, to.X, to.Y);

	private static float? _scaling;
	public static float GetDisplayScreenScaling(this IApp app)
	{
#if IS_RUNTIME_UI_TESTS
		return 1f;
#else
		return _scaling ?? (float)(_scaling = GetScaling());

		float GetScaling()
		{
			var scalingRaw = app.InvokeGeneric("browser:SampleRunner|GetDisplayScreenScaling", "0");

			if (float.TryParse(scalingRaw?.ToString(), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var scaling))
			{
				Console.WriteLine($"Display Scaling: {scaling}");
				return scaling / 100f;
			}
			else
			{
				return 1f;
			}
		}
#endif
	}

	public static Func<IAppQuery, IAppQuery> WaitThenTap(this IApp app, Func<IAppQuery, IAppQuery> query, TimeSpan? timeout = null)
	{
		app.WaitForElement(query, timeout: timeout);
		app.Tap(query);

		return query;
	}

	public static Func<IAppQuery, IAppQuery> WaitThenTap(this IApp app, string marked, TimeSpan? timeout = null)
		=> WaitThenTap(app, q => q.All().Marked(marked), timeout);

	public static QueryEx ToQueryEx(this Func<IAppQuery, IAppQuery> query) => new QueryEx(query);
}
