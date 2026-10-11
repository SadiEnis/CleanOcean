using System;
using _Game.Scripts.Gameplay.Rescue;

public class GameEventsManager : MonoSingleton<GameEventsManager>
{
    public event EventHandler OnGameStarted;
    public event EventHandler OnGameEnded;
    public event EventHandler<OnEnterRecyclableObjectArgs> OnEnterRecyclableObject;

    public event EventHandler<OnRescueCompletedEventsArgs> OnRescueCompleted;

    public class OnRescueCompletedEventsArgs : EventArgs
    {
        public RescueTarget rescueTarget;
    }
    
    public class OnEnterRecyclableObjectArgs : EventArgs
    {
        public Interfaces.IRecyclable recyclableObject;
    }

    public event EventHandler<OnRecycledEnterArgs> OnRecycledItem;

    public class OnRecycledEnterArgs : EventArgs
    {
        public Interfaces.IRecyclable recyclableObject;
    }

    public event EventHandler<OnScoreEventsArgs> OnScoreIncrease;
    public event EventHandler<OnScoreEventsArgs> OnScoreDecrease;
    public event EventHandler OnScoreChanged;

    public class OnScoreEventsArgs : EventArgs
    {
        public ulong playerId;
        public int score;
    }

    public event EventHandler<OnHealthChangedEventsArgs> OnHealthChanged;

    public class OnHealthChangedEventsArgs : EventArgs
    {
        public ulong playerId;
        public int newHealthScore;
    }

    public event EventHandler<OnPlayerDeathEventsArgs> OnPlayerDeath;
    public event EventHandler<OnPlayerRevivedEventsArgs> OnPlayerRevived;

    public class OnPlayerDeathEventsArgs : EventArgs
    {
        public ulong playerId;
    }

    public class OnPlayerRevivedEventsArgs : EventArgs
    {
        public ulong revivedPlayerId;
        public ulong reviverPlayerId;
    }

    public event EventHandler<OnGameTimerUpdatedEventsArgs> OnGameTimerUpdated;

    public class OnGameTimerUpdatedEventsArgs : EventArgs
    {
        public float newVal;
    }

    public void TriggerOnGameStarted() => OnGameStarted?.Invoke(this, EventArgs.Empty);
    public void TriggerOnGameEnded() => OnGameEnded?.Invoke(this, EventArgs.Empty);
    public void TriggerOnScoreChanged() => OnScoreChanged?.Invoke(this, EventArgs.Empty);

    public void TriggerOnRescueCompleted(RescueTarget rescueTarget)
    {
        OnRescueCompleted?.Invoke(this, new OnRescueCompletedEventsArgs
        {
            rescueTarget = rescueTarget
        });
    }
    
    public void TriggerOnGameTimerUpdated(float newValue)
    {
        OnGameTimerUpdated?.Invoke(this, new OnGameTimerUpdatedEventsArgs { newVal = newValue });
    }

    public void TriggerOnHealthChanged(ulong playerId, int newHealthScore)
    {
        OnHealthChanged?.Invoke(this, new OnHealthChangedEventsArgs
        {
            playerId = playerId,
            newHealthScore = newHealthScore
        });
    }

    public void TriggerOnPlayerDeath(ulong playerId)
    {
        OnPlayerDeath?.Invoke(this, new OnPlayerDeathEventsArgs
            {
                playerId = playerId
            }
        );
    }

    public void TriggerOnPlayerRevived(ulong revivedPlayerId, ulong reviverPlayerId)
    {
        OnPlayerRevived?.Invoke(this, new OnPlayerRevivedEventsArgs()
            {
                revivedPlayerId = revivedPlayerId,
                reviverPlayerId = reviverPlayerId
            }
        );
    }

    public void TriggerOnEnterRecyclableObject(Interfaces.IRecyclable recyclable)
    {
        OnEnterRecyclableObject?.Invoke(this, new OnEnterRecyclableObjectArgs
        {
            recyclableObject = recyclable
        });
    }

    public void TriggerOnRecycledItem(Interfaces.IRecyclable recycledItem)
    {
        OnRecycledItem?.Invoke(this, new OnRecycledEnterArgs
        {
            recyclableObject = recycledItem
        });
    }

    public void TriggerOnScoreIncrease(ulong playerId, int score)
    {
        OnScoreIncrease?.Invoke(this, new OnScoreEventsArgs
        {
            playerId = playerId,
            score = score
        });
    }

    public void TriggerOnScoreDecrease(ulong playerId, int score)
    {
        OnScoreDecrease?.Invoke(this, new OnScoreEventsArgs
        {
            playerId = playerId,
            score = score
        });
    }
}