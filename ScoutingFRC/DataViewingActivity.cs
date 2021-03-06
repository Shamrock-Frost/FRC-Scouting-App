using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ScoutingFRC
{
    [Activity(Label = "Data Viewing", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DataViewingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ViewData);
            var bytes = Intent.GetByteArrayExtra("MatchBytes");
            List<TeamData> MatchList = MatchData.Deserialize<List<TeamData>>(bytes);

            if (MatchList.Count > 0)
            {
                displayData(MatchList);
            }

        }

        private void displayData(List<TeamData> datas)
        {
            FindViewById<TextView>(Resource.Id.textViewTeamNumber).Text = datas[0].teamNumber.ToString();
            int count = datas.Count;
            string matches = "";
            int matchCount = 0;
            int[] gears = new int[4];
            int[] HighGoals = new int[4];
            int[] LowGoals = new int[4];
            int baseline = 0;
            double climbing =0;
            foreach (var teamData in datas)
            {
                if (!(teamData is MatchData))
                {
                    continue;
                }
                matchCount++;
                MatchData matchData = teamData as MatchData;
                matches += matchData.match + ", ";
                if (matchData.automomous.oneTimePoints)
                {
                    baseline++;
                }
                if (matchData.teleoperated.oneTimePoints)
                {
                    climbing++;
                }
                addScoringMethod(matchData.automomous.gears, 0, gears);
                addScoringMethod(matchData.teleoperated.gears, 2, gears);
                addScoringMethod(matchData.automomous.highBoiler, 0, HighGoals);
                addScoringMethod(matchData.teleoperated.highBoiler, 2, HighGoals);
                addScoringMethod(matchData.automomous.lowBoiler, 0, LowGoals);
                addScoringMethod(matchData.teleoperated.lowBoiler, 2, LowGoals);
            }

            //[autoSucc, autoall]
            double[] high = divide(HighGoals, matchCount);
            double[] low = divide(LowGoals, matchCount);
            double[] gear = divide(gears, matchCount);
            double baselinePercentage = (((double)baseline)/matchCount)*100;
            double climbingPercentage = (climbing / matchCount) * 100;

            UpdateTextView(Resource.Id.textViewBaseline, $"Baseline - {Math.Round(baselinePercentage,2)}%",(int)baselinePercentage);
            UpdateTextView(Resource.Id.textViewAutoGear, $"Gear - {Math.Round(gear[0]*100, 2)}%", gear[0]);
            UpdateTextView(Resource.Id.textViewAutoHG, $"High Goals - {Math.Round(high[0], 2)}", high[0]);
            UpdateTextView(Resource.Id.textViewAutoLG, $"Low Goals - {Math.Round(low[0], 2)}", low[0]);

            UpdateTextView(Resource.Id.textViewTeleGears, $"Gears - {Math.Round(gear[2], 2)}/{Math.Round(gear[3], 2)}", gear[3]);
            UpdateTextView(Resource.Id.textViewTeleHG, $"High Goals - {Math.Round(high[2], 2)}/{Math.Round(high[3], 2)}", high[3]);
            UpdateTextView(Resource.Id.textViewTeleLG, $"Low Goals - {Math.Round(low[2], 2)}/{Math.Round(low[3], 2)}", low[3]);
            UpdateTextView(Resource.Id.textViewClimbingView, $"Climbing - {Math.Round(climbingPercentage, 2)}%", climbingPercentage);
            if (matchCount > 0)
            {
                FindViewById<TextView>(Resource.Id.textView1).Text = ((matchCount>1)? "Mathces: " : "Match: ") + matches.Substring(0, matches.Length - 2);
                double autoPoints = (baselinePercentage/100)*5 + (gear[0])*60 + high[0] + low[0]/3;
                double telePoints = (climbingPercentage / 100) * 50 + (gear[2]) * 10 + high[0]/3 + low[0] / 9;
                FindViewById<TextView>(Resource.Id.textViewAutoPts).Text = Math.Round(autoPoints, 3) + " pts";
                FindViewById<TextView>(Resource.Id.textViewTelePts).Text = Math.Round(telePoints, 3) + " pts";

            }
            else
            {
                FindViewById<TextView>(Resource.Id.textView1).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.linearLayoutAuto).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.linearLayoutTele).Visibility = ViewStates.Gone;
            }
           LinearLayout.LayoutParams textViewLayout = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            foreach (var teamData in datas)
            {
                if (!string.IsNullOrEmpty(teamData.notes))
                {
                    String note = ($"\"{teamData.notes}\" - {teamData.scoutName}");
                    TextView text = new TextView(this);
                    text.LayoutParameters = textViewLayout;
                    text.Text = note;
                    FindViewById<LinearLayout>(Resource.Id.linearLayoutListNotes).AddView(text);
                }
            }
         

        }

        private void UpdateTextView(int id, String value, double visable)
        {
            using (TextView textView = FindViewById<TextView>(id))
            {
                if (visable > 0)
                {
                    textView.Text = value;
                }
                else
                {
                    textView.Visibility = ViewStates.Gone;
                }
            }
        }

        private double[] divide(int[] ar, int a)
        {
            double[] ret = new double[ar.Length];
            for (int j = 0; j < ar.Length; j++)
            {
                ret[j] = ((double) ar[j])/a;
            }
            return ret;
        }

        private void addScoringMethod(MatchData.PerformanceData.ScoringMethod method, int start, int[] arr)
        {
            arr[start] += method.successes;
            arr[start + 1] += method.failedAttempts + method.successes;
        }
    }
}