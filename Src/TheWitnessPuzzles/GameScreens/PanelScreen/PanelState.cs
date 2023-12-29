﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameManager
{
    [Flags]
    enum PanelStates
    {
        None = 0,
        Solved = 1,
        ErrorHappened = 2,
        ErrorBlink = 4,
        LineFade = 8,
        EliminationStarted = 16,
        EliminationFinished = 32,
        FinishTracing = 64
    }

    class PanelState
    {
        public PanelStates State { get; private set; }

        const int fadeOutErrorMaxTime = 400;
        const int fadeOutNeutralMaxTime = 80;
        float fadeOutTime = 0;
        public float LineFadeOpacity => fadeOutTime / (State.HasFlag(PanelStates.ErrorHappened) ? fadeOutErrorMaxTime : fadeOutNeutralMaxTime);
        public bool IsFading => fadeOutTime > 0;

        const int errorBlinkMaxTime = 800;
        int errorBlinkTime = 0;
        public float ErrorBlinkOpacity { get; private set; } = 1f;
        float errorBlinkSpeed = 0.1f;
        bool errorBlinkOpacityDown = true;

        public float FinishTracingBlinkOpacity { get; private set; } = 0f;
        float finishTracingBlinkSpeed = 0.05f;
        bool finishTracingBlinkOpacityDown = false;

        const int eliminationTimeMax = 70;
        int eliminationTime = 0;
        const int eliminationFadeTimeMax = 80;
        float eliminationFadeTime = eliminationFadeTimeMax;
        public float EliminationFadeOpacity => eliminationFadeTime / eliminationFadeTimeMax;
        public event Action EliminationFinished;

        public PanelState()
        {
            State = PanelStates.None;
            EliminationFinished = new Action(EliminationFinishedHandler);
        }

        public void Update()
        {
            // If fadeOutTime > 0 and elimination either not started or already finished
            if(fadeOutTime > 0 
                && (!State.HasFlag(PanelStates.EliminationStarted) 
                    || (State.HasFlag(PanelStates.EliminationStarted | PanelStates.EliminationFinished))))
            {
                fadeOutTime--;
                if (fadeOutTime == 0)
                    State &= ~PanelStates.LineFade;
            }

            if(errorBlinkTime > 0 || (errorBlinkTime == 0 && ErrorBlinkOpacity > 0))
            {
                if (errorBlinkTime > 0)
                    errorBlinkTime--;

                if (errorBlinkOpacityDown)
                {
                    ErrorBlinkOpacity = Math.Max(0, ErrorBlinkOpacity - errorBlinkSpeed);
                    if (ErrorBlinkOpacity == 0)
                        errorBlinkOpacityDown = false;
                }
                else
                {
                    ErrorBlinkOpacity = Math.Min(1f, ErrorBlinkOpacity + errorBlinkSpeed);
                    if (ErrorBlinkOpacity == 1f)
                        errorBlinkOpacityDown = true;
                }

                if (errorBlinkTime == 0)
                    State &= ~PanelStates.ErrorBlink;
            }

            if(eliminationTime > 0)
            {
                eliminationTime--;
                if (eliminationTime == 0)
                    EliminationFinished.Invoke();
            }

            if(eliminationFadeTime < eliminationFadeTimeMax)
                eliminationFadeTime++;

            if(State.HasFlag(PanelStates.FinishTracing))
            {
                if(finishTracingBlinkOpacityDown)
                {
                    FinishTracingBlinkOpacity -= finishTracingBlinkSpeed;
                    if(FinishTracingBlinkOpacity <= 0)
                    {
                        FinishTracingBlinkOpacity = 0;
                        finishTracingBlinkOpacityDown = false;
                    }
                }
                else
                {
                    FinishTracingBlinkOpacity += finishTracingBlinkSpeed;
                    if(FinishTracingBlinkOpacity >= 1f)
                    {
                        FinishTracingBlinkOpacity = 1f;
                        finishTracingBlinkOpacityDown = true;
                    }
                }
            }
        }

        public void InvokeFadeOut(bool isError)
        {
            State &= ~PanelStates.FinishTracing;

            if(isError)
            {
                State |= PanelStates.ErrorHappened | PanelStates.ErrorBlink | PanelStates.LineFade;
                fadeOutTime = fadeOutErrorMaxTime;
                errorBlinkTime = errorBlinkMaxTime;
                ErrorBlinkOpacity = 1f;
                errorBlinkOpacityDown = true;
            }
            else
            {
                State |= PanelStates.LineFade;
                fadeOutTime = fadeOutNeutralMaxTime;
            }
        }

        public void SetSuccess()
        {
            fadeOutTime = 0;
            errorBlinkTime = 0;
            errorBlinkOpacityDown = true;
            State = PanelStates.Solved | (State & (PanelStates.EliminationFinished | PanelStates.EliminationStarted));
        }

        public void ResetToNeutral()
        {
            State = PanelStates.None;
            fadeOutTime = 0;
            ErrorBlinkOpacity = 1f;
            errorBlinkTime = 0;
            errorBlinkOpacityDown = true;
            eliminationFadeTime = eliminationFadeTimeMax;
            eliminationTime = 0;
            FinishTracingBlinkOpacity = 0;
            finishTracingBlinkOpacityDown = false;
        }

        public void InitializeElimination()
        {
            State |= PanelStates.EliminationStarted;
            eliminationTime = eliminationTimeMax;
        }

        private void EliminationFinishedHandler()
        {
            State |= PanelStates.EliminationFinished;
            eliminationFadeTime = 0;
        }

        public void SetFinishTracing(bool state)
        {
            if (state)
            {
                State |= PanelStates.FinishTracing;
                FinishTracingBlinkOpacity = 0;
                finishTracingBlinkOpacityDown = false;
            }
            else
            {
                State &= ~PanelStates.FinishTracing;
            }
        }
    }
}
