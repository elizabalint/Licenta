using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

namespace History
{
    [RequireComponent(typeof(HistoryLogManager))] 
    [RequireComponent(typeof(HistoryNavigation))] 
    public class HistoryManager : MonoBehaviour
    {
        public const int HISTPRY_CACHE_LIMIT = 100;
        public static HistoryManager instance { get; private set; }
        public List<HistoryState> history = new List<HistoryState>();

        public bool isViewingHistory => navigation.isViewingHistory;

        private HistoryNavigation navigation;
        public HistoryLogManager logManager { get; private set; }
        private void Awake()
        {
            instance = this;
            navigation = GetComponent<HistoryNavigation>();
            logManager = GetComponent<HistoryLogManager>();
        }

        // Start is called before the first frame update
        void Start()
        {
            DialogueSystem.instance.onClear += LogCurrentState;
        }

       public void LogCurrentState()
        {
            HistoryState state = HistoryState.Capture();
            history.Add(state);
            logManager.AddLog(state);


            if(history.Count > HISTPRY_CACHE_LIMIT)
            {
                history.RemoveAt(0);
            }
        }

        public void LoadState(HistoryState state)
        {
            state.Load();

        }
        public void GoForward() => navigation.GoForward();
        public void GoBack() => navigation.GoBack();
    }

}
