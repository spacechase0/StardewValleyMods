using System;

namespace GenericModConfigMenu.Framework.ModOption
{
    internal class TimePickerModOption : NumericModOption<int>
    {
        private static readonly int HOUR_DIVISOR = 100;
        private static readonly int MINUTES_PER_HOUR = 60;

        public TimePickerModOption(string fieldId, Func<string> name, Func<string> tooltip, ModConfig mod, Func<int> getValue, Action<int> setValue, int min, int max, int interval)
            : base(
                  fieldId: fieldId,
                  name: name,
                  tooltip: tooltip,
                  mod: mod,
                  getValue: () => GetTotalMinutesFromTime(getValue()),
                  setValue: (newValue) => setValue(GetTimeFromTotalMinutes(newValue)),
                  min: GetTotalMinutesFromTime(min),
                  max: GetTotalMinutesFromTime(max),
                  interval: GetTotalMinutesFromTime(interval),
                  formatValue: GetLabelFromTotalMinutes)
        {

        }

        private static int GetTotalMinutesFromTime(int time)
        {
            int hours = time / HOUR_DIVISOR;
            int minutes = time % HOUR_DIVISOR;
            return hours * MINUTES_PER_HOUR + minutes;
        }

        private static int GetTimeFromTotalMinutes(int totalMinutes)
        {
            int hours = totalMinutes / MINUTES_PER_HOUR;
            int minutes = totalMinutes % MINUTES_PER_HOUR;
            return hours * HOUR_DIVISOR + minutes;
        }

        private static string GetLabelFromTotalMinutes(int totalMinutes)
        {
            int hour = totalMinutes / MINUTES_PER_HOUR;
            int min = totalMinutes % MINUTES_PER_HOUR;
            bool isAM = hour % 24 < 12;
            int hourLabel = hour % 12;
            if (hourLabel == 0)
            {
                hourLabel = 12;
            }
            return string.Format("{0}:{1:D2} {2}", hourLabel, min, isAM ? "AM" : "PM");
        }
    }
}
