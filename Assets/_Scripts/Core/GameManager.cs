using System;
using UnityEngine;

namespace JamUnity.Core
{
    [Serializable]
    public enum Diff
    {
        Easy,
        Normal,
        Hard
    }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public Diff CurrentGameDiff {  get; private set; }

        public float ElapsedTime { get; private set; }


        public bool IsRunning { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (IsRunning)
            {
                ElapsedTime += Time.deltaTime;
            }
        }
        public void StartTimer() { ElapsedTime = 0f; IsRunning = true; }
        public void Pause() => IsRunning = false;
        public void Resume() => IsRunning = true;
        public void Stop() => IsRunning = false;
        public void SetDiff(Diff newDiff)
        {
            CurrentGameDiff = newDiff;
        }
        public static string Format(float seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return $"{(int)t.TotalMinutes:00}:{t.Seconds:00}.{t.Milliseconds:000}";
        }
    }
}
