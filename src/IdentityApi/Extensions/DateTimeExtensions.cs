using System;

namespace IdentityApi.Extensions;

public static class DateTimeExtensions
{
	/// <summary>
	/// Calculates the tolal number of seconds that have passed since the Unix epoch (1970.01.01 00:00:00)
	/// </summary>
	/// <param name="now">The <see cref="DateTime"/> relative to which to calculate</param>
	/// <returns>The total number of seconds since the Unix epoch</returns>
	public static long GetTotalSeconds(this DateTime now)
	{
		return (now.Ticks - DateTime.UnixEpoch.Ticks) / 10_000_000;
	}
}