using System;

namespace IdentityApi.Extensions;

public static class DateTimeExtensions
{
	public static long GetTotalSeconds(this DateTime now)
	{
		var start = new DateTime(1970, 1, 1, 0, 0, 0);
		return (now.Ticks - start.Ticks) / 10_000_000;
	}
}