using MaksIT.Core.Extensions;

namespace MaksIT.Core.Tests.Extensions {

  public class DateTimeExtensionsTests {
    // Mock implementation of IHolidayCalendar for testing
    private class TestHolidayCalendar : IHolidayCalendar {
      private readonly HashSet<DateTime> _holidays;

      public TestHolidayCalendar(IEnumerable<DateTime> holidays) {
        _holidays = new HashSet<DateTime>(holidays);
      }

      public bool Contains(DateTime date) {
        return _holidays.Contains(date.Date);
      }
    }

    [Fact]
    public void AddWorkdays_PositiveDays_SkipsWeekendsAndHolidays() {
      // Arrange
      var startDate = new DateTime(2023, 12, 22); // Friday
      int daysToAdd = 5;
      var holidays = new List<DateTime>
      {
                new DateTime(2023, 12, 25), // Christmas Day (Monday)
            };
      var holidayCalendar = new TestHolidayCalendar(holidays);

      // Act
      var resultDate = startDate.AddWorkdays(daysToAdd, holidayCalendar);

      // Assert
      Assert.Equal(new DateTime(2023, 12, 29), resultDate);
    }

    [Fact]
    public void AddWorkdays_NegativeDays_SkipsWeekendsAndHolidays() {
      // Arrange
      var startDate = new DateTime(2023, 1, 3); // Tuesday
      int daysToAdd = -5;
      var holidays = new List<DateTime>
      {
                new DateTime(2023, 1, 1), // New Year's Day (Sunday)
            };
      var holidayCalendar = new TestHolidayCalendar(holidays);

      // Act
      var resultDate = startDate.AddWorkdays(daysToAdd, holidayCalendar);

      // Assert
      Assert.Equal(new DateTime(2022, 12, 28), resultDate);
    }

    // ... rest of your tests remain the same

    [Fact]
    public void NextWeekday_ReturnsNextWeek_WhenTargetDayIsToday() {
      // Arrange
      var startDate = new DateTime(2023, 10, 5); // Thursday
      var targetDay = DayOfWeek.Thursday;

      // Act
      var resultDate = startDate.NextWeekday(targetDay);

      // Assert
      Assert.Equal(new DateTime(2023, 10, 12), resultDate);
    }

    [Fact]
    public void ToNextWeekday_ReturnsSevenDays_WhenTargetDayIsToday() {
      // Arrange
      var startDate = new DateTime(2023, 10, 5); // Thursday
      var targetDay = DayOfWeek.Thursday;

      // Act
      var timeSpan = startDate.ToNextWeekday(targetDay);

      // Assert
      Assert.Equal(TimeSpan.FromDays(7), timeSpan);
    }
  }
}
