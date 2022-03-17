using System;


namespace CoreDll.Bindables
{
    public interface IResumeNotifications
    {
        void ResumeNotifications();
    }

    public sealed class ResumeOnDispose<T> : IDisposable
        where T : class, IResumeNotifications
    {
        private T Instance { get; set; }

        public ResumeOnDispose(T resumingInstance)
        {
            Instance = resumingInstance;
        }

        public void Dispose()
        {
            if (Instance != null)
            {
                Instance.ResumeNotifications();
                Instance = null;
            }
        }
    }
}