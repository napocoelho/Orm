namespace CoreDll.Bindables
{
    public class LazyValueBag
    {
        private object _LOCK_;
        private LazyValue Value { get; set; }
        public bool HasValue { get { return Value != null; } }

        public LazyValueBag()
        {
            _LOCK_ = new object();
            Value = null;
        }

        public LazyValueBag(object lockObject)
        {
            _LOCK_ = lockObject;
            Value = null;
        }

        public void SetValue(LazyValue value)
        {
            lock (_LOCK_)
            {
                Value = value;
            }
        }

        public bool TryGetValue(out LazyValue value)
        {
            lock (_LOCK_)
            {
                if (HasValue)
                {
                    value = Value;
                    Value = null;
                    return true;
                }

                value = null;
                return false;
            }
        }
    }
}