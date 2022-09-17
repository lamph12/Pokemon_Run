using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager
{
    public static DateTime GetCurrentRealTime()
    {
        //Test
        return DateTime.Now;
    }

    public static bool IsPassTheDay(DateTime oldTime, DateTime currentTime)
    {
        var replaceOldTime = new DateTime(oldTime.Year, oldTime.Month, oldTime.Day);
        var replaceCurrentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);

        if (replaceCurrentTime > replaceOldTime)
            return true;

        return false;
    }


    /// <summary>
    ///     Chuyển đổi thời gian hiện tại về thời gian đầu ngày 00:00:00 day/mouth/year
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTime ParseTimeStartDay(DateTime time)
    {
        return new DateTime(time.Year, time.Month, time.Day);
    }

    /// <summary>
    ///     Chuyển đổi thời gian hiện tại về thời gian đầu tuần
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTime ParseTimeStartWeek(DateTime time)
    {
        time = ParseTimeStartDay(time);
        var timePass = (int)time.DayOfWeek - 1;
        Debug.Log("time.DayOfWeek " + time.DayOfWeek);
        var a = time.AddDays(-timePass);
        return new DateTime(a.Year, a.Month, a.Day);
    }

    /// <summary>
    ///     Lấy ra thời điểm cuối tháng
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTime GetLastDayOfMonth(DateTime time)
    {
        var daysInMonth = DateTime.DaysInMonth(time.Year, time.Month);
        var start = new DateTime(time.Year, time.Month, 1);
        var last = start.AddDays(daysInMonth - 1);
        return last;
    }


    public static long TimeLeftPassTheDay(DateTime currentTime)
    {
        var replaceCurrentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);
        var nextTime = replaceCurrentTime.AddDays(1);


        return CaculateTime(currentTime, nextTime);
    }

    public static long CaculateTime(DateTime oldTime, DateTime newTime)
    {
        var diff2 = newTime - oldTime;
        long result = diff2.Days * 24 * 60 * 60 + diff2.Hours * 60 * 60 + diff2.Minutes * 60 + diff2.Seconds;
        if (result >= 0)
            return result;
        return -result;
    }

    public static long CaculatePing(DateTime oldTime, DateTime newTime)
    {
        var diff2 = newTime - oldTime;

        return diff2.Milliseconds;
    }

    //public UnityEngine.UI.Text remainingTime_Text;
    public static void ShowTime(ref Text remainingTime_Text, long remainingTime)
    {
        if (remainingTime < 60)
        {
            remainingTime_Text.text = remainingTime.ToString();
        }
        else if (remainingTime >= 60 && remainingTime < 3600)
        {
            var remainingMinuter = remainingTime / 60;
            var remainingSecons = remainingTime - remainingMinuter * 60;
            if (remainingSecons >= 10)
                remainingTime_Text.text = remainingMinuter + ":" + remainingSecons;
            else
                remainingTime_Text.text = remainingMinuter + ":" + "0" + remainingSecons;
        }
        else if (remainingTime >= 3600 && remainingTime < 86400)
        {
            var remainingHours = remainingTime / 3600;
            var remainingMinuter = (remainingTime - remainingHours * 3600) / 60;
            var remainingSecons = remainingTime - remainingHours * 3600 - remainingMinuter * 60;

            if (remainingMinuter == 0)
            {
                if (remainingSecons >= 10)
                    remainingTime_Text.text = remainingHours + ":" + remainingSecons;
                else
                    remainingTime_Text.text = remainingHours + ":" + "0" + remainingSecons;
            }
            else if (remainingMinuter < 10)
            {
                if (remainingSecons >= 10)
                    remainingTime_Text.text = remainingHours + ":" + "0" + remainingMinuter + ":" + remainingSecons;
                else
                    remainingTime_Text.text =
                        remainingHours + ":" + "0" + remainingMinuter + ":" + "0" + remainingSecons;
            }
            else
            {
                if (remainingSecons >= 10)
                    remainingTime_Text.text = remainingHours + ":" + remainingMinuter + ":" + remainingSecons;
                else
                    remainingTime_Text.text = remainingHours + ":" + remainingMinuter + ":" + "0" + remainingSecons;
            }
        }
    }

    public static string ShowMinutesTime(long remainingTime)
    {
        var result = "00:00";
        if (remainingTime < 60)
        {
            if (remainingTime >= 10)
                result = "00:" + remainingTime;
            else
                result = "00:0" + remainingTime;
        }

        return result;
    }

    public static string ShowTime(long remainingTime)
    {
        var result = "00:00:00";
        if (remainingTime < 60)
        {
            if (remainingTime >= 10)
                result = "00:00:" + remainingTime;
            else
                result = "00:00:0" + remainingTime;
        }
        else if (remainingTime >= 60 && remainingTime < 3600)
        {
            var remainingMinuter = remainingTime / 60;
            var remainingSecons = remainingTime - remainingMinuter * 60;

            if (remainingMinuter >= 10)
            {
                if (remainingSecons >= 10)
                    result = "00:" + remainingMinuter + ":" + remainingSecons;
                else
                    result = "00:" + remainingMinuter + ":" + "0" + remainingSecons;
            }
            else
            {
                if (remainingSecons >= 10)
                    result = "00:0" + remainingMinuter + ":" + remainingSecons;
                else
                    result = "00:0" + remainingMinuter + ":" + "0" + remainingSecons;
            }
        }

        else if (remainingTime >= 3600 && remainingTime < 86400)


        {
            var remainingHours = remainingTime / 3600;
            var remainingMinuter = (remainingTime - remainingHours * 3600) / 60;
            var remainingSecons = remainingTime - remainingHours * 3600 - remainingMinuter * 60;

            if (remainingMinuter == 0)
            {
                if (remainingSecons >= 10)
                    result = remainingHours + ":00:" + remainingSecons;
                else
                    result = remainingHours + ":00:0" + remainingSecons;
            }
            else if (remainingMinuter < 10)
            {
                if (remainingSecons >= 10)
                    result = remainingHours + ":" + "0" + remainingMinuter + ":" + remainingSecons;
                else
                    result = remainingHours + ":" + "0" + remainingMinuter + ":" + "0" + remainingSecons;
            }
            else
            {
                if (remainingSecons >= 10)
                    result = remainingHours + ":" + remainingMinuter + ":" + remainingSecons;
                else
                    result = remainingHours + ":" + remainingMinuter + ":" + "0" + remainingSecons;
            }
        }

        else if (remainingTime >= 86400)
        {
            var remainingDay = remainingTime / 86400;
            var remainingHours = (remainingTime - remainingDay * 86400) / 3600;

            result = remainingDay + " Day " + remainingHours + " Hour";
        }

        return result;
    }

    public static string ShowTime2(long remainingTime)
    {
        var result = "00:00:00";
        if (remainingTime < 60)
        {
            if (remainingTime >= 10)
                result = "00:" + remainingTime;
            else
                result = "00:0" + remainingTime;
        }
        else if (remainingTime >= 60 && remainingTime < 3600)
        {
            var remainingMinuter = remainingTime / 60;
            var remainingSecons = remainingTime - remainingMinuter * 60;

            if (remainingMinuter >= 10)
            {
                if (remainingSecons >= 10)
                    result = remainingMinuter + ":" + remainingSecons;
                else
                    result = remainingMinuter + ":" + "0" + remainingSecons;
            }
            else
            {
                if (remainingSecons >= 10)
                    result = "0" + remainingMinuter + ":" + remainingSecons;
                else
                    result = "0" + remainingMinuter + ":" + "0" + remainingSecons;
            }
        }

        else if (remainingTime >= 3600 && remainingTime < 86400)


        {
            var remainingHours = remainingTime / 3600;
            var remainingMinuter = (remainingTime - remainingHours * 3600) / 60;
            var remainingSecons = remainingTime - remainingHours * 3600 - remainingMinuter * 60;

            if (remainingMinuter == 0)
            {
                if (remainingSecons >= 10)
                    result = remainingHours + ":00:" + remainingSecons;
                else
                    result = remainingHours + ":00:0" + remainingSecons;
            }
            else if (remainingMinuter < 10)
            {
                if (remainingSecons >= 10)
                    result = remainingHours + ":" + "0" + remainingMinuter + ":" + remainingSecons;
                else
                    result = remainingHours + ":" + "0" + remainingMinuter + ":" + "0" + remainingSecons;
            }
            else
            {
                if (remainingSecons >= 10)
                    result = remainingHours + ":" + remainingMinuter + ":" + remainingSecons;
                else
                    result = remainingHours + ":" + remainingMinuter + ":" + "0" + remainingSecons;
            }
        }

        else if (remainingTime >= 86400)
        {
            var remainingDay = remainingTime / 86400;
            var remainingHours = (remainingTime - remainingDay * 86400) / 3600;

            result = remainingDay + " Day " + remainingHours + " Hour";
        }

        return result;
    }

    public static void ShowTimeOfText(ref Text remainingTime_Text, long remainingTime)
    {
        //if (remainingTime < 60)
        //    remainingTime_Text.text = remainingTime.ToString() + Localization.Get("lb_s");
        //else if (remainingTime >= 60 && remainingTime < 3600)
        //{
        //    long remainingMinuter = remainingTime / 60;
        //    long remainingSecons = remainingTime - remainingMinuter * 60;
        //    if (remainingSecons >= 10)
        //        remainingTime_Text.text = remainingMinuter.ToString() + Localization.Get("lb_m") + remainingSecons.ToString() + Localization.Get("lb_s");
        //    else
        //        remainingTime_Text.text = remainingMinuter.ToString() + Localization.Get("lb_m") + "0" + remainingSecons.ToString() + Localization.Get("lb_s");
        //}
        //else if (remainingTime >= 3600 && remainingTime < 86400)
        //{
        //    long remainingHours = remainingTime / 3600;
        //    long remainingMinuter = (remainingTime - remainingHours * 3600) / 60;
        //    long remainingSecons = remainingTime - remainingHours * 3600 - remainingMinuter * 60;

        //    if (remainingMinuter == 0)
        //    {
        //        if (remainingSecons >= 10)
        //            remainingTime_Text.text = remainingHours.ToString() + Localization.Get("lb_h") + remainingSecons.ToString() + Localization.Get("lb_s");
        //        else
        //            remainingTime_Text.text = remainingHours.ToString() + Localization.Get("lb_h") + "0" + remainingSecons.ToString() + Localization.Get("lb_s");

        //    }
        //    else if (remainingMinuter < 10)
        //    {
        //        if (remainingSecons >= 10)
        //            remainingTime_Text.text = remainingHours.ToString() + Localization.Get("lb_h") + "0" + remainingMinuter.ToString() + Localization.Get("lb_m") + remainingSecons.ToString() + Localization.Get("lb_s");
        //        else
        //            remainingTime_Text.text = remainingHours.ToString() + Localization.Get("lb_h") + "0" + remainingMinuter.ToString() + Localization.Get("lb_m") + "0" + remainingSecons.ToString() + Localization.Get("lb_s");
        //    }
        //    else
        //    {
        //        if (remainingSecons >= 10)
        //            remainingTime_Text.text = remainingHours.ToString() + Localization.Get("lb_h") + remainingMinuter.ToString() + Localization.Get("lb_m") + remainingSecons.ToString() + Localization.Get("lb_s");
        //        else
        //            remainingTime_Text.text = remainingHours.ToString() + Localization.Get("lb_h") + remainingMinuter.ToString() + Localization.Get("lb_m") + "0" + remainingSecons.ToString() + Localization.Get("lb_s");
        //    }
        //}
    }


    public static void ShowTimeTextOptimal(ref float remainingTime, ref float timer, Text textShow)
    {
        remainingTime -= Time.unscaledDeltaTime;
        timer += Time.unscaledDeltaTime;

        if (remainingTime > 86400)
        {
            if (timer >= 3600) //1H đếm 1 lần
            {
                textShow.text = ShowTime((long)remainingTime);
                timer = 0;
            }
        }
        else
        {
            if (timer >= 1) //1s đếm 1 lần
            {
                textShow.text = ShowTime((long)remainingTime);
                timer = 0;
            }
        }
    }


    /// <summary>
    ///     return datetime co dang 8h:25m:40s
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static string ToDateTimeString(long seconds)
    {
        var num = seconds / 3600;
        var num2 = seconds % 3600 / 60;
        var num3 = seconds % 60;
        if (num > 0) return string.Format("{0}h:{1:D2}m:{2:D2}s", num, num2, num3);
        if (num2 > 0)
            return num2 + "m:" + num3.ToString("00") + "s";
        //eturn num2.ToString("00") + ":" + num3.ToString("00") + "s";
        return num3 + " s";
        //return num3.ToString("00") + "s";
    }
}