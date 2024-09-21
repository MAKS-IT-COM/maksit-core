using System;

namespace MaksIT.Core.Extensions;

public static class DateTimeExtensions {
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

  public static DateTime AddWorkdays(this DateTime date, TimeSpan timeSpanWorkDays, IHolidayCalendar holidayCalendar) =>
      date.AddWorkdays(timeSpanWorkDays.Days, holidayCalendar);

  public static DateTime NextWeekday(this DateTime start, DayOfWeek day) {
    int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
    daysToAdd = daysToAdd == 0 ? 7 : daysToAdd; // If today is the target day, move to next week
    return start.AddDays(daysToAdd);
  }

  public static TimeSpan ToNextWeekday(this DateTime start, DayOfWeek day) {
    int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
    daysToAdd = daysToAdd == 0 ? 7 : daysToAdd;
    return TimeSpan.FromDays(daysToAdd);
  }

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

  public static DateTime EndOfMonth(this DateTime date) =>
      new DateTime(
          date.Year,
          date.Month,
          DateTime.DaysInMonth(date.Year, date.Month),
          date.Hour,
          date.Minute,
          date.Second,
          date.Kind);

  public static DateTime BeginOfMonth(this DateTime date) =>
      new DateTime(date.Year, date.Month, 1, date.Hour, date.Minute, date.Second, date.Kind);

  public static DateTime StartOfYear(this DateTime date) =>
      new DateTime(date.Year, 1, 1, date.Hour, date.Minute, date.Second, date.Kind);

  public static DateTime EndOfYear(this DateTime date) =>
      new DateTime(date.Year, 12, 31, date.Hour, date.Minute, date.Second, date.Kind);

  public static bool IsEndOfMonth(this DateTime date) =>
      date.Day == DateTime.DaysInMonth(date.Year, date.Month);

  public static bool IsBeginOfMonth(this DateTime date) => date.Day == 1;

  public static bool IsEndOfYear(this DateTime date) => date.Day == 31 && date.Month == 12;

  public static bool IsSameMonth(this DateTime date, DateTime targetDate) =>
      date.Month == targetDate.Month && date.Year == targetDate.Year;

  public static int GetDifferenceInYears(this DateTime startDate, DateTime endDate) {
    // Implementation follows the logic used in Excel's DATEDIF function
    // "COMPLETE calendar years in between dates"

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
