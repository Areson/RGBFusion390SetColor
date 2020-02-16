using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using SelLEDControl;

namespace RGBFusion390SetColor.Animation
{
    public class AnimationHandler
    {
        private int clock;
        private readonly int stepLengthMs;
        private readonly CancellationToken cancellationToken;
        private readonly ConcurrentDictionary<sbyte, Animation> animations;
        private readonly object LOCK_OBJECT = new object();
        private readonly object PLAY_LOCK_OBJECT = new object();
        private readonly Action<List<CommUI.Area_class>> handler;
        private readonly ManualResetEvent canPlay;        
        private bool isPaused = true;

        public AnimationHandler(CancellationToken cancellationToken, Action<List<CommUI.Area_class>> handler, int stepLengthMs = 10)
        {
            clock = 0;
            this.stepLengthMs = stepLengthMs;
            this.cancellationToken = cancellationToken;
            animations = new ConcurrentDictionary<sbyte, Animation>();
            this.handler = handler;
            this.canPlay = new ManualResetEvent(false);
        }

        public void AddAnimationPhase(sbyte areaId, AnimationPhase phase)
        {
            lock (LOCK_OBJECT)
            {
                var animation = animations.GetOrAdd(areaId, (a) => new Animation(a));
                animation.AddPhase(phase);

                if (!isPaused)
                {
                    canPlay.Set();
                }
            }
        }

        public void AddAnimation(Animation animation)
        {
            if (animation.Ready)
            {
                lock (LOCK_OBJECT)
                {
                    var existing = animations.GetOrAdd(animation.AreaId, (a) => new Animation(a));

                    foreach (var phase in animation.Phases)
                    {
                        existing.AddPhase(phase);
                    }

                    if (!isPaused)
                    {
                        canPlay.Set();
                    }
                }
            }
        }

        public void RemoveAnimation(sbyte areaId)
        {            
            if (animations.ContainsKey(areaId))
            {
                lock (LOCK_OBJECT)
                {
                    int tries = 0;
                    while (!animations.TryRemove(areaId, out _) && tries < 3) ;

                    if (!animations.Any() && !isPaused)
                    {
                        Stop();
                    }
                }
            }
        }

        public void Play()
        {
            lock (PLAY_LOCK_OBJECT)
            {
                if (isPaused)
                {
                    isPaused = false;
                    if (animations.Any())
                    {
                        canPlay.Set();
                    }
                }
            }
        }

        public void Pause()
        {
            lock (PLAY_LOCK_OBJECT)
            {
                if (!isPaused)
                {
                    isPaused = true;
                    canPlay.Reset();
                }
            }
        }

        public void Stop()
        {
            lock (PLAY_LOCK_OBJECT)
            {
                isPaused = true;
                canPlay.Reset();
                foreach (var a in animations.Values)
                {
                    a.Reset();
                }

                clock = 0;
            }
        }

        public void Reset()
        {
            lock (LOCK_OBJECT)
            {
                animations.Clear();
                Stop();
            }
        }

        public void AnimationLoop()
        {
            while (true)
            {
                if (WaitHandle.WaitAny(new WaitHandle[] { cancellationToken.WaitHandle, canPlay}) == 0)
                {
                    // Animation loop has been canceled.
                    return;
                }
                
                WaitHandle.WaitAny(new WaitHandle[] { cancellationToken.WaitHandle }, stepLengthMs);

                var animationList = animations.Values.ToList();

                if (animationList.Any())
                {
                    List<CommUI.Area_class> areaInfo = new List<CommUI.Area_class>();

                    foreach (var animation in animationList)
                    {
                        if (animation.Ready)
                        {
                            areaInfo.Add(animation.Step(clock).BuildArea());
                        }
                    }

                    handler(areaInfo);

                    clock++;
                }
            }
        }
    }
}
