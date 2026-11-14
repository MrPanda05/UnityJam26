using JamUnity.Commons.Components;
using JamUnity.LePlayer;
using JamUnity.Levels;
using JamUnity.UI.MenuUI;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

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

        public float FinalTime { get; private set; }

        [SerializeField]
        private GameObject _gameOverScreen;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerGroup[] _groups;
        [SerializeField] private float _defaultVolume = 25f;
        [SerializeField]
        private PlayMusic _gameOverMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void Start()
        {
            Player.OnFullDeath += HandleGameOver;
            WinFlag.OnGameWon += HandleGameWin;
            foreach (var group in _groups)
            {
                float saved = PlayerPrefs.GetFloat(group.name, _defaultVolume);
                _audioMixer.SetFloat(group.name, VolumeSlider.ScaleDecibels(saved));
            }
        }
        

        private async void HandleGameWin()
        {
            Stop();
            FinalTime = ElapsedTime;
            await SceneLoader.Instance.UnloadSceneAsync("PlayerScene");
            await SceneLoader.Instance.LoadSceneAsync("EndScreen");
            DeloadAllLevels();
        }
        public async void GoToMainMenu()
        {
            await SceneLoader.Instance.SwapAsync("EndScreen","MainMenu");
        }
        private void OnDestroy()
        {
            Player.OnFullDeath -= HandleGameOver;
            WinFlag.OnGameWon -= HandleGameWin;

        }
        private async void HandleGameOver()
        {
            _gameOverMusic.PlayMusicThis();
            Stop();
            FinalTime = ElapsedTime;
            _gameOverScreen.SetActive(true);
            try
            {
                await Task.Delay(3500, destroyCancellationToken);
                _gameOverScreen.SetActive(false);
                DeloadAllLevels();

                await SceneLoader.Instance.LoadSceneAsync("MainMenu");
                await SceneLoader.Instance.UnloadSceneAsync("PlayerScene");
            }
            catch (OperationCanceledException) { }
        }
        private async void DeloadAllLevels()
        {
            var sceneList = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                sceneList.Add(scene.name);
            }
            foreach (var item in sceneList)
            {
                if (item.StartsWith("Level"))
                {
                    // Do something
                    await SceneLoader.Instance.UnloadSceneAsync(item);
                }
            }
        }

        private void Update()
        {
            if (IsRunning && !SceneLoader.Instance.IsLoading)
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
