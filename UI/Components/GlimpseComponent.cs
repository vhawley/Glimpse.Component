﻿using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class GlimpseComponent : IComponent
    {
        public Settings Settings { get; set; }

        protected LiveSplitState State { get; set; }
        protected Form Form { get; set; }

        public float PaddingTop => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;

        public string ComponentName => $"Glimpse";

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        public RequestFactory Factory;

        public int? runID;
        public TimeSpan? durationAtLastEvent; // for calculating each split diffs
        public DateTime? timeOfLastEvent; // for calculating time from last split until reset

        public GlimpseComponent(LiveSplitState state)
        {
            Factory = new RequestFactory();
            Settings = new Settings(Factory);

            ContextMenuControls = new Dictionary<string, Action>();
            ContextMenuControls.Add("Glimpse Settings", OpenGlimpseSettings);

            State = state;
            Form = state.Form;
            
            // add event listeners
            State.OnStart += State_OnStart;
            State.OnSplit += State_OnSplit;
            State.OnReset += State_OnReset;
            State.OnSkipSplit += State_OnSkipSplit;
            State.OnUndoSplit += State_OnUndoSplit;
            State.OnPause += State_OnPause;
            State.OnResume += State_OnResume;

            // Events not supported by Glimpse currently 
            State.OnUndoAllPauses += State_OnUndoAllPauses;
            State.OnSwitchComparisonPrevious += State_OnSwitchComparisonPrevious;
            State.OnSwitchComparisonNext += State_OnSwitchComparisonNext;
            
        }
        
        public void OpenGlimpseSettings()
        {
            Console.Out.WriteLine("OpenGlimpseSettings");
        }

        private async void State_OnStart(object sender, EventArgs e)
        {
            try
            {
                JObject comparisons = new JObject();
                List<string> splitNames = new List<string>();

                // object to store last comparison time values for each comparison type 
                Dictionary<string, double?> lasts = new Dictionary<string, double?>();

                // go through run to get split names and comparisons; for each segment in run
                for (int i = 0; i < State.Run.Count; i++) // need i for index of comparisons object
                {
                    Segment s = (Segment) State.Run[i];

                    // add to splitnames
                    splitNames.Add(s.Name);

                    // for each comparison object at this segment
                    foreach (string key in s.Comparisons.Keys)
                    {
                        // create array for this comparison if it doesn't exist
                        if (comparisons[key] == null)
                        {
                            comparisons[key] = new JArray();
                        }
                        JArray comparisonsArray = (JArray)comparisons[key];

                        // create double for last comparison value if it doesn't exist
                        if (!lasts.ContainsKey(key))
                        {
                            lasts.Add(key, 0);
                        }

                        // get comparison time and add to array
                        TimeSpan? comparisonTime = s.Comparisons[key].RealTime;
                        try
                        {
                            comparisonsArray.Add(comparisonTime.Value.TotalMilliseconds - lasts[key]);
                            lasts[key] = comparisonTime.Value.TotalMilliseconds;
                        } catch (Exception exc)
                        {
                            comparisonsArray.Add(null);
                            Console.Out.WriteLine(exc.Message);
                        }
                    }
                }

                // send response
                HttpResponseMessage response = await Factory.PostGlimpseRunStartEvent(State.Run.GameName, State.Run.CategoryName, splitNames, comparisons, State.AttemptStarted.Time);
                string responseString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {

                    // get run ID for future events
                    try
                    {
                        JObject responseObject = JObject.Parse(responseString);

                        // set runID and initiate durationAtLastEvent for diff use
                        runID = responseObject.Value<int?>("runID");
                        if (!runID.HasValue)
                        {
                            Console.Out.WriteLine("runID not an int");
                        } else
                        {
                            Console.Out.WriteLine(runID);
                            durationAtLastEvent = new TimeSpan();
                            timeOfLastEvent = State.AttemptStarted.Time;
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.Out.WriteLine(exc.Message);
                        Console.Out.WriteLine(responseString);
                    }
                } else
                {
                    Console.Out.WriteLine(response.StatusCode + " ERROR: " + responseString);
                }

            }
            catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }
        }

        private async void State_OnSplit(object sender, EventArgs e)
        {
            try
            {
                // make sure we have a runID first and foremost
                if (runID.HasValue)
                {
                    DateTime eventTime = DateTime.UtcNow;
                    int completedSplitNumber = State.CurrentSplitIndex;
                    if (durationAtLastEvent.HasValue && State.CurrentTime.RealTime.HasValue)
                    {

                        TimeSpan contributableDuration = State.CurrentTime.RealTime.Value - durationAtLastEvent.Value;
                        TimeSpan totalDuration = State.CurrentTime.RealTime.Value;

                        durationAtLastEvent = totalDuration;
                        timeOfLastEvent = eventTime;

                        // check if its a mid-run split or a run ending
                        if (State.CurrentSplit != null) // mid-run
                        {
                            HttpResponseMessage response = await Factory.PostGlimpseRunSplitEvent(runID.Value, eventTime, completedSplitNumber, contributableDuration, totalDuration);
                        }
                        else
                        {
                            HttpResponseMessage response = await Factory.PostGlimpseRunEndEvent(runID.Value, eventTime, completedSplitNumber, contributableDuration, totalDuration);
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine("No durationAtLastEvent.  Can't send event because we can't calculate contributableDuration");
                    }
                }
            } catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }
            
        }

        private async void State_OnReset(object sender, TimerPhase e)
        {
            try
            {
                // make sure we have a runID first and foremost
                if (runID.HasValue)
                {
                    DateTime eventTime = DateTime.UtcNow;
                    TimeSpan totalDuration = new TimeSpan();

                    // must calculate totalDuration ourselves because attemptduration is unreliable (different value depending on whether user saves best splits or not)
                    if (durationAtLastEvent.HasValue && timeOfLastEvent.HasValue)
                    {
                        if (e == TimerPhase.Running)
                        {
                            totalDuration = durationAtLastEvent.Value + (DateTime.UtcNow - timeOfLastEvent.Value);
                        }
                        else
                        {
                            totalDuration = durationAtLastEvent.Value;
                        }
                    }

                    HttpResponseMessage response = await Factory.PostGlimpseRunFinalizeEvent(runID.Value, eventTime, totalDuration);
                }
            } catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }
            
        }

        private void State_OnSkipSplit(object sender, EventArgs e)
        {
            Console.Out.WriteLine("OnSkipSplit");
        }

        private void State_OnUndoSplit(object sender, EventArgs e)
        {
            Console.Out.WriteLine("OnUndoSplit");
        }

        private async void State_OnPause(object sender, EventArgs e)
        {
            try
            {
                // make sure we have a runID first and foremost
                if (runID.HasValue)
                {
                    DateTime eventTime = DateTime.UtcNow;
                    int completedSplitNumber = State.CurrentSplitIndex;
                    if (durationAtLastEvent.HasValue && State.CurrentTime.RealTime.HasValue)
                    {
                        TimeSpan contributableDuration = State.CurrentTime.RealTime.Value - durationAtLastEvent.Value;
                        TimeSpan totalDuration = State.CurrentTime.RealTime.Value;

                        // update time and duration
                        durationAtLastEvent = totalDuration;
                        timeOfLastEvent = eventTime;

                        HttpResponseMessage response = await Factory.PostGlimpseRunPauseEvent(runID.Value, eventTime, contributableDuration, totalDuration);
                    }
                    else
                    {
                        Console.Out.WriteLine("No durationAtLastEvent.  Can't send event because we can't calculate contributableDuration");
                    }
                }
            } catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }
            
        }

        private async void State_OnResume(object sender, EventArgs e)
        {
            try
            {
                // make sure we have a runID first and foremost
                if (runID.HasValue)
                {
                    DateTime eventTime = DateTime.UtcNow;
                    TimeSpan totalDuration = new TimeSpan();
                    if (State.CurrentTime.RealTime.HasValue)
                    {
                        totalDuration = State.CurrentTime.RealTime.Value;
                    }

                    // update time and duration
                    durationAtLastEvent = totalDuration;
                    timeOfLastEvent = eventTime;

                    HttpResponseMessage response = await Factory.PostGlimpseRunResumeEvent(runID.Value, eventTime, totalDuration);
                }
            } catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }
            
        }

        private void State_OnUndoAllPauses(object sender, EventArgs e)
        {
            Console.Out.WriteLine("OnUndoAllPauses");
        }

        private void State_OnSwitchComparisonPrevious(object sender, EventArgs e)
        {
            Console.Out.WriteLine("OnSwitchComparisonPrevious");
        }

        private void State_OnSwitchComparisonNext(object sender, EventArgs e)
        {
            Console.Out.WriteLine("OnSwitchComparisonNext");
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
        }

        public float VerticalHeight => 0;

        public float MinimumWidth => 0;

        public float HorizontalWidth => 0;

        public float MinimumHeight => 0;

        public XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
        }

        public void Dispose()
        {
            // remove event listeners
            State.OnStart -= State_OnStart;
        }

        public int GetSettingsHashCode()
        {
            return Settings.GetSettingsHashCode();
        }
    }
}
