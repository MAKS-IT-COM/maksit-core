using System;

namespace MaksIT.Core.Extensions;

public static class DateTimeExtensions {

  /// <summary>
  /// Adds workdays to a given date, skipping weekends and holidays defined in the holiday calendar.
  /// </summary>
  /// <param name="date"></param>
  /// <param name="days"></param>
  /// <param name="holidayCalendar"></param>
  /// <returns></returns>
  public static DateTime AddWorkdays(this DateTime date, int days, IHolidayCalendar holidayCalendar) {
    if (days == 0)
      return date;

    int direction = days > 0 ? 1 : -1;
    int absDays = Math.Abs(days);
    DateTime currentDate = date;

    while (absDays > 0) {
      // If the current date is a workday, decrement absDays
      if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
          currentDate.DayOfWeek != DayOfWeek.Sunday &&
          !holidayCalendar.Contains(currentDate)) {
        absDays--;
        if (absDays == 0)
          break;
      }

      // Move to the next date
      currentDate = currentDate.AddDays(direction);
    }

    return currentDate;
  }

  /// <summary>
  /// Adds workdays to a given date, skipping weekends and holidays defined in the holiday calendar.
  /// </summary>
  /// <param name="date"></param>
  /// <param name="timeSpanWorkDays"></param>
  /// <param name="holidayCalendar"></param>
  /// <returns></returns>
  public static DateTime AddWorkdays(this DateTime date, TimeSpan timeSpanWorkDays, IHolidayCalendar holidayCalendar) =>
      date.AddWorkdays(timeSpanWorkDays.Days, holidayCalendar);

  /// <summary>
  /// Finds the next specified weekday from the given start date.
  /// </summary>
  /// <param name="start"></param>
  /// <param name="day"></param>
  /// <returns></returns>
  public static DateTime NextWeekday(this DateTime start, DayOfWeek day) {
    int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
    daysToAdd = daysToAdd == 0 ? 7 : daysToAdd; // If today is the target day, move to next week
    return start.AddDays(daysToAdd);
  }

  /// <summary>
  /// Finds the TimeSpan to the next specified weekday from the given start date.
  /// </summary>
  /// <param name="start"></param>
  /// <param name="day"></param>
  /// <returns></returns>
  public static TimeSpan ToNextWeekday(this DateTime start, DayOfWeek day) {
    int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
    daysToAdd = daysToAdd == 0 ? 7 : daysToAdd;
    return TimeSpan.FromDays(daysToAdd);
  }

  /// <summary>
  /// Finds the next end of month from the given date.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static DateTime NextEndOfMonth(this DateTime date) {
    var nextMonth = date.AddMonths(1);
    return new DateTime(
        nextMonth.Year,
        nextMonth.Month,
        DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month),
        date.Hour,
        date.Minute,
        date.Second,
        date.Kind);
  }

  /// <summary>
  /// Finds the end of month for the given date.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static DateTime EndOfMonth(this DateTime date) =>
      new DateTime(
          date.Year,
          date.Month,
          DateTime.DaysInMonth(date.Year, date.Month),
          date.Hour,
          date.Minute,
          date.Second,
          date.Kind);

  /// <summary>
  /// Finds the begin of month for the given date.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static DateTime BeginOfMonth(this DateTime date) =>
      new DateTime(date.Year, date.Month, 1, date.Hour, date.Minute, date.Second, date.Kind);

  /// <summary>
  /// Finds the start of year for the given date.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static DateTime StartOfYear(this DateTime date) =>
      new DateTime(date.Year, 1, 1, date.Hour, date.Minute, date.Second, date.Kind);

  /// <summary>
  /// Finds the end of year for the given date.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static DateTime EndOfYear(this DateTime date) =>
      new DateTime(date.Year, 12, 31, date.Hour, date.Minute, date.Second, date.Kind);

  /// <summary>
  /// Determines if the given date is the end of the month.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static bool IsEndOfMonth(this DateTime date) =>
      date.Day == DateTime.DaysInMonth(date.Year, date.Month);

  /// <summary>
  /// Determines if the given date is the begin of the month.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static bool IsBeginOfMonth(this DateTime date) => date.Day == 1;

  /// <summary>
  /// Determines if the given date is the end of the year.
  /// </summary>
  /// <param name="date"></param>
  /// <returns></returns>
  public static bool IsEndOfYear(this DateTime date) => date.Day == 31 && date.Month == 12;

  /// <summary>
  /// Determines if the given date is the begin of the year.
  /// </summary>
  /// <param name="date"></param>
  /// <param name="targetDate"></param>
  /// <returns></returns>
  public static bool IsSameMonth(this DateTime date, DateTime targetDate) =>
      date.Month == targetDate.Month && date.Year == targetDate.Year;

  /// <summary>
  /// Calculates the difference in complete years between two dates.
  /// Implementation follows the logic used in Excel's DATEDIF function
  /// "COMPLETE calendar years in between dates"
  /// </summary>
  /// <param name="startDate"></param>
  /// <param name="endDate"></param>
  /// <returns></returns>
  public static int GetDifferenceInYears(this DateTime startDate, DateTime endDate) {
    int years = endDate.Year - startDate.Year;

    if (endDate.Month < startDate.Month ||
        (endDate.Month == startDate.Month && endDate.Day < startDate.Day)) {
      years--;
    }

    return years;
  }
}

public interface IHolidayCalendar {
  bool Contains(DateTime date);
}
